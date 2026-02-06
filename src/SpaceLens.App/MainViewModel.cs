using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SpaceLens.Core;

namespace SpaceLens.App;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IFakeScanService _fakeScanService;
    private bool _isScanning;
    private int _scanProgress;
    private string _statusText = "Ready";
    private string _selectedDrive = "C:";
    private string _selectedFolder = @"C:\Users\Demo";

    public MainViewModel() : this(new FakeScanService())
    {
    }

    internal MainViewModel(IFakeScanService fakeScanService)
    {
        _fakeScanService = fakeScanService;
        ScanCommand = new RelayCommand(async _ => await RunFakeScanAsync(), _ => !IsScanning);

        SnapshotPlaceholders.Add("Snapshot (placeholder)");
        TreePlaceholders.Add("Tree view placeholder");
        TopFilesPlaceholders.Add("Top files placeholder");
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

    private async Task RunFakeScanAsync()
    {
        IsScanning = true;
        StatusText = "Running fake scan...";
        ScanProgress = 0;

        var progress = new Progress<int>(value =>
        {
            ScanProgress = value;
            StatusText = $"Fake scan progress: {value}%";
        });

        var result = await _fakeScanService.RunAsync(progress, CancellationToken.None);
        SnapshotPlaceholders.Insert(0, $"Snapshot {DateTime.Now:HH:mm:ss} ({FormatSize(result.TotalBytes)})");
        StatusText = $"Fake scan complete: {result.Items.Count} mock items";
        IsScanning = false;
    }

    private static string FormatSize(long bytes)
    {
        var gb = bytes / 1024d / 1024d / 1024d;
        return $"{gb:F1} GB";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
