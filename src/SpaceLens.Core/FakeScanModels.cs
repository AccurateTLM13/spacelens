namespace SpaceLens.Core;

public sealed record FakeScanItem(string Name, string Path, long SizeBytes, bool IsFolder);

public sealed record FakeScanResult(IReadOnlyList<FakeScanItem> Items, long TotalBytes);
