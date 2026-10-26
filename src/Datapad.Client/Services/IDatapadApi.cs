using Datapad.Contracts;

namespace Datapad.Client.Services;

/// <summary>The client's only door to the server. Components depend on this, never on HttpClient.</summary>
/// <remarks>Every method throws <see cref="DatapadApiException"/> with a message fit to show the user.</remarks>
public interface IDatapadApi
{
    Task<IReadOnlyList<CharacterSummary>> GetCharactersAsync(CancellationToken cancellationToken);

    Task<CharacterDetails> GetCharacterAsync(int id, CancellationToken cancellationToken);
}
