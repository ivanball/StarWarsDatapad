using System.Net;
using System.Text;
using Datapad.Client.Services;

namespace Datapad.Client.Tests.Services;

public sealed class DatapadApiClientTests
{
    [Fact]
    public async Task GetCharactersAsync_Ok_ReturnsSummaries()
    {
        var api = CreateClient(_ => Json("""[{"id":1,"name":"Luke Skywalker"}]"""));

        var characters = await api.GetCharactersAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Luke Skywalker", Assert.Single(characters).Name);
    }

    [Fact]
    public async Task GetCharacterAsync_Ok_ReadsStringEnumSections()
    {
        var api = CreateClient(_ => Json("""
            {"id":1,"name":"Luke Skywalker",
             "attributes":{"birthYear":null,"gender":null,"height":null,"mass":null,"eyeColor":null,"hairColor":null,"skinColor":null},
             "species":[],"homeworld":null,"films":[],"starships":[],"vehicles":[],
             "unavailableSections":["Starships"]}
            """));

        var luke = await api.GetCharacterAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(Contracts.DetailSection.Starships, Assert.Single(luke.UnavailableSections));
    }

    [Fact]
    public async Task GetCharacterAsync_NotFound_ThrowsNotFound()
    {
        var api = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<DatapadApiException>(
            () => api.GetCharacterAsync(17, TestContext.Current.CancellationToken));

        Assert.True(ex.IsNotFound);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "The Star Wars API is unavailable right now. Please try again shortly.")]
    [InlineData(HttpStatusCode.InternalServerError, "Something went wrong loading the character list.")]
    public async Task GetCharactersAsync_ErrorStatus_ThrowsWithUserFacingMessage(HttpStatusCode status, string message)
    {
        var api = CreateClient(_ => new HttpResponseMessage(status));

        var ex = await Assert.ThrowsAsync<DatapadApiException>(
            () => api.GetCharactersAsync(TestContext.Current.CancellationToken));

        Assert.Equal(message, ex.Message);
        Assert.False(ex.IsNotFound);
    }

    [Fact]
    public async Task GetCharactersAsync_ServerUnreachable_ThrowsWithUserFacingMessage()
    {
        var api = CreateClient(_ => throw new HttpRequestException("connection refused"));

        var ex = await Assert.ThrowsAsync<DatapadApiException>(
            () => api.GetCharactersAsync(TestContext.Current.CancellationToken));

        Assert.StartsWith("Could not reach the Datapad server", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetCharactersAsync_HtmlInsteadOfJson_ThrowsWithUserFacingMessage()
    {
        var api = CreateClient(_ => Json("<!DOCTYPE html><html></html>"));

        await Assert.ThrowsAsync<DatapadApiException>(
            () => api.GetCharactersAsync(TestContext.Current.CancellationToken));
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private static DatapadApiClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        new(new HttpClient(new StubHandler(respond)) { BaseAddress = new Uri("http://localhost/") });

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
