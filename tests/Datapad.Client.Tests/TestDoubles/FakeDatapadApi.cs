using Datapad.Client.Services;
using Datapad.Contracts;

namespace Datapad.Client.Tests.TestDoubles;

/// <summary>Swap the delegates to script responses, failures, or never-completing tasks (to observe loading states).</summary>
internal sealed class FakeDatapadApi : IDatapadApi
{
    public Func<CancellationToken, Task<IReadOnlyList<CharacterSummary>>> Characters { get; set; } =
        _ => Task.FromResult(TestData.Characters);

    public Func<int, CancellationToken, Task<CharacterDetails>> Character { get; set; } =
        (id, _) => Task.FromResult(TestData.Details(id));

    public int CharactersCalls { get; private set; }

    public List<int> RequestedIds { get; } = [];

    public Task<IReadOnlyList<CharacterSummary>> GetCharactersAsync(CancellationToken cancellationToken)
    {
        CharactersCalls++;
        return Characters(cancellationToken);
    }

    public Task<CharacterDetails> GetCharacterAsync(int id, CancellationToken cancellationToken)
    {
        RequestedIds.Add(id);
        return Character(id, cancellationToken);
    }
}
