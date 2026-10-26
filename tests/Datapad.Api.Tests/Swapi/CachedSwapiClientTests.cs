using Datapad.Api.Swapi;
using Datapad.Api.Tests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Datapad.Api.Tests.Swapi;

public sealed class CachedSwapiClientTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly FakeSwapiClient _inner = new();

    [Fact]
    public async Task GetPeopleAsync_SecondCall_IsServedFromCache()
    {
        var client = CreateClient();

        await client.GetPeopleAsync(TestContext.Current.CancellationToken);
        var second = await client.GetPeopleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, _inner.PeopleCalls);
        Assert.Equal(3, second.Count);
    }

    [Fact]
    public async Task GetPeopleAsync_AfterFailure_RetriesUpstreamInsteadOfCachingTheError()
    {
        var client = CreateClient();
        var people = _inner.People;
        _inner.People = null;

        await Assert.ThrowsAsync<SwapiUnavailableException>(
            () => client.GetPeopleAsync(TestContext.Current.CancellationToken));

        _inner.People = people;
        var recovered = await client.GetPeopleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, _inner.PeopleCalls);
        Assert.Equal(3, recovered.Count);
    }

    public void Dispose() => _cache.Dispose();

    private CachedSwapiClient CreateClient() =>
        new(_inner, _cache, Options.Create(new SwapiOptions()));
}
