using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using SpaceLens.Core;
using System.Linq;
using System.IO;

namespace SpaceLens.App;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IScanService _scanService;
    private readonly ConcurrentQueue<ScanItem> _pendingUiItems = new();
    private readonly DispatcherTimer _uiBatchTimer;
    private bool _isScanning;
    private int _scanProgress;
    private string _statusText = "Ready";
    private string _selectedDrive = "C:";
    private string _selectedFolder = @"C:\Users\Demo";
    private long _latestBytes;

    public MainViewModel() : this(new FileSystemScanService())
    {
    }

    internal MainViewModel(IScanService scanService)
    {
        _scanService = scanService;
        ScanCommand = new RelayCommand(async _ => await RunScanAsync(), _ => !IsScanning);

        _uiBatchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _uiBatchTimer.Tick += (_, _) => ConsumePendingUiItems();

        SnapshotPlaceholders.Add("No snapshots yet.");
        TreePlaceholders.Add("No scan data yet.");
        TopFilesPlaceholders.Add("No scan data yet.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> SnapshotPlaceholders { get; } = new();
    public ObservableCollection<string> TreePlaceholders { get; } = new();
    public ObservableCollection<string> TopFilesPlaceholders { get; } = new();

    public RelayCommand ScanCommand { get; }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (_isScanning == value) return;
            _isScanning = value;
            OnPropertyChanged();
            ScanCommand.RaiseCanExecuteChanged();
        }
    }

    public int ScanProgress
    {
        get => _scanProgress;
        private set
        {
            if (_scanProgress == value) return;
            _scanProgress = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText == value) return;
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public string SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (_selectedDrive == value) return;
            _selectedDrive = value;
            OnPropertyChanged();
        }
    }

    public string SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (_selectedFolder == value) return;
            _selectedFolder = value;
            OnPropertyChanged();
        }
    }

    private async Task RunScanAsync()
    {
        IsScanning = true;
        StatusText = "Scanning filesystem...";
        ScanProgress = 0;
        _latestBytes = 0;

        _pendingUiItems.Clear();
        TreePlaceholders.Clear();
        TopFilesPlaceholders.Clear();

        _uiBatchTimer.Start();

        var root = ResolveRoot();
        var progress = new Progress<ScanProgress>(p =>
        {
            _latestBytes = p.BytesDiscovered;
            if (p.ItemDiscovered is not null)
            {
                _pendingUiItems.Enqueue(p.ItemDiscovered);
            }

            StatusText = $"Scanning {p.CurrentPath} | files: {p.FilesProcessed}, folders: {p.FoldersProcessed}, errors: {p.ErrorsCount}";
            ScanProgress = p.QueueDepth == 0 ? 100 : 50;
        });

        var snapshot = await _scanService.ScanAsync(root, progress, CancellationToken.None);

        ConsumePendingUiItems();
        _uiBatchTimer.Stop();

        var summary = $"Snapshot {DateTime.Now:HH:mm:ss} ({FormatSize(snapshot.TotalBytes)})";
        if (SnapshotPlaceholders.Count == 1 && SnapshotPlaceholders[0] == "No snapshots yet.")
        {
            SnapshotPlaceholders.Clear();
        }

        SnapshotPlaceholders.Insert(0, summary);
        StatusText = $"Scan complete: {snapshot.Items.Count} items, {snapshot.Errors.Count} errors";
        ScanProgress = 100;
        IsScanning = false;
    }

    private ScanRoot ResolveRoot()
    {
        if (!string.IsNullOrWhiteSpace(SelectedFolder) && Directory.Exists(SelectedFolder))
        {
            return ScanRoot.ForFolder(SelectedFolder);
        }

        var drivePath = SelectedDrive.EndsWith(":", StringComparison.Ordinal) ? $"{SelectedDrive}{Path.DirectorySeparatorChar}" : SelectedDrive;
        return ScanRoot.ForDrive(drivePath);
    }

    private void ConsumePendingUiItems()
    {
        var batch = new List<ScanItem>(capacity: 400);
        while (batch.Count < 400 && _pendingUiItems.TryDequeue(out var item))
        {
            batch.Add(item);
        }

        if (batch.Count == 0)
        {
            return;
        }

        foreach (var directory in batch.Where(i => i.IsDirectory).Take(100))
        {
            TreePlaceholders.Add(FormatDirectoryPresentation(directory));
        }

        var topFiles = batch
            .Where(i => !i.IsDirectory)
            .OrderByDescending(i => i.SizeBytes)
            .Take(20)
            .Select(i => $"{Path.GetFileName(i.Path)} - {FormatSize(i.SizeBytes)}")
            .ToList();

        if (topFiles.Count > 0)
        {
            if (TopFilesPlaceholders.Count == 1 && TopFilesPlaceholders[0] == "No scan data yet.")
            {
                TopFilesPlaceholders.Clear();
            }

            foreach (var topFile in topFiles)
            {
                TopFilesPlaceholders.Add(topFile);
            }

            while (TopFilesPlaceholders.Count > 40)
            {
                TopFilesPlaceholders.RemoveAt(TopFilesPlaceholders.Count - 1);
            }
        }

        if (TreePlaceholders.Count > 200)
        {
            while (TreePlaceholders.Count > 200)
            {
                TreePlaceholders.RemoveAt(0);
            }
        }

        StatusText = $"Discovered {FormatSize(_latestBytes)} so far";
    }

    private static string FormatDirectoryPresentation(ScanItem directory)
    {
        var baseLabel = $"{directory.Path} [{FormatSize(directory.SizeBytes)}]";
        return IsSystemDirectoryPath(directory.Path)
            ? $"{baseLabel} — System directory (collapsed by default)"
            : baseLabel;
    }

    private static bool IsSystemDirectoryPath(string path)
    {
        var normalized = path.Replace('/', '\\').TrimEnd('\\');
        var leaf = Path.GetFileName(normalized);

        return leaf.Equals("Windows", StringComparison.OrdinalIgnoreCase)
               || leaf.Equals("Program Files", StringComparison.OrdinalIgnoreCase)
               || leaf.Equals("Program Files (x86)", StringComparison.OrdinalIgnoreCase)
               || leaf.Equals("ProgramData", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatSize(long bytes)
    {
        var gb = bytes / 1024d / 1024d / 1024d;
        return gb >= 1 ? $"{gb:F1} GB" : $"{bytes / 1024d / 1024d:F1} MB";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
