using FCG.TechChallenge.Jogos.Application.Abstractions;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;
using FCG.TechChallenge.Jogos.Application.Queries.Jogos;
using FCG.TechChallenge.Jogos.Infrastructure.Messaging.ServiceBus;
using FCG.TechChallenge.Jogos.Infrastructure.Outbox;
using FCG.TechChallenge.Jogos.Infrastructure.ReadModels.Sql;

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
            var cs = configuration.GetConnectionString("Postgres") ?? configuration["Postgres"];

            services.AddScoped<IJogosReadRepository, JogosReadRepository>();
            services.AddSingleton<Domain.Abstractions.IOutbox>(sp => new OutboxStore(cs));
            services.AddSingleton(new OutboxRepository(cs));
            services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));
            services.AddHostedService<OutboxDispatcher>();
            return services;
        }
    }
}
