using Bunit;
using Datapad.Client.Pages;

namespace Datapad.Client.Tests;

public sealed class HomeSmokeTests : BunitContext
{
    [Fact]
    public void Home_RendersTitle()
    {
        var cut = Render<Home>();

        Assert.Contains("Star Wars Character Datapad", cut.Find("h1").TextContent, StringComparison.Ordinal);
    }
}
