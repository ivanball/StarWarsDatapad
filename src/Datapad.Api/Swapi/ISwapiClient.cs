namespace Datapad.Api.Swapi;

/// <summary>
/// Reads whole SWAPI collections. swapi.info serves each collection as one unpaginated array and the data is
/// static, so the rest of the app resolves cross-references in memory instead of fetching one URL at a time.
/// </summary>
/// <remarks>Every method throws <see cref="SwapiUnavailableException"/> when the upstream cannot be read.</remarks>
internal interface ISwapiClient
{
    Task<IReadOnlyList<SwapiPerson>> GetPeopleAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwapiPlanet>> GetPlanetsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwapiSpecies>> GetSpeciesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwapiFilm>> GetFilmsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwapiStarship>> GetStarshipsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwapiVehicle>> GetVehiclesAsync(CancellationToken cancellationToken);
}
