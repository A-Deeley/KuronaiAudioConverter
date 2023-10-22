using CommunityToolkit.Mvvm.ComponentModel;
using FFMpegCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KuronaiAudioConverter;

public partial class FileConversionItem : ObservableObject
{
    public FileConversionItem(string filePath, string outputDirectory, string destinationCodec)
    {
        string fileName = Path.GetFileName(filePath).Split('.')[0];
        DestinationCodec = destinationCodec;
        CurrentLocation = new(filePath);
        OutputLocation = new(Path.Combine(outputDirectory, $"{fileName}.{destinationCodec}"));
        Status = ConversionStatus.NotStarted;
        Progress = 0;
    }

    public FileConversionItem ChangeDestinationCodec(string newCodec)
    {
        string fileName = CurrentName.Split('.')[0];

        OutputLocation = new(Path.Combine(Path.GetDirectoryName(OutputLocation.LocalPath), $"{fileName}.{newCodec}"));

        return this;
    }

    public void UpdateProgress(object? sender, double progress)
    {
        Progress = progress;
    }

    public async Task RunFFProbe()
    {
        try
        {
            ChangeStatus(ConversionStatus.Analysing, Brushes.Yellow);
            MediaAnalysis = await FFProbe.AnalyseAsync(CurrentLocation.LocalPath);
            ChangeStatus(ConversionStatus.Analysed, Brushes.Green);
        }
        catch (Exception)
        {
            ChangeStatus(ConversionStatus.Failed, Brushes.Red);
            MediaAnalysis = null;
        }

    }

    private void ChangeStatus(ConversionStatus status, Brush colour)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Status = status;
            StatusColour = colour;
            StatusMessage = status.ToString();
        });
    }


    public Uri CurrentLocation { get; set; }
    public string DestinationCodec { get; set; }
    public Uri OutputLocation { get; set; }
    public string CurrentName { get => Path.GetFileName(CurrentLocation.LocalPath); }
    public string OutputName { get => Path.GetFileName(OutputLocation.LocalPath); }

    [ObservableProperty]
    public double progress;
    public IMediaAnalysis? MediaAnalysis { get; set; }
    public ConversionStatus Status { get; set; }

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private Brush statusColour;
}
