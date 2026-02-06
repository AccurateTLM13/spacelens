namespace SpaceLens.Core;

public interface IScanService
{
    Task<ScanSnapshot> ScanAsync(ScanRoot root, IProgress<ScanProgress> progress, CancellationToken ct);
}
