using System.Collections.Generic;

namespace SpaceLens.Core;

public sealed class FileSystemScanService : IScanService
{
    public Task<ScanSnapshot> ScanAsync(ScanRoot root, IProgress<ScanProgress> progress, CancellationToken ct)
    {
        return Task.Run(() => ScanInternal(root, progress, ct), ct);
    }

    private static ScanSnapshot ScanInternal(ScanRoot root, IProgress<ScanProgress> progress, CancellationToken ct)
    {
        var startedUtc = DateTime.UtcNow;
        var items = new List<ScanItem>();
        var folderTotals = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        long filesProcessed = 0;
        long foldersProcessed = 0;
        long bytesDiscovered = 0;

        var normalizedRoot = NormalizePath(root.Path);
        var pending = new Stack<string>();
        pending.Push(normalizedRoot);

        while (pending.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var currentDirectoryPath = pending.Pop();

            DirectoryInfo currentDirectory;
            try
            {
                currentDirectory = new DirectoryInfo(currentDirectoryPath);
                if (!currentDirectory.Exists)
                {
                    continue;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{currentDirectoryPath}: {ex.Message}");
                ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, currentDirectoryPath, pending.Count, errors.Count, null);
                continue;
            }

            var directoryItem = new ScanItem
            {
                Path = currentDirectory.FullName,
                IsDirectory = true,
                SizeBytes = 0,
                Extension = null,
                LastModifiedUtc = currentDirectory.LastWriteTimeUtc
            };

            items.Add(directoryItem);
            foldersProcessed++;
            ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, directoryItem.Path, pending.Count, errors.Count, directoryItem);

            var isReparsePoint = (currentDirectory.Attributes & FileAttributes.ReparsePoint) != 0;
            if (isReparsePoint)
            {
                continue;
            }

            try
            {
                foreach (var filePath in Directory.EnumerateFiles(currentDirectory.FullName))
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        var fileSize = fileInfo.Exists ? fileInfo.Length : 0;

                        var fileItem = new ScanItem
                        {
                            Path = fileInfo.FullName,
                            IsDirectory = false,
                            SizeBytes = fileSize,
                            Extension = fileInfo.Extension,
                            LastModifiedUtc = fileInfo.LastWriteTimeUtc
                        };

                        items.Add(fileItem);
                        filesProcessed++;
                        bytesDiscovered += fileSize;
                        RollupFileSize(fileInfo.DirectoryName, fileSize, normalizedRoot, folderTotals);

                        ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, fileItem.Path, pending.Count, errors.Count, fileItem);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{filePath}: {ex.Message}");
                        ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, filePath, pending.Count, errors.Count, null);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{currentDirectory.FullName}: {ex.Message}");
                ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, currentDirectory.FullName, pending.Count, errors.Count, null);
            }

            try
            {
                foreach (var childDirectoryPath in Directory.EnumerateDirectories(currentDirectory.FullName))
                {
                    ct.ThrowIfCancellationRequested();
                    pending.Push(childDirectoryPath);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{currentDirectory.FullName}: {ex.Message}");
                ReportProgress(progress, filesProcessed, foldersProcessed, bytesDiscovered, currentDirectory.FullName, pending.Count, errors.Count, null);
            }
        }

        foreach (var item in items.Where(i => i.IsDirectory))
        {
            var normalized = NormalizePath(item.Path);
            if (folderTotals.TryGetValue(normalized, out var total))
            {
                item.SizeBytes = total;
            }
        }

        return new ScanSnapshot(root, startedUtc, DateTime.UtcNow, items, bytesDiscovered, errors);
    }

    private static void RollupFileSize(string? directoryPath, long fileSize, string rootPath, Dictionary<string, long> folderTotals)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        var normalizedRoot = NormalizePath(rootPath);
        var current = NormalizePath(directoryPath);

        while (!string.IsNullOrWhiteSpace(current) && current.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            folderTotals[current] = folderTotals.TryGetValue(current, out var existing)
                ? existing + fileSize
                : fileSize;

            if (string.Equals(current, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = NormalizePath(parent.FullName);
        }
    }

    private static string NormalizePath(string path)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    private static void ReportProgress(
        IProgress<ScanProgress> progress,
        long filesProcessed,
        long foldersProcessed,
        long bytesDiscovered,
        string currentPath,
        int queueDepth,
        int errorsCount,
        ScanItem? discoveredItem)
    {
        progress.Report(new ScanProgress
        {
            FilesProcessed = filesProcessed,
            FoldersProcessed = foldersProcessed,
            BytesDiscovered = bytesDiscovered,
            CurrentPath = currentPath,
            QueueDepth = queueDepth,
            ErrorsCount = errorsCount,
            ItemDiscovered = discoveredItem
        });
    }
}
