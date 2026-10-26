using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Datapad.Api.Swapi;

internal static class SwapiServiceCollectionExtensions
{
    public static IServiceCollection AddSwapi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SwapiOptions>()
            .Bind(configuration.GetSection(SwapiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();

        // Standard pipeline: total timeout, per-attempt timeout, retry with jittered backoff on transient
        // failures (5xx, 408, 429, network errors), and a circuit breaker so a dead upstream fails fast.
        services.AddHttpClient<SwapiClient>((sp, client) =>
                client.BaseAddress = sp.GetRequiredService<IOptions<SwapiOptions>>().Value.BaseAddress)
            .AddStandardResilienceHandler();

        services.AddScoped<ISwapiClient>(sp => new CachedSwapiClient(
            sp.GetRequiredService<SwapiClient>(),
            sp.GetRequiredService<IMemoryCache>(),
            sp.GetRequiredService<IOptions<SwapiOptions>>()));

        return services;
    }
}
