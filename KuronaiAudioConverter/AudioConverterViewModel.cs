using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuronaiAudioConverter;

public sealed class AudioConverterViewModel : ObservableRecipient
{
    public IEnumerable<string> AudioFormats { get; } = FFMpeg.GetAudioCodecs().Select(codec => codec.Name).OrderBy(name => name);

    private string selectedAudioFormat;
    public string SelectedAudioFormat
    {
        get => selectedAudioFormat;
        set
        {
            SetProperty(ref selectedAudioFormat, value);
            if (FolderContents is not null && FolderContents.Any())
                FolderContents = new(FolderContents.Select(line => line.ChangeDestinationCodec(value)));
        }
    }

    private bool isRunning;
    public bool IsRunning
    {
        get => isRunning;
        set => SetProperty(ref isRunning, value);
    }

    private string inputDirectory;
    public string InputDirectory
    {
        get => inputDirectory;
        set => SetProperty(ref inputDirectory, value);
    }

    private string outputDirectory;
    public string OutputDirectory
    {
        get => outputDirectory;
        set => SetProperty(ref outputDirectory, value);
    }


    private ObservableCollection<FileConversionItem> folderContents;
    public ObservableCollection<FileConversionItem> FolderContents
    {
        get => folderContents;
        set => SetProperty(ref folderContents, value);
    }

    public RelayCommand StopLoadingFiles { get; }
    public AsyncRelayCommand SelectInputFolder { get; }
    public AsyncRelayCommand SelectOutputFolder { get; }

    private int inputDirectoryFileCount;
    public int InputDirectoryFileCount
    {
        get => inputDirectoryFileCount;
        set => SetProperty(ref inputDirectoryFileCount, value);
    }

    private Dictionary<CancellationTokenOperation, CancellationTokenSource> cancellationSources;

    public AudioConverterViewModel()
    {
        InitializeDefaults();
        StopLoadingFiles = new RelayCommand(StopLoadingFilesExecute, CanStopLoadingFiles);
        SelectInputFolder = new AsyncRelayCommand(SelectInputDirectory, CanSelectInputDirectory);
        SelectOutputFolder = new AsyncRelayCommand(SelectOutputDirectory, CanSelectOutputDirectory);
    }

    private void InitializeDefaults()
    {
        SelectedAudioFormat = AudioFormats.First();
        IsRunning = false;
        cancellationSources = new()
        {
            { CancellationTokenOperation.CountInputDirFiles, new() },
            { CancellationTokenOperation.LoadAudioFiles, new() },
        };
        FolderContents = new();
        InputDirectoryRecursive = false;
    }

    #region SelectInputFolder
    private async Task SelectInputDirectory()
    {
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            return;

        string inputDirectory = folderBrowserDialog.SelectedPath;
        Task countFilesTask = GetFileCountAsync(InputDirectoryRecursive);
        InputDirectory = inputDirectory;

        if (string.IsNullOrWhiteSpace(InputDirectory) || string.IsNullOrWhiteSpace(OutputDirectory))
            return;

        await countFilesTask;
        await LoadFolderContentsAsync(InputDirectoryRecursive);
    }

    private bool? inputDirectoryRecursive;
    public bool? InputDirectoryRecursive
    {
        get => inputDirectoryRecursive;
        set => SetProperty(ref inputDirectoryRecursive, value);
    }

    private double loadFileProgress;
    public double LoadFileProgress
    {
        get => loadFileProgress;
        set => SetProperty(ref loadFileProgress, value);
    }

    private bool CanSelectInputDirectory() => !IsRunning;
    #endregion
    #region SelectOutputFolder
    private async Task SelectOutputDirectory()
    {
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            return;

        OutputDirectory = folderBrowserDialog.SelectedPath;

        if (string.IsNullOrWhiteSpace(InputDirectory) || string.IsNullOrWhiteSpace(OutputDirectory))
            return;

        await LoadFolderContentsAsync(InputDirectoryRecursive);
    }

    private bool CanSelectOutputDirectory() => !IsRunning;
    #endregion

    #region StopLoadingFiles
    private void StopLoadingFilesExecute()
    {
        cancellationSources[CancellationTokenOperation.LoadAudioFiles].Cancel();
        IsRunning = false;
    }

    private bool CanStopLoadingFiles() => IsRunning;
    #endregion

    private Task LoadFolderContentsAsync(bool? recursive)
    {
        cancellationSources[CancellationTokenOperation.LoadAudioFiles].Cancel();
        cancellationSources[CancellationTokenOperation.LoadAudioFiles] = new CancellationTokenSource();
        return Task.Run(async () =>
        {
            App.Current.Dispatcher.Invoke(() => IsRunning = true);
            SearchOption searchOption = (recursive is null or false)
                ? SearchOption.TopDirectoryOnly
                : SearchOption.AllDirectories;

            List<Task> ffprobeAnalysis = new();
            foreach (var item in Directory.EnumerateFiles(inputDirectory, "*.*", searchOption))
            {
                if (!AudioFormats.Contains(Path.GetFileName(item).Split('.')[1])) continue;
                FileConversionItem conversionItem = new(item, OutputDirectory, SelectedAudioFormat);
                ffprobeAnalysis.Add(conversionItem.RunFFProbe());

                App.Current.Dispatcher.Invoke(() =>
                {
                    FolderContents.Add(conversionItem);
                    LoadFileProgress++;
                });

                if (cancellationSources[CancellationTokenOperation.LoadAudioFiles].Token.IsCancellationRequested) break;
            }

            await Task.WhenAll(ffprobeAnalysis);
            App.Current.Dispatcher.Invoke(() => IsRunning = false);
        }, cancellationSources[CancellationTokenOperation.LoadAudioFiles].Token);
    }

    private Task GetFileCountAsync(bool? recursive = null)
    {
        cancellationSources[CancellationTokenOperation.CountInputDirFiles].Cancel();
        cancellationSources[CancellationTokenOperation.CountInputDirFiles] = new CancellationTokenSource();

        SearchOption searchOption = (recursive is null or false)
            ? SearchOption.TopDirectoryOnly
            : SearchOption.AllDirectories;

        return Task.Run(() =>
        {
            foreach (var item in Directory.EnumerateFiles(InputDirectory, "*.*", searchOption))
            {
                if (!AudioFormats.Contains(Path.GetFileName(item).Split('.')[1])) continue;
                InputDirectoryFileCount++;
                if (cancellationSources[CancellationTokenOperation.CountInputDirFiles].IsCancellationRequested) break;
            }

        }, cancellationSources[CancellationTokenOperation.CountInputDirFiles].Token);
    }
}
