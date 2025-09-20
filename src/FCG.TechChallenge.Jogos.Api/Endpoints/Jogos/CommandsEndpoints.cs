using FCG.TechChallenge.Jogos.Application.Commands.Jogos.CreateJogo;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.DeleteJogo;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogo;
using FCG.TechChallenge.Jogos.Application.Commands.Jogos.UpdateJogoPreco;
using MediatR;

namespace FCG.TechChallenge.Jogos.Api.Endpoints.Jogos
{
    public static class CommandsEndpoints
    {
        public static IEndpointRouteBuilder MapJogoCommands(this IEndpointRouteBuilder app)
        {
            app.MapPost("/jogos", async (CreateJogoCommand cmd, ISender sender) =>
            {
                Guid id = await sender.Send(cmd);
                return Results.Created($"/jogos/{id}", new { id });
            });

            app.MapPut("/jogos", async (UpdateJogoCommand cmd, ISender sender) =>
            {
                Guid id = await sender.Send(cmd);
                return Results.NoContent();
            });

            app.MapPut("/jogos/{id:guid}/preco", async (Guid id, UpdateJogoPrecoCommand body, ISender sender) =>
            {
                await sender.Send(new UpdateJogoPrecoCommand(id, body.NovoPreco));
                return Results.NoContent();
            });

            app.MapDelete("/jogos/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new DeleteJogoCommand(id));
                return Results.NoContent();
            });

            return app;
        }
    }
}