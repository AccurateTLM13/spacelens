using Microsoft.Data.Sqlite;
using SpaceLens.Core;

namespace SpaceLens.Data;

public sealed record SnapshotMetadata(long Id, DateTime TimestampUtc, string RootPath, long TotalBytes);

public sealed class SnapshotStore
{
    private readonly string _connectionString;

    public SnapshotStore(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString();

        Initialize();
    }

    public static SnapshotStore CreateDefault()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(localAppData, "SpaceLens", "spacelens.db");
        return new SnapshotStore(dbPath);
    }

    public long SaveSnapshot(ScanSnapshot snapshot)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var insertSnapshot = connection.CreateCommand();
        insertSnapshot.Transaction = transaction;
        insertSnapshot.CommandText = @"
            INSERT INTO snapshots (completed_utc, root_path, root_kind, total_bytes)
            VALUES ($completedUtc, $rootPath, $rootKind, $totalBytes);
            SELECT last_insert_rowid();";
        insertSnapshot.Parameters.AddWithValue("$completedUtc", snapshot.CompletedUtc.ToString("O"));
        insertSnapshot.Parameters.AddWithValue("$rootPath", snapshot.Root.Path);
        insertSnapshot.Parameters.AddWithValue("$rootKind", snapshot.Root.Kind.ToString());
        insertSnapshot.Parameters.AddWithValue("$totalBytes", snapshot.TotalBytes);

        var snapshotId = (long)(insertSnapshot.ExecuteScalar() ?? 0L);

        var insertItem = connection.CreateCommand();
        insertItem.Transaction = transaction;
        insertItem.CommandText = @"
            INSERT INTO snapshot_items (snapshot_id, path, size_bytes, is_directory, extension, last_modified_utc)
            VALUES ($snapshotId, $path, $sizeBytes, $isDirectory, $extension, $lastModifiedUtc);";

        var snapshotIdParam = insertItem.Parameters.Add("$snapshotId", SqliteType.Integer);
        var pathParam = insertItem.Parameters.Add("$path", SqliteType.Text);
        var sizeParam = insertItem.Parameters.Add("$sizeBytes", SqliteType.Integer);
        var isDirectoryParam = insertItem.Parameters.Add("$isDirectory", SqliteType.Integer);
        var extensionParam = insertItem.Parameters.Add("$extension", SqliteType.Text);
        var lastModifiedParam = insertItem.Parameters.Add("$lastModifiedUtc", SqliteType.Text);

        foreach (var item in snapshot.Items)
        {
            snapshotIdParam.Value = snapshotId;
            pathParam.Value = item.Path;
            sizeParam.Value = item.SizeBytes;
            isDirectoryParam.Value = item.IsDirectory ? 1 : 0;
            extensionParam.Value = item.Extension ?? string.Empty;
            lastModifiedParam.Value = item.LastModifiedUtc.ToString("O");
            insertItem.ExecuteNonQuery();
        }

        transaction.Commit();
        return snapshotId;
    }

    public IReadOnlyList<SnapshotMetadata> GetSnapshots()
    {
        var snapshots = new List<SnapshotMetadata>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, completed_utc, root_path, total_bytes
            FROM snapshots
            ORDER BY completed_utc DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            snapshots.Add(new SnapshotMetadata(
                reader.GetInt64(0),
                DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind),
                reader.GetString(2),
                reader.GetInt64(3)));
        }

        return snapshots;
    }

    public ScanSnapshot? GetSnapshot(long snapshotId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var snapshotCommand = connection.CreateCommand();
        snapshotCommand.CommandText = @"
            SELECT root_kind, root_path, completed_utc, total_bytes
            FROM snapshots
            WHERE id = $id;";
        snapshotCommand.Parameters.AddWithValue("$id", snapshotId);

        using var snapshotReader = snapshotCommand.ExecuteReader();
        if (!snapshotReader.Read())
        {
            return null;
        }

        var rootKindValue = snapshotReader.GetString(0);
        var rootKind = Enum.TryParse<ScanRootKind>(rootKindValue, out var parsedKind)
            ? parsedKind
            : ScanRootKind.Folder;
        var rootPath = snapshotReader.GetString(1);
        var completedUtc = DateTime.Parse(snapshotReader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind);
        var totalBytes = snapshotReader.GetInt64(3);

        var items = new List<ScanItem>();
        var itemCommand = connection.CreateCommand();
        itemCommand.CommandText = @"
            SELECT path, size_bytes, is_directory, extension, last_modified_utc
            FROM snapshot_items
            WHERE snapshot_id = $snapshotId;";
        itemCommand.Parameters.AddWithValue("$snapshotId", snapshotId);

        using var itemReader = itemCommand.ExecuteReader();
        while (itemReader.Read())
        {
            var extension = itemReader.GetString(3);
            items.Add(new ScanItem
            {
                Path = itemReader.GetString(0),
                SizeBytes = itemReader.GetInt64(1),
                IsDirectory = itemReader.GetInt64(2) == 1,
                Extension = string.IsNullOrWhiteSpace(extension) ? null : extension,
                LastModifiedUtc = DateTime.Parse(itemReader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind)
            });
        }

        return new ScanSnapshot(
            new ScanRoot(rootKind, rootPath),
            completedUtc,
            completedUtc,
            items,
            totalBytes,
            Array.Empty<string>());
    }

    private void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS snapshots (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                completed_utc TEXT NOT NULL,
                root_path TEXT NOT NULL,
                root_kind TEXT NOT NULL,
                total_bytes INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS snapshot_items (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                snapshot_id INTEGER NOT NULL,
                path TEXT NOT NULL,
                size_bytes INTEGER NOT NULL,
                is_directory INTEGER NOT NULL,
                extension TEXT NOT NULL,
                last_modified_utc TEXT NOT NULL,
                FOREIGN KEY(snapshot_id) REFERENCES snapshots(id)
            );

            CREATE INDEX IF NOT EXISTS idx_snapshot_items_snapshot_id
            ON snapshot_items(snapshot_id);";
        command.ExecuteNonQuery();
    }
}
