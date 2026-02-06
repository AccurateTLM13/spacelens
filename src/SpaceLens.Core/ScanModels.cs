namespace SpaceLens.Core;

public enum ScanRootKind
{
    Drive,
    Folder
}

public sealed record ScanRoot(ScanRootKind Kind, string Path)
{
    public static ScanRoot ForDrive(string drivePath) => new(ScanRootKind.Drive, drivePath);

    public static ScanRoot ForFolder(string folderPath) => new(ScanRootKind.Folder, folderPath);
}

public sealed class ScanItem
{
    public required string Path { get; init; }

    public required bool IsDirectory { get; init; }

    public long SizeBytes { get; set; }

    public string? Extension { get; init; }

    public DateTime LastModifiedUtc { get; init; }
}

public sealed record ScanSnapshot(
    ScanRoot Root,
    DateTime StartedUtc,
    DateTime CompletedUtc,
    IReadOnlyList<ScanItem> Items,
    long TotalBytes,
    IReadOnlyList<string> Errors);

public sealed class ScanProgress
{
    public long FilesProcessed { get; init; }

    public long FoldersProcessed { get; init; }

    public long BytesDiscovered { get; init; }

    public string CurrentPath { get; init; } = string.Empty;

    public int QueueDepth { get; init; }

    public int ErrorsCount { get; init; }

    public ScanItem? ItemDiscovered { get; init; }
}
