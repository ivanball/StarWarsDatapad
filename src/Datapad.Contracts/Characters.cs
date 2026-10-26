using System.Text.Json.Serialization;

namespace Datapad.Contracts;

// The API's public shape, shared with the Blazor client. Values that SWAPI reports as unknown arrive as null;
// units and number formatting are the UI's job, so these stay raw (for example Height "172", Mass "1,358").

public sealed record CharacterSummary(int Id, string Name);

public sealed record CharacterDetails(
    int Id,
    string Name,
    CharacterAttributes Attributes,
    IReadOnlyList<SpeciesInfo> Species,
    PlanetInfo? Homeworld,
    IReadOnlyList<FilmInfo> Films,
    IReadOnlyList<CraftInfo> Starships,
    IReadOnlyList<CraftInfo> Vehicles,
    IReadOnlyList<DetailSection> UnavailableSections);

public sealed record CharacterAttributes(
    string? BirthYear,
    string? Gender,
    string? Height,
    string? Mass,
    string? EyeColor,
    string? HairColor,
    string? SkinColor);

public sealed record SpeciesInfo(
    string Name,
    string? Classification,
    string? Designation,
    string? AverageHeight,
    string? AverageLifespan,
    string? Language);

public sealed record PlanetInfo(
    string Name,
    string? Population,
    string? Terrain,
    string? Climate,
    string? SurfaceWater,
    string? Diameter,
    string? RotationPeriod,
    string? OrbitalPeriod,
    string? Gravity);

/// <summary>Every film is returned, in episode order; <see cref="FeaturesCharacter"/> marks the ones the character is in.</summary>
public sealed record FilmInfo(int EpisodeId, string Title, bool FeaturesCharacter);

/// <summary>A starship or vehicle the character pilots.</summary>
public sealed record CraftInfo(
    string Name,
    string? Class,
    string? Crew,
    string? Passengers,
    string? Model,
    string? Manufacturer);

/// <summary>A related section the API could not load. The rest of the character is still returned.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DetailSection>))]
public enum DetailSection
{
    Species,
    Homeworld,
    Films,
    Starships,
    Vehicles,
}
