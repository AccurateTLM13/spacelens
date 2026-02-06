namespace SpaceLens.Core;

public sealed class FakeScanService : IFakeScanService
{
    public async Task<FakeScanResult> RunAsync(IProgress<int> progress, CancellationToken cancellationToken)
    {
        var items = new List<FakeScanItem>();

        for (var pct = 0; pct <= 100; pct += 5)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(80, cancellationToken);
            progress.Report(pct);
        }

        items.Add(new FakeScanItem("Windows", @"C:\\Windows", 12_000_000_000, true));
        items.Add(new FakeScanItem("Program Files", @"C:\\Program Files", 18_500_000_000, true));
        items.Add(new FakeScanItem("Users", @"C:\\Users", 35_000_000_000, true));
        items.Add(new FakeScanItem("video.mp4", @"C:\\Users\\Demo\\Videos\\video.mp4", 1_700_000_000, false));
        items.Add(new FakeScanItem("archive.zip", @"C:\\Users\\Demo\\Downloads\\archive.zip", 860_000_000, false));

        var total = items.Sum(i => i.SizeBytes);
        return new FakeScanResult(items, total);
    }
}
