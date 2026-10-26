using System.Net;
using System.Text;

namespace Datapad.Api.Tests.TestDoubles;

/// <summary>Recorded swapi.info responses (a trimmed subset: Luke, R2-D2 and Arvel Crynyd plus what they reference).</summary>
internal static class Fixtures
{
    public static string Read(string resource) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", $"{resource}.json"));

    /// <summary>A handler that answers <c>GET .../{resource}</c> from the fixture files, like swapi.info would.</summary>
    public static StubHttpMessageHandler Handler() => new(request =>
    {
        var resource = request.RequestUri!.Segments[^1].TrimEnd('/');
        return File.Exists(Path.Combine(AppContext.BaseDirectory, "Fixtures", $"{resource}.json"))
            ? Json(Read(resource))
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    });

    public static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
}
