using Datapad.Contracts;

namespace Datapad.Client.Tests.TestDoubles;

internal static class TestData
{
    public static IReadOnlyList<CharacterSummary> Characters { get; } =
    [
        new(11, "Anakin Skywalker"),
        new(2, "C-3PO"),
        new(1, "Luke Skywalker"),
        new(3, "R2-D2"),
    ];

    public static CharacterDetails Details(int id) => id switch
    {
        1 => Luke,
        _ => Luke with { Id = id, Name = Characters.Single(c => c.Id == id).Name },
    };

    public static CharacterDetails Luke { get; } = new(
        Id: 1,
        Name: "Luke Skywalker",
        Attributes: new("19BBY", "male", "172", "77", "blue", "blond", "fair"),
        Species: [new("Human", "mammal", "sentient", "180", "120", "Galactic Basic")],
        Homeworld: new("Tatooine", "200000", "desert", "arid", "1", "10465", "23", "304", "1 standard"),
        Films:
        [
            new(1, "The Phantom Menace", false),
            new(2, "Attack of the Clones", false),
            new(3, "Revenge of the Sith", true),
            new(4, "A New Hope", true),
            new(5, "The Empire Strikes Back", true),
            new(6, "Return of the Jedi", true),
        ],
        Starships:
        [
            new("X-wing", "Starfighter", "1", "0", "T-65 X-wing", "Incom Corporation"),
            new("Imperial shuttle", "Armed government transport", "6", "20", "Lambda-class T-4a shuttle", "Sienar Fleet Systems"),
        ],
        Vehicles:
        [
            new("Snowspeeder", "airspeeder", "2", "0", "t-47 airspeeder", "Incom corporation"),
            new("Imperial Speeder Bike", "speeder", "1", "1", "74-Z speeder bike", "Aratech Repulsor Company"),
        ],
        UnavailableSections: []);
}
