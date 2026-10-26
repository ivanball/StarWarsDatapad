using Bunit;
using Datapad.Client.Components;
using Datapad.Client.Services;
using Datapad.Client.Tests.TestDoubles;
using Datapad.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Datapad.Client.Tests.Components;

public sealed class CharacterListTests : BunitContext
{
    private readonly FakeDatapadApi _api = new();

    public CharacterListTests() => Services.AddSingleton<IDatapadApi>(_api);

    [Fact]
    public void WhileLoading_ShowsLoadingStatus()
    {
        _api.Characters = _ => new TaskCompletionSource<IReadOnlyList<CharacterSummary>>().Task;

        var cut = Render<CharacterList>();

        Assert.Equal("Loading characters...", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void Loaded_ListsEveryCharacterAsALink()
    {
        var cut = Render<CharacterList>();

        var links = cut.FindAll("a");
        Assert.Equal(["Anakin Skywalker", "C-3PO", "Luke Skywalker", "R2-D2"], links.Select(a => a.TextContent));
        Assert.Equal("characters/1", links[2].GetAttribute("href"));
    }

    [Fact]
    public void SelectedCharacter_IsMarkedCurrent()
    {
        var cut = Render<CharacterList>(p => p.Add(c => c.SelectedId, 1));

        var current = cut.Find("a[aria-current=page]");
        Assert.Equal("Luke Skywalker", current.TextContent);
        Assert.Contains("selected", current.ClassList);
    }

    [Fact]
    public void Search_FiltersTheList()
    {
        var cut = Render<CharacterList>(p => p.Add(c => c.Search, "sky"));

        Assert.Equal(["Anakin Skywalker", "Luke Skywalker"], cut.FindAll("a").Select(a => a.TextContent));
    }

    [Fact]
    public void Search_NoMatch_SaysSoInsteadOfAnEmptyBox()
    {
        var cut = Render<CharacterList>(p => p.Add(c => c.Search, " Jar Jar "));

        Assert.Empty(cut.FindAll("a"));
        Assert.Equal("No characters match \"Jar Jar\".", cut.Find("[role=status]").TextContent.Trim());
    }

    [Fact]
    public void LoadFails_ShowsErrorWithRetryThatRecovers()
    {
        var attempts = 0;
        _api.Characters = _ => ++attempts == 1
            ? Task.FromException<IReadOnlyList<CharacterSummary>>(new DatapadApiException("The Star Wars API is unavailable right now."))
            : Task.FromResult(TestData.Characters);

        var cut = Render<CharacterList>();

        Assert.Contains("The Star Wars API is unavailable right now.", cut.Find("[role=alert]").TextContent, StringComparison.Ordinal);

        cut.Find("button").Click();

        Assert.Equal(4, cut.FindAll("a").Count);
        Assert.Equal(2, _api.CharactersCalls);
    }
}
