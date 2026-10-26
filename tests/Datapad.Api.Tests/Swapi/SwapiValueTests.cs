using Datapad.Api.Swapi;

namespace Datapad.Api.Tests.Swapi;

public sealed class SwapiValueTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("unknown")]
    [InlineData("Unknown")]
    [InlineData(" unknown ")]
    public void Normalize_MissingSentinel_ReturnsNull(string? value) =>
        Assert.Null(SwapiValue.Normalize(value));

    [Theory]
    [InlineData("n/a", "n/a")]
    [InlineData("none", "none")]
    [InlineData("1,358", "1,358")]
    [InlineData(" blond ", "blond")]
    [InlineData("indefinite", "indefinite")]
    public void Normalize_RealValue_IsKeptTrimmed(string value, string expected) =>
        Assert.Equal(expected, SwapiValue.Normalize(value));

    [Theory]
    [InlineData("https://swapi.info/api/people/1", 1)]
    [InlineData("https://swapi.info/api/people/83/", 83)]
    public void TryGetId_ResourceUrl_ReturnsId(string url, int expected)
    {
        Assert.True(SwapiValue.TryGetId(url, out var id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://swapi.info/api/people")]
    [InlineData("https://swapi.info/api/people/abc")]
    [InlineData("https://swapi.info/api/people/0")]
    [InlineData("https://swapi.info/api/people/-1")]
    public void TryGetId_NotAResourceUrl_ReturnsFalse(string? url) =>
        Assert.False(SwapiValue.TryGetId(url, out _));
}
