using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;

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

            // TODO: FluentValidation, mapeamentos, behaviors, etc.
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Exemplo com EF Core (ajuste para o que você usa)
            // var connectionString = configuration.GetConnectionString("Default")!;
            // services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

            // TODO: registrar repositórios, UoW, serviços de infra, caches, etc.
            // services.AddScoped<IJogoRepository, JogoRepository>();

            return services;
        }
    }
}
