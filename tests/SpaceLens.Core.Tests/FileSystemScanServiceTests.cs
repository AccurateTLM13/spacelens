using Xunit;
using SpaceLens.Core;

namespace SpaceLens.Core.Tests;

public sealed class FileSystemScanServiceTests
{
    [Fact]
    public async Task ScanAsync_ComputesFolderSizesFromFiles()
    {
        var root = Directory.CreateTempSubdirectory("spacelens-scan-");
        try
        {
            var sub = Directory.CreateDirectory(Path.Combine(root.FullName, "sub"));
            await File.WriteAllBytesAsync(Path.Combine(sub.FullName, "a.bin"), new byte[128]);
            await File.WriteAllBytesAsync(Path.Combine(sub.FullName, "b.bin"), new byte[256]);

            var service = new FileSystemScanService();
            var snapshot = await service.ScanAsync(ScanRoot.ForFolder(root.FullName), new Progress<ScanProgress>(), CancellationToken.None);

            var subFolderItem = snapshot.Items.Single(i => i.IsDirectory && Path.GetFullPath(i.Path) == sub.FullName);
            Assert.Equal(384, subFolderItem.SizeBytes);
            Assert.Equal(384, snapshot.TotalBytes);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ScanAsync_RecordsReparsePointButDoesNotTraverseIt()
    {
        var root = Directory.CreateTempSubdirectory("spacelens-reparse-");
        try
        {
            var target = Directory.CreateDirectory(Path.Combine(root.FullName, "target"));
            await File.WriteAllBytesAsync(Path.Combine(target.FullName, "inside.bin"), new byte[64]);

            var linkPath = Path.Combine(root.FullName, "link");
            try
            {
                Directory.CreateSymbolicLink(linkPath, target.FullName);
            }
            catch
            {
                return;
            }

            var service = new FileSystemScanService();
            var snapshot = await service.ScanAsync(ScanRoot.ForFolder(root.FullName), new Progress<ScanProgress>(), CancellationToken.None);

            var linkItem = snapshot.Items.Single(i => i.IsDirectory && Path.GetFullPath(i.Path) == Path.GetFullPath(linkPath));
            Assert.Equal(0, linkItem.SizeBytes);
            Assert.DoesNotContain(snapshot.Items, i => !i.IsDirectory && i.Path.StartsWith(linkPath, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
