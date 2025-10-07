using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogoPreco;
using FCG.TechChallenge.Jogos.Application.Queries.Jogos;
using FCG.TechChallenge.Jogos.Infrastructure.Config.Options;
using FCG.TechChallenge.Jogos.Infrastructure.EventStore;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace FCG.TechChallenge.Jogos.Api.CompositionRoot
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateJogoCommand>());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SearchJogosQuery>());

            // FluentValidation + Behaviors (se já tiver os validators)
            services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(CreateJogoCommand))!);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CreateJogoValidator));
            // (Opcional) Logging/Tracing behaviors, se quiser
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UpdateJogoValidator));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UpdateJogoPrecoValidator));
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<SqlOptions>(opt =>
            {
                var cs = configuration.GetConnectionString("Postgres") ?? configuration["ConnectionStrings:Postgres"];

                if (string.IsNullOrWhiteSpace(cs))
                {
                    throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
                }

                opt.ConnectionString = cs;
            });

            services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));
            services.Configure<ElasticOptions>(configuration.GetSection("Elastic"));

            // Repositório de leitura
            services.AddScoped<IJogosReadRepository, JogosReadRepository>();

            // EventStore (para comandos)
            services.AddScoped<IEventStore, PgEventStore>();

            // Outbox: usar OutboxStore + Adapter p/ Application.Abstractions.IOutbox
            services.AddSingleton(sp =>
            {
                var cs = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SqlOptions>>().Value.ConnectionString;
                return new OutboxStore(cs);
            });

            services.AddSingleton<ApplicationOutboxAdapter>(); // wrapper
            services.AddSingleton<IOutbox>(sp => sp.GetRequiredService<ApplicationOutboxAdapter>());

            // Outbox repo + dispatcher (publica no Service Bus)
            services.AddSingleton(sp =>
            {
                var cs = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SqlOptions>>().Value.ConnectionString;
                return new OutboxRepository(cs);
            });
            
            services.AddHostedService<OutboxDispatcher>();
            return services;
        }
    }
}