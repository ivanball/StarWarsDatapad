using Datapad.Api.Swapi;
using Datapad.Contracts;

namespace Datapad.Api.Characters;

/// <summary>Maps SWAPI records to contracts, applying <see cref="SwapiValue.Normalize"/> to every free-text value.</summary>
internal static class CharacterMapper
{
    public static CharacterAttributes ToAttributes(SwapiPerson person) => new(
        BirthYear: SwapiValue.Normalize(person.BirthYear),
        Gender: SwapiValue.Normalize(person.Gender),
        Height: SwapiValue.Normalize(person.Height),
        Mass: SwapiValue.Normalize(person.Mass),
        EyeColor: SwapiValue.Normalize(person.EyeColor),
        HairColor: SwapiValue.Normalize(person.HairColor),
        SkinColor: SwapiValue.Normalize(person.SkinColor));

    public static SpeciesInfo ToSpecies(SwapiSpecies species) => new(
        Name: species.Name,
        Classification: SwapiValue.Normalize(species.Classification),
        Designation: SwapiValue.Normalize(species.Designation),
        AverageHeight: SwapiValue.Normalize(species.AverageHeight),
        AverageLifespan: SwapiValue.Normalize(species.AverageLifespan),
        Language: SwapiValue.Normalize(species.Language));

    /// <summary>
    /// Returns null for SWAPI's placeholder planet (named "unknown", every field unknown), so the UI shows
    /// "homeworld unknown" rather than a card full of blanks.
    /// </summary>
    public static PlanetInfo? ToPlanet(SwapiPlanet planet)
    {
        var name = SwapiValue.Normalize(planet.Name);
        return name is null
            ? null
            : new PlanetInfo(
                Name: name,
                Population: SwapiValue.Normalize(planet.Population),
                Terrain: SwapiValue.Normalize(planet.Terrain),
                Climate: SwapiValue.Normalize(planet.Climate),
                SurfaceWater: SwapiValue.Normalize(planet.SurfaceWater),
                Diameter: SwapiValue.Normalize(planet.Diameter),
                RotationPeriod: SwapiValue.Normalize(planet.RotationPeriod),
                OrbitalPeriod: SwapiValue.Normalize(planet.OrbitalPeriod),
                Gravity: SwapiValue.Normalize(planet.Gravity));
    }

    public static CraftInfo ToCraft(SwapiStarship starship) => new(
        Name: starship.Name,
        Class: SwapiValue.Normalize(starship.StarshipClass),
        Crew: SwapiValue.Normalize(starship.Crew),
        Passengers: SwapiValue.Normalize(starship.Passengers),
        Model: SwapiValue.Normalize(starship.Model),
        Manufacturer: SwapiValue.Normalize(starship.Manufacturer));

    public static CraftInfo ToCraft(SwapiVehicle vehicle) => new(
        Name: vehicle.Name,
        Class: SwapiValue.Normalize(vehicle.VehicleClass),
        Crew: SwapiValue.Normalize(vehicle.Crew),
        Passengers: SwapiValue.Normalize(vehicle.Passengers),
        Model: SwapiValue.Normalize(vehicle.Model),
        Manufacturer: SwapiValue.Normalize(vehicle.Manufacturer));
}
