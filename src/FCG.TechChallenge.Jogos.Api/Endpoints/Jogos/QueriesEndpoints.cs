using MediatR;

using FCG.TechChallenge.Jogos.Application.Queries.Jogos;

namespace FCG.TechChallenge.Jogos.Api.Endpoints.Jogos
{
    public static class QueriesEndpoints
    {
        public static IEndpointRouteBuilder MapJogoQueries(this IEndpointRouteBuilder app)
        {
            app.MapGet("/jogos/{id:guid}", async (Guid id, ISender s) =>
                (await s.Send(new GetJogoByIdQuery(id))) is { } dto ? Results.Ok(dto) : Results.NotFound());

            app.MapGet("/jogos", async ([AsParameters] SearchJogosQuery q, ISender s) =>
                Results.Ok(await s.Send(q)));

            return app;
        }
    }
}