using System.Globalization;

namespace Datapad.Api.Swapi;

/// <summary>Rules for reading SWAPI's all-strings data.</summary>
internal static class SwapiValue
{
    /// <summary>
    /// Returns null for values SWAPI uses to mean "we do not know" ("unknown", empty, whitespace), otherwise the
    /// trimmed value. "n/a" and "none" are deliberately kept: they are facts (a droid has no gender, a bald
    /// character has no hair colour), not gaps.
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Equals("unknown", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    /// <summary>Extracts the numeric id from a resource URL such as <c>https://swapi.info/api/people/1</c> (or <c>.../1/</c>).</summary>
    public static bool TryGetId(string? url, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var lastSegment = url.TrimEnd('/');
        lastSegment = lastSegment[(lastSegment.LastIndexOf('/') + 1)..];
        return int.TryParse(lastSegment, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }
}
