namespace Datapad.Api.Swapi;

// Upstream shapes, exactly as swapi.info returns them (snake_case JSON, every value a string).
// Only the fields this app reads are declared; normalization happens when mapping to contracts.

internal sealed record SwapiPerson(
    string Name,
    string? Height,
    string? Mass,
    string? HairColor,
    string? SkinColor,
    string? EyeColor,
    string? BirthYear,
    string? Gender,
    string? Homeworld,
    IReadOnlyList<string>? Films,
    IReadOnlyList<string>? Species,
    IReadOnlyList<string>? Vehicles,
    IReadOnlyList<string>? Starships,
    string Url);

internal sealed record SwapiPlanet(
    string Name,
    string? RotationPeriod,
    string? OrbitalPeriod,
    string? Diameter,
    string? Climate,
    string? Gravity,
    string? Terrain,
    string? SurfaceWater,
    string? Population,
    string Url);

internal sealed record SwapiSpecies(
    string Name,
    string? Classification,
    string? Designation,
    string? AverageHeight,
    string? AverageLifespan,
    string? Language,
    string Url);

internal sealed record SwapiFilm(
    string Title,
    int EpisodeId,
    string? ReleaseDate,
    string Url);

internal sealed record SwapiStarship(
    string Name,
    string? Model,
    string? Manufacturer,
    string? Crew,
    string? Passengers,
    string? StarshipClass,
    string Url);

internal sealed record SwapiVehicle(
    string Name,
    string? Model,
    string? Manufacturer,
    string? Crew,
    string? Passengers,
    string? VehicleClass,
    string Url);
