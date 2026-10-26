using System.Net;
using Datapad.Api.Swapi;
using Datapad.Api.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Datapad.Api.Tests.Swapi;

public sealed class SwapiClientTests
{
    private static readonly Uri BaseAddress = new("https://swapi.test/api/");

    [Fact]
    public async Task GetPeopleAsync_RecordedResponse_MapsSnakeCaseFields()
    {
        var client = CreateClient(Fixtures.Handler());

        var people = await client.GetPeopleAsync(TestContext.Current.CancellationToken);

        var luke = Assert.Single(people, p => p.Name == "Luke Skywalker");
        Assert.Equal("19BBY", luke.BirthYear);
        Assert.Equal("blond", luke.HairColor);
        Assert.Equal("https://swapi.info/api/planets/1", luke.Homeworld);
        Assert.Equal(4, luke.Films!.Count);
        Assert.Empty(luke.Species!);
    }

    [Fact]
    public async Task GetFilmsAsync_RecordedResponse_MapsEpisodeId()
    {
        var client = CreateClient(Fixtures.Handler());

        var films = await client.GetFilmsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(6, films.Count);
        Assert.Equal(4, Assert.Single(films, f => f.Title == "A New Hope").EpisodeId);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task GetPeopleAsync_ErrorStatus_ThrowsSwapiUnavailable(HttpStatusCode status)
    {
        var client = CreateClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(status)));

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => client.GetPeopleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("<html>maintenance</html>")]
    [InlineData("{\"detail\":\"not a list\"}")]
    [InlineData("null")]
    public async Task GetPeopleAsync_UnreadableBody_ThrowsSwapiUnavailable(string body)
    {
        var client = CreateClient(new StubHttpMessageHandler(_ => Fixtures.Json(body)));

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => client.GetPeopleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetPeopleAsync_NetworkFailure_ThrowsSwapiUnavailable()
    {
        var client = CreateClient(new StubHttpMessageHandler(_ => throw new HttpRequestException("connection refused")));

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => client.GetPeopleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetPeopleAsync_CallerCancels_PropagatesCancellationNotUpstreamFailure()
    {
        var client = CreateClient(Fixtures.Handler());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetPeopleAsync(cts.Token));

        Assert.IsNotType<SwapiUnavailableException>(ex);
    }

    private static SwapiClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = BaseAddress }, NullLogger<SwapiClient>.Instance);
}
