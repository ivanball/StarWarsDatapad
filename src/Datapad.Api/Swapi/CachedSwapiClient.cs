using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Datapad.Api.Swapi;

/// <summary>
/// Caches each successfully loaded collection. A failure is never cached, so the next request retries upstream.
/// </summary>
/// <remarks>
/// Registered per request rather than as a singleton: it wraps the typed <see cref="SwapiClient"/>, and holding a
/// typed HttpClient in a singleton would pin one handler forever and defeat IHttpClientFactory's rotation.
/// Concurrent cold-cache requests may each go upstream once; with six small collections that is acceptable.
/// </remarks>
internal sealed class CachedSwapiClient(ISwapiClient inner, IMemoryCache cache, IOptions<SwapiOptions> options) : ISwapiClient
{
    public Task<IReadOnlyList<SwapiPerson>> GetPeopleAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("people", inner.GetPeopleAsync, cancellationToken);

    public Task<IReadOnlyList<SwapiPlanet>> GetPlanetsAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("planets", inner.GetPlanetsAsync, cancellationToken);

    public Task<IReadOnlyList<SwapiSpecies>> GetSpeciesAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("species", inner.GetSpeciesAsync, cancellationToken);

    public Task<IReadOnlyList<SwapiFilm>> GetFilmsAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("films", inner.GetFilmsAsync, cancellationToken);

    public Task<IReadOnlyList<SwapiStarship>> GetStarshipsAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("starships", inner.GetStarshipsAsync, cancellationToken);

    public Task<IReadOnlyList<SwapiVehicle>> GetVehiclesAsync(CancellationToken cancellationToken) =>
        GetOrLoadAsync("vehicles", inner.GetVehiclesAsync, cancellationToken);

    private async Task<IReadOnlyList<T>> GetOrLoadAsync<T>(
        string resource,
        Func<CancellationToken, Task<IReadOnlyList<T>>> load,
        CancellationToken cancellationToken)
    {
        var key = $"swapi:{resource}";
        if (cache.TryGetValue(key, out IReadOnlyList<T>? cached) && cached is not null)
        {
            return cached;
        }

        var items = await load(cancellationToken);
        cache.Set(key, items, options.Value.CacheDuration);
        return items;
    }
}
