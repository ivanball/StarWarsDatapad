using Datapad.Client.Services;

namespace Datapad.Client.Tests.Services;

public sealed class DisplayTests
{
    [Fact]
    public void Text_Null_IsUnknown() => Assert.Equal("Unknown", Display.Text(null));

    [Theory]
    [InlineData("172", " cm", "172 cm")]
    [InlineData("1,358", " kg", "1,358 kg")]
    [InlineData("78.2", " kg", "78.2 kg")]
    [InlineData("10465", " km", "10,465 km")]
    [InlineData("200000", "", "200,000")]
    [InlineData("1", "%", "1%")]
    public void Number_CleanNumber_GetsSeparatorAndUnit(string value, string suffix, string expected) =>
        Assert.Equal(expected, Display.Number(value, suffix));

    [Theory]
    [InlineData("indefinite")]
    [InlineData("n/a")]
    [InlineData("30-165")]
    public void Number_NotANumber_IsShownAsIsWithoutUnit(string value) =>
        Assert.Equal(value, Display.Number(value, " yr"));

    [Fact]
    public void Number_Null_IsUnknownWithoutUnit() => Assert.Equal("Unknown", Display.Number(null, " cm"));

    [Fact]
    public void Pair_JoinsAndFillsGaps() => Assert.Equal("1 / Unknown", Display.Pair("1", null));

    [Theory]
    [InlineData(1, "Episode I")]
    [InlineData(4, "Episode IV")]
    [InlineData(9, "Episode IX")]
    [InlineData(10, "Episode 10")]
    public void Episode_UsesRomanNumerals(int episode, string expected) =>
        Assert.Equal(expected, Display.Episode(episode));
}
