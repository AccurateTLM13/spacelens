using SpaceLens.Core;
using SpaceLens.Data;

namespace SpaceLens.Core.Tests;

public sealed class SnapshotStoreTests
{
    [Fact]
    public void SaveSnapshot_PersistsMetadataAndItems()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"spacelens-{Guid.NewGuid():N}.db");
        try
        {
            var store = new SnapshotStore(dbPath);
            var snapshot = new ScanSnapshot(
                ScanRoot.ForFolder(@"C:\\Data"),
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow,
                new List<ScanItem>
                {
                    new() { Path = @"C:\\Data", IsDirectory = true, SizeBytes = 300, Extension = null, LastModifiedUtc = DateTime.UtcNow.AddDays(-1) },
                    new() { Path = @"C:\\Data\\a.txt", IsDirectory = false, SizeBytes = 100, Extension = ".txt", LastModifiedUtc = DateTime.UtcNow.AddDays(-1) },
                    new() { Path = @"C:\\Data\\b.bin", IsDirectory = false, SizeBytes = 200, Extension = ".bin", LastModifiedUtc = DateTime.UtcNow.AddHours(-2) }
                },
                300,
                Array.Empty<string>());

            var id = store.SaveSnapshot(snapshot);

            var snapshots = store.GetSnapshots();
            Assert.Single(snapshots);
            Assert.Equal(id, snapshots[0].Id);
            Assert.Equal(@"C:\\Data", snapshots[0].RootPath);
            Assert.Equal(300, snapshots[0].TotalBytes);

            var loaded = store.GetSnapshot(id);
            Assert.NotNull(loaded);
            Assert.Equal(3, loaded!.Items.Count);
            Assert.Equal(300, loaded.TotalBytes);
            Assert.Contains(loaded.Items, item => item.Path.EndsWith("a.txt", StringComparison.Ordinal));
        }
        finally
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}
