using Datapad.Api.Characters;
using Datapad.Api.Swapi;
using Datapad.Api.Tests.TestDoubles;
using Datapad.Contracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace Datapad.Api.Tests.Characters;

public sealed class CharacterServiceTests
{
    private const int Luke = 1;
    private const int R2D2 = 3;
    private const int ArvelCrynyd = 29;

    private readonly FakeSwapiClient _swapi = new();

    [Fact]
    public async Task GetAllAsync_ReturnsEveryCharacterSortedByName()
    {
        var characters = await CreateService().GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [new(ArvelCrynyd, "Arvel Crynyd"), new(Luke, "Luke Skywalker"), new CharacterSummary(R2D2, "R2-D2")],
            characters);
    }

    [Fact]
    public async Task GetAllAsync_PeopleUnavailable_Throws()
    {
        _swapi.People = null;

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => CreateService().GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var details = await CreateService().GetByIdAsync(17, TestContext.Current.CancellationToken);

        Assert.Null(details);
    }

    [Fact]
    public async Task GetByIdAsync_Luke_ResolvesAttributesHomeworldAndCraftInReferenceOrder()
    {
        var luke = await GetAsync(Luke);

        Assert.Equal("Luke Skywalker", luke.Name);
        Assert.Equal(new CharacterAttributes("19BBY", "male", "172", "77", "blue", "blond", "fair"), luke.Attributes);
        Assert.Equal("Tatooine", luke.Homeworld?.Name);
        Assert.Equal("200000", luke.Homeworld?.Population);
        Assert.Equal(["X-wing", "Imperial shuttle"], luke.Starships.Select(s => s.Name));
        Assert.Equal("Starfighter", luke.Starships[0].Class);
        Assert.Equal(["Snowspeeder", "Imperial Speeder Bike"], luke.Vehicles.Select(v => v.Name));
        Assert.Empty(luke.UnavailableSections);
    }

    [Fact]
    public async Task GetByIdAsync_EmptySpeciesInSwapi_ReturnsNoSpeciesRatherThanGuessing()
    {
        var luke = await GetAsync(Luke);

        Assert.Empty(luke.Species);
        Assert.DoesNotContain(DetailSection.Species, luke.UnavailableSections);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAllFilmsInEpisodeOrderFlaggingTheCharactersOnes()
    {
        var luke = await GetAsync(Luke);

        Assert.Equal([1, 2, 3, 4, 5, 6], luke.Films.Select(f => f.EpisodeId));
        Assert.Equal([false, false, true, true, true, true], luke.Films.Select(f => f.FeaturesCharacter));
    }

    [Fact]
    public async Task GetByIdAsync_Droid_ResolvesSpeciesAndEmptyCraft()
    {
        var r2 = await GetAsync(R2D2);

        Assert.Equal("Droid", Assert.Single(r2.Species).Name);
        Assert.Equal("n/a", r2.Species[0].Language);
        Assert.Equal("n/a", r2.Attributes.HairColor);
        Assert.Empty(r2.Starships);
        Assert.Empty(r2.Vehicles);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownValues_BecomeNullAndPlaceholderPlanetIsNoHomeworld()
    {
        var arvel = await GetAsync(ArvelCrynyd);

        Assert.Null(arvel.Attributes.Height);
        Assert.Null(arvel.Attributes.Mass);
        Assert.Null(arvel.Homeworld);
        Assert.DoesNotContain(DetailSection.Homeworld, arvel.UnavailableSections);
    }

    [Fact]
    public async Task GetByIdAsync_RelatedCollectionDown_ReturnsCharacterAndNamesTheMissingSection()
    {
        _swapi.Starships = null;
        _swapi.Planets = null;

        var luke = await GetAsync(Luke);

        Assert.Equal([DetailSection.Homeworld, DetailSection.Starships], luke.UnavailableSections);
        Assert.Null(luke.Homeworld);
        Assert.Empty(luke.Starships);
        Assert.Equal(2, luke.Vehicles.Count);
        Assert.Equal(6, luke.Films.Count);
    }

    [Fact]
    public async Task GetByIdAsync_PeopleUnavailable_Throws()
    {
        _swapi.People = null;

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => CreateService().GetByIdAsync(Luke, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetByIdAsync_ReferenceToMissingRecord_IsSkipped()
    {
        var luke = _swapi.People!.Single(p => p.Name == "Luke Skywalker");
        _swapi.People = [luke with { Starships = [.. luke.Starships!, "https://swapi.info/api/starships/9999"] }];

        var details = await GetAsync(Luke);

        Assert.Equal(2, details.Starships.Count);
    }

    [Fact]
    public async Task GetByIdAsync_TrailingSlashInReference_StillResolves()
    {
        var luke = _swapi.People!.Single(p => p.Name == "Luke Skywalker");
        _swapi.People = [luke with { Homeworld = luke.Homeworld + "/" }];

        var details = await GetAsync(Luke);

        Assert.Equal("Tatooine", details.Homeworld?.Name);
    }

    private async Task<CharacterDetails> GetAsync(int id)
    {
        var details = await CreateService().GetByIdAsync(id, TestContext.Current.CancellationToken);
        Assert.NotNull(details);
        return details;
    }

    private CharacterService CreateService() => new(_swapi, NullLogger<CharacterService>.Instance);
}
