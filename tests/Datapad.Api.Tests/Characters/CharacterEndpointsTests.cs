using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Datapad.Api.Swapi;
using Datapad.Api.Tests.TestDoubles;
using Datapad.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Datapad.Api.Tests.Characters;

public sealed class CharacterEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly FakeSwapiClient _swapi = new();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetCharacters_ReturnsSummaries()
    {
        using var client = CreateClient();

        var characters = await client.GetFromJsonAsync<List<CharacterSummary>>(Url("/api/characters"), Ct);

        Assert.Equal(["Arvel Crynyd", "Luke Skywalker", "R2-D2"], characters!.Select(c => c.Name));
    }

    [Fact]
    public async Task GetCharacter_Known_ReturnsDetails()
    {
        using var client = CreateClient();

        var luke = await client.GetFromJsonAsync<CharacterDetails>(Url("/api/characters/1"), Ct);

        Assert.Equal("Luke Skywalker", luke!.Name);
        Assert.Equal("Tatooine", luke.Homeworld?.Name);
    }

    [Fact]
    public async Task GetCharacter_Unknown_Returns404Problem()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(Url("/api/characters/17"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/api/characters")]
    [InlineData("/api/characters/1")]
    public async Task SwapiDown_Returns503Problem(string path)
    {
        _swapi.People = null;
        using var client = CreateClient();

        var response = await client.GetAsync(Url(path), Ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
        Assert.Equal("Star Wars API unavailable", problem!.Title);
    }

    [Fact]
    public async Task GetCharacter_RelatedCollectionDown_Returns200WithSectionNamedAsString()
    {
        _swapi.Starships = null;
        using var client = CreateClient();

        var json = await client.GetStringAsync(Url("/api/characters/1"), Ct);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Starships", doc.RootElement.GetProperty("unavailableSections")[0].GetString());
    }

    [Fact]
    public async Task UnknownApiRoute_Returns404ProblemNotTheClientApp()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(Url("/api/planets"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetCharacter_ThroughRealHttpPipeline_ResolvesFromRecordedUpstream()
    {
        // Keeps the real typed client, resilience handler and cache; only the network is replaced.
        var upstream = Fixtures.Handler();
        using var client = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(services =>
                services.AddHttpClient<SwapiClient>().ConfigurePrimaryHttpMessageHandler(() => upstream)))
            .CreateClient();

        var luke = await client.GetFromJsonAsync<CharacterDetails>(Url("/api/characters/1"), Ct);
        await client.GetFromJsonAsync<CharacterDetails>(Url("/api/characters/1"), Ct);

        Assert.Equal("Tatooine", luke!.Homeworld?.Name);
        Assert.Equal(["X-wing", "Imperial shuttle"], luke.Starships.Select(s => s.Name));
        Assert.Equal(6, upstream.RequestCount); // six collections once; the second request is served from cache
    }

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private HttpClient CreateClient() =>
        factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(services => services.AddScoped<ISwapiClient>(_ => _swapi)))
            .CreateClient();
}
