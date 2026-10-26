using System.Text.Json;
using Datapad.Api.Swapi;

namespace Datapad.Api.Tests.TestDoubles;

/// <summary>
/// In-memory <see cref="ISwapiClient"/> seeded from the recorded fixtures. Set a collection to <c>null</c> to make
/// that call throw <see cref="SwapiUnavailableException"/>, the way the real client reports an upstream failure.
/// </summary>
internal sealed class FakeSwapiClient : ISwapiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public List<SwapiPerson>? People { get; set; } = Load<SwapiPerson>("people");

    public List<SwapiPlanet>? Planets { get; set; } = Load<SwapiPlanet>("planets");

    public List<SwapiSpecies>? Species { get; set; } = Load<SwapiSpecies>("species");

    public List<SwapiFilm>? Films { get; set; } = Load<SwapiFilm>("films");

    public List<SwapiStarship>? Starships { get; set; } = Load<SwapiStarship>("starships");

    public List<SwapiVehicle>? Vehicles { get; set; } = Load<SwapiVehicle>("vehicles");

    public int PeopleCalls { get; private set; }

    public Task<IReadOnlyList<SwapiPerson>> GetPeopleAsync(CancellationToken cancellationToken)
    {
        PeopleCalls++;
        return Result(People, "people");
    }

    public Task<IReadOnlyList<SwapiPlanet>> GetPlanetsAsync(CancellationToken cancellationToken) => Result(Planets, "planets");

    public Task<IReadOnlyList<SwapiSpecies>> GetSpeciesAsync(CancellationToken cancellationToken) => Result(Species, "species");

    public Task<IReadOnlyList<SwapiFilm>> GetFilmsAsync(CancellationToken cancellationToken) => Result(Films, "films");

    public Task<IReadOnlyList<SwapiStarship>> GetStarshipsAsync(CancellationToken cancellationToken) => Result(Starships, "starships");

    public Task<IReadOnlyList<SwapiVehicle>> GetVehiclesAsync(CancellationToken cancellationToken) => Result(Vehicles, "vehicles");

    private static List<T> Load<T>(string resource) =>
        JsonSerializer.Deserialize<List<T>>(Fixtures.Read(resource), JsonOptions)!;

    private static Task<IReadOnlyList<T>> Result<T>(List<T>? items, string resource) =>
        items is null
            ? Task.FromException<IReadOnlyList<T>>(new SwapiUnavailableException($"{resource} is down"))
            : Task.FromResult<IReadOnlyList<T>>(items);
}
