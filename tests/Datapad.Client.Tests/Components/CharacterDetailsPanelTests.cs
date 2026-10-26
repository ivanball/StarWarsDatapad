using Bunit;
using Datapad.Client.Components;
using Datapad.Client.Services;
using Datapad.Client.Tests.TestDoubles;
using Datapad.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Datapad.Client.Tests.Components;

public sealed class CharacterDetailsPanelTests : BunitContext
{
    private readonly FakeDatapadApi _api = new();

    public CharacterDetailsPanelTests() => Services.AddSingleton<IDatapadApi>(_api);

    [Fact]
    public void NoSelection_PromptsToSelectAndLoadsNothing()
    {
        var cut = Render<CharacterDetailsPanel>();

        Assert.Contains("Select a character", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(_api.RequestedIds);
    }

    [Fact]
    public void WhileLoading_ShowsLoadingStatus()
    {
        _api.Character = (_, _) => new TaskCompletionSource<CharacterDetails>().Task;

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        Assert.Equal("Loading character...", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void Loaded_RendersEverySectionOfTheWireframe()
    {
        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        Assert.Equal("Luke Skywalker", cut.Find("h2").TextContent);
        Assert.Equal(
            ["Attributes", "Species", "Homeworld", "Films", "Starships", "Vehicles"],
            cut.FindAll("h3").Select(h => h.TextContent));
        Assert.Equal(["Human", "Tatooine"], cut.FindAll(".fact-card__title").Select(t => t.TextContent));
        Assert.Equal(2, cut.FindAll(".craft").Count(c => c.TextContent.Contains("Incom", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Loaded_FormatsValuesWithUnits()
    {
        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        Assert.Equal("172 cm", Row(cut, "Height"));
        Assert.Contains("200,000", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("10,465 km", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("1 / 0", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Films_DimsTheOnesTheCharacterIsNotIn()
    {
        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        var absent = cut.FindAll(".film--absent .film__title").Select(t => t.TextContent);
        Assert.Equal(["The Phantom Menace", "Attack of the Clones"], absent);
        Assert.Equal("Episode IV", cut.FindAll(".film__episode")[3].TextContent);
    }

    [Fact]
    public void MissingData_ShowsUnknownAndExplicitEmptyMessages()
    {
        _api.Character = (_, _) => Task.FromResult(TestData.Luke with
        {
            Attributes = TestData.Luke.Attributes with { Height = null, Mass = null },
            Species = [],
            Homeworld = null,
            Starships = [],
            Vehicles = [],
        });

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        Assert.Equal("Unknown", Row(cut, "Height"));
        Assert.Equal("Unknown", Row(cut, "Mass"));
        Assert.Contains("Species not recorded in SWAPI.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Homeworld unknown.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Luke Skywalker does not pilot any starships.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Luke Skywalker does not pilot any vehicles.", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnavailableSection_ShowsInlineNoticeAndKeepsTheRest()
    {
        _api.Character = (_, _) => Task.FromResult(TestData.Luke with
        {
            Starships = [],
            UnavailableSections = [DetailSection.Starships],
        });

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        var notice = Assert.Single(cut.FindAll(".section-unavailable"));
        Assert.Contains("Starships could not be loaded right now.", notice.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("does not pilot any starships", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("Tatooine", cut.FindAll(".fact-card__title")[1].TextContent);
    }

    [Fact]
    public void LoadFails_ShowsErrorWithRetryThatRecovers()
    {
        var attempts = 0;
        _api.Character = (id, _) => ++attempts == 1
            ? Task.FromException<CharacterDetails>(new DatapadApiException("The Star Wars API is unavailable right now."))
            : Task.FromResult(TestData.Details(id));

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));
        Assert.Contains("unavailable right now", cut.Find("[role=alert]").TextContent, StringComparison.Ordinal);

        cut.Find("[role=alert] button").Click();

        Assert.Equal("Luke Skywalker", cut.Find("h2").TextContent);
    }

    [Fact]
    public void NotFound_ShowsMessageWithoutRetry()
    {
        _api.Character = (_, _) => Task.FromException<CharacterDetails>(
            new DatapadApiException("That character could not be found.", isNotFound: true));

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 999));

        Assert.Contains("could not be found", cut.Find("[role=alert]").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void SlowOlderResponse_NeverOverwritesTheNewerSelection()
    {
        var pending = new Dictionary<int, TaskCompletionSource<CharacterDetails>>();
        _api.Character = (id, _) => (pending[id] = new TaskCompletionSource<CharacterDetails>()).Task;

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));
        cut.Render(p => p.Add(c => c.Id, 3));

        pending[3].SetResult(TestData.Details(3));
        pending[1].SetResult(TestData.Details(1));

        cut.WaitForAssertion(() => Assert.Equal("R2-D2", cut.Find("h2").TextContent));
    }

    [Fact]
    public void SlowOlderFailure_DoesNotReplaceTheNewerSelectionWithAnError()
    {
        var pending = new Dictionary<int, TaskCompletionSource<CharacterDetails>>();
        _api.Character = (id, _) => (pending[id] = new TaskCompletionSource<CharacterDetails>()).Task;

        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));
        cut.Render(p => p.Add(c => c.Id, 3));

        pending[3].SetResult(TestData.Details(3));
        pending[1].SetException(new DatapadApiException("boom"));

        cut.WaitForAssertion(() => Assert.Equal("R2-D2", cut.Find("h2").TextContent));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void SameIdOnRerender_DoesNotReload()
    {
        var cut = Render<CharacterDetailsPanel>(p => p.Add(c => c.Id, 1));

        cut.Render(p => p.Add(c => c.Id, 1));

        Assert.Equal([1], _api.RequestedIds);
    }

    private static string Row(IRenderedComponent<CharacterDetailsPanel> cut, string label) =>
        cut.FindAll(".attributes tr").Single(r => r.QuerySelector("th")!.TextContent == label).QuerySelector("td")!.TextContent;
}
