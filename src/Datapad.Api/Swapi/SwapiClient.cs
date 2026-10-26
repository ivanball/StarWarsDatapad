using System.Net.Http.Json;
using System.Text.Json;

namespace Datapad.Api.Swapi;

/// <summary>Typed HTTP client for swapi.info. Retries, timeouts and the circuit breaker live in the HttpClient pipeline.</summary>
internal sealed partial class SwapiClient(HttpClient httpClient, ILogger<SwapiClient> logger) : ISwapiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public Task<IReadOnlyList<SwapiPerson>> GetPeopleAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiPerson>("people", cancellationToken);

    public Task<IReadOnlyList<SwapiPlanet>> GetPlanetsAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiPlanet>("planets", cancellationToken);

    public Task<IReadOnlyList<SwapiSpecies>> GetSpeciesAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiSpecies>("species", cancellationToken);

    public Task<IReadOnlyList<SwapiFilm>> GetFilmsAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiFilm>("films", cancellationToken);

    public Task<IReadOnlyList<SwapiStarship>> GetStarshipsAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiStarship>("starships", cancellationToken);

    public Task<IReadOnlyList<SwapiVehicle>> GetVehiclesAsync(CancellationToken cancellationToken) =>
        GetCollectionAsync<SwapiVehicle>("vehicles", cancellationToken);

    private async Task<IReadOnlyList<T>> GetCollectionAsync<T>(string resource, CancellationToken cancellationToken)
    {
        try
        {
            var items = await httpClient.GetFromJsonAsync<List<T>>(new Uri(resource, UriKind.Relative), JsonOptions, cancellationToken);
            return items ?? throw new SwapiUnavailableException($"SWAPI returned an empty body for '{resource}'.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The caller gave up (for example the browser navigated away); that is not an upstream failure.
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            // OperationCanceledException here, with the caller's token still live, is the resilience timeout.
            LogUpstreamFailure(logger, resource, ex);
            throw new SwapiUnavailableException($"Could not read '{resource}' from SWAPI.", ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SWAPI request for {Resource} failed")]
    private static partial void LogUpstreamFailure(ILogger logger, string resource, Exception exception);
}
