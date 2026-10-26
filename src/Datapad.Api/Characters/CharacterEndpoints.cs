using Datapad.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Datapad.Api.Characters;

internal static class CharacterEndpoints
{
    public static IEndpointRouteBuilder MapCharacterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/characters").WithTags("Characters");

        group.MapGet("/", async (CharacterService characters, CancellationToken cancellationToken) =>
            TypedResults.Ok(await characters.GetAllAsync(cancellationToken)));

        group.MapGet("/{id:int}", async Task<Results<Ok<CharacterDetails>, ProblemHttpResult>> (
            int id,
            CharacterService characters,
            CancellationToken cancellationToken) =>
        {
            var details = await characters.GetByIdAsync(id, cancellationToken);
            return details is null
                ? TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Character not found",
                    detail: $"There is no character with id {id}.")
                : TypedResults.Ok(details);
        });

        return endpoints;
    }
}
