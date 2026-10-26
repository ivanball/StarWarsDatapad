using Datapad.Contracts;

namespace Datapad.Client.Services;

/// <summary>
/// Search runs in the browser: the full list is 82 names, so filtering locally is instant and costs no requests.
/// If the dataset grew past a few thousand, this would move to a server-side, paginated query.
/// </summary>
public static class CharacterFilter
{
    /// <summary>Case-insensitive match on any part of the name; a blank query returns everything.</summary>
    public static IReadOnlyList<CharacterSummary> Apply(IReadOnlyList<CharacterSummary> characters, string? query)
    {
        var term = query?.Trim();
        return string.IsNullOrEmpty(term)
            ? characters
            : characters.Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
