using System.ComponentModel.DataAnnotations;

namespace Datapad.Api.Swapi;

internal sealed class SwapiOptions
{
    public const string SectionName = "Swapi";

    [Required]
    public Uri BaseAddress { get; set; } = new("https://swapi.info/api/");

    /// <summary>How long a successfully loaded collection is reused. The films are not changing, so hours is safe.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "7.00:00:00")]
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(6);
}
