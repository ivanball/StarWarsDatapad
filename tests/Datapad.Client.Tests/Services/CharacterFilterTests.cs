using Datapad.Client.Services;
using Datapad.Client.Tests.TestDoubles;

namespace Datapad.Client.Tests.Services;

public sealed class CharacterFilterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Apply_BlankQuery_ReturnsEveryone(string? query) =>
        Assert.Equal(TestData.Characters, CharacterFilter.Apply(TestData.Characters, query));

    [Theory]
    [InlineData("sky")]
    [InlineData("SKY")]
    [InlineData("  skywalker ")]
    public void Apply_MatchesAnyPartOfTheNameIgnoringCaseAndSurroundingSpace(string query) =>
        Assert.Equal(
            ["Anakin Skywalker", "Luke Skywalker"],
            CharacterFilter.Apply(TestData.Characters, query).Select(c => c.Name));

    [Fact]
    public void Apply_PunctuationInName_Matches() =>
        Assert.Equal("R2-D2", Assert.Single(CharacterFilter.Apply(TestData.Characters, "r2-")).Name);

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty() =>
        Assert.Empty(CharacterFilter.Apply(TestData.Characters, "Jar Jar"));
}
