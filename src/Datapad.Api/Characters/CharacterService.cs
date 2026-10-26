using Datapad.Api.Swapi;
using Datapad.Contracts;

namespace Datapad.Api.Characters;

/// <summary>Builds the page's view of a character from cached SWAPI collections.</summary>
internal sealed partial class CharacterService(ISwapiClient swapi, ILogger<CharacterService> logger)
{
    /// <summary>All characters, sorted by name.</summary>
    /// <exception cref="SwapiUnavailableException">The people collection cannot be loaded.</exception>
    public async Task<IReadOnlyList<CharacterSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        var people = await swapi.GetPeopleAsync(cancellationToken);

        return people
            .Select(p => SwapiValue.TryGetId(p.Url, out var id) ? new CharacterSummary(id, p.Name) : null)
            .OfType<CharacterSummary>()
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>One character with everything the details page shows, or null when the id does not exist.</summary>
    /// <remarks>
    /// The person itself is required. Each related collection is optional: if one fails, that section is listed in
    /// <see cref="CharacterDetails.UnavailableSections"/> and the rest of the character is still returned.
    /// </remarks>
    /// <exception cref="SwapiUnavailableException">The people collection cannot be loaded.</exception>
    public async Task<CharacterDetails?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var people = await swapi.GetPeopleAsync(cancellationToken);
        var person = people.FirstOrDefault(p => SwapiValue.TryGetId(p.Url, out var personId) && personId == id);
        if (person is null)
        {
            return null;
        }

        var species = TryLoadAsync(DetailSection.Species, swapi.GetSpeciesAsync, cancellationToken);
        var planets = TryLoadAsync(DetailSection.Homeworld, swapi.GetPlanetsAsync, cancellationToken);
        var films = TryLoadAsync(DetailSection.Films, swapi.GetFilmsAsync, cancellationToken);
        var starships = TryLoadAsync(DetailSection.Starships, swapi.GetStarshipsAsync, cancellationToken);
        var vehicles = TryLoadAsync(DetailSection.Vehicles, swapi.GetVehiclesAsync, cancellationToken);
        await Task.WhenAll(species, planets, films, starships, vehicles);

        var unavailable = new List<DetailSection>();

        return new CharacterDetails(
            id,
            person.Name,
            CharacterMapper.ToAttributes(person),
            Resolve(species.Result, person.Species, unavailable, DetailSection.Species, s => s.Url, CharacterMapper.ToSpecies),
            ResolveHomeworld(planets.Result, person.Homeworld, unavailable),
            ResolveFilms(films.Result, person.Films, unavailable),
            Resolve(starships.Result, person.Starships, unavailable, DetailSection.Starships, s => s.Url, CharacterMapper.ToCraft),
            Resolve(vehicles.Result, person.Vehicles, unavailable, DetailSection.Vehicles, v => v.Url, CharacterMapper.ToCraft),
            unavailable);
    }

    private static List<TOut> Resolve<TIn, TOut>(
        IReadOnlyList<TIn>? collection,
        IReadOnlyList<string>? references,
        List<DetailSection> unavailable,
        DetailSection section,
        Func<TIn, string> urlOf,
        Func<TIn, TOut> map)
    {
        if (collection is null)
        {
            unavailable.Add(section);
            return [];
        }

        // Look up by URL and keep the person's own order. A reference to a record that does not exist is skipped.
        var byUrl = collection.DistinctBy(item => Key(urlOf(item))).ToDictionary(item => Key(urlOf(item)));
        return (references ?? [])
            .Select(url => byUrl.GetValueOrDefault(Key(url)))
            .OfType<TIn>()
            .Select(map)
            .ToList();
    }

    private static PlanetInfo? ResolveHomeworld(IReadOnlyList<SwapiPlanet>? planets, string? homeworldUrl, List<DetailSection> unavailable)
    {
        if (planets is null)
        {
            unavailable.Add(DetailSection.Homeworld);
            return null;
        }

        var planet = homeworldUrl is null ? null : planets.FirstOrDefault(p => Key(p.Url) == Key(homeworldUrl));
        return planet is null ? null : CharacterMapper.ToPlanet(planet);
    }

    private static List<FilmInfo> ResolveFilms(IReadOnlyList<SwapiFilm>? films, IReadOnlyList<string>? references, List<DetailSection> unavailable)
    {
        if (films is null)
        {
            unavailable.Add(DetailSection.Films);
            return [];
        }

        var featured = (references ?? []).Select(Key).ToHashSet();
        return films
            .OrderBy(f => f.EpisodeId)
            .Select(f => new FilmInfo(f.EpisodeId, f.Title, featured.Contains(Key(f.Url))))
            .ToList();
    }

    // SWAPI is inconsistent about trailing slashes; compare URLs without them.
    private static string Key(string url) => url.TrimEnd('/');

    private async Task<IReadOnlyList<T>?> TryLoadAsync<T>(
        DetailSection section,
        Func<CancellationToken, Task<IReadOnlyList<T>>> load,
        CancellationToken cancellationToken)
    {
        try
        {
            return await load(cancellationToken);
        }
        catch (SwapiUnavailableException ex)
        {
            LogSectionUnavailable(logger, section, ex);
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Character section {Section} is unavailable; returning the character without it")]
    private static partial void LogSectionUnavailable(ILogger logger, DetailSection section, Exception exception);
}
