namespace SpaceLens.Core;

public interface IFakeScanService
{
    Task<FakeScanResult> RunAsync(IProgress<int> progress, CancellationToken cancellationToken);
}
