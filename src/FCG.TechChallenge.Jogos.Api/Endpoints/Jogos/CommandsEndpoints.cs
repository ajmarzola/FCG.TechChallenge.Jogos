using MediatR;

using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;

namespace FCG.TechChallenge.Jogos.Api.Endpoints.Jogos
{
    public static class CommandsEndpoints
    {
        public static IEndpointRouteBuilder MapJogoCommands(this IEndpointRouteBuilder app)
        {
            app.MapPost("/jogos", async (CreateJogoCommand cmd, ISender sender) =>
            {
                var id = await sender.Send(cmd);
                return Results.Created($"/jogos/{id}", new { id });
            });

            // PUT /jogos/{id}/preco
            // DELETE /jogos/{id}
            return app;
        }
    }
}