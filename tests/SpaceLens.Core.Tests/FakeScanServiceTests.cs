using SpaceLens.Core;

namespace SpaceLens.Core.Tests;

public sealed class FakeScanServiceTests
{
    [Fact]
    public async Task RunAsync_ReturnsMockItems()
    {
        var service = new FakeScanService();
        var result = await service.RunAsync(new Progress<int>(), CancellationToken.None);

        Assert.NotEmpty(result.Items);
        Assert.True(result.TotalBytes > 0);
    }
}
