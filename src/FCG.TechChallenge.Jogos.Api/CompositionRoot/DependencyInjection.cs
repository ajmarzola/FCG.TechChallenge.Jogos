using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;
using FCG.TechChallenge.Jogos.Application.Queries.Jogos;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.TechChallenge.Jogos.Api.CompositionRoot
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR (escaneia os handlers do Application)
            services.AddMediatR(cfg =>
            {
                // Escolha um tipo âncora do assembly Application
                cfg.RegisterServicesFromAssemblyContaining<CreateJogoCommand>();
            });

            services.AddMediatR(cfg =>
            {
                // Escolha um tipo âncora do assembly Application
                cfg.RegisterServicesFromAssemblyContaining<SearchJogosQuery>();
            });

            // TODO: FluentValidation, mapeamentos, behaviors, etc.
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var write = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
            var read  = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");

            var serviceBus = configuration.GetSection("ServiceBus") ?? throw new InvalidOperationException("ServiceBus:ConnectionString não configurado.");

            services.AddScoped<IJogosReadRepository, JogosReadRepository>();
            services.AddSingleton<Domain.Abstractions.IOutbox>(sp => new OutboxStore(write));
            services.AddSingleton(new OutboxRepository(read));
            services.Configure<ServiceBusOptions>(serviceBus);
            services.AddHostedService<OutboxDispatcher>();
            return services;
        }
    }
}