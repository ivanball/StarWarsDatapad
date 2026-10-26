using System.Globalization;

namespace Datapad.Client.Services;

/// <summary>
/// Turns the API's raw values into display text. The API sends null for "unknown" and leaves numbers as SWAPI wrote
/// them, because SWAPI mixes numbers with words ("indefinite", "n/a") and ranges ("30-165"); only clean numbers get
/// a thousands separator and a unit.
/// </summary>
public static class Display
{
    public const string Unknown = "Unknown";

    private static readonly string[] RomanEpisodes = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"];

    public static string Text(string? value) => value ?? Unknown;

    /// <summary>"10465" with " km" becomes "10,465 km"; "indefinite" stays as is; null becomes "Unknown".</summary>
    public static string Number(string? value, string suffix = "")
    {
        if (value is null)
        {
            return Unknown;
        }

        return decimal.TryParse(value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)
            ? number.ToString("#,0.##", CultureInfo.InvariantCulture) + suffix
            : value;
    }

    /// <summary>Joins two values the way the wireframe shows them, for example "1 / 0" or "T-65 X-wing / Incom Corporation".</summary>
    public static string Pair(string? first, string? second) => $"{Text(first)} / {Text(second)}";

    public static string Episode(int episodeId) =>
        episodeId is >= 1 and <= 9 ? $"Episode {RomanEpisodes[episodeId - 1]}" : $"Episode {episodeId}";
}
