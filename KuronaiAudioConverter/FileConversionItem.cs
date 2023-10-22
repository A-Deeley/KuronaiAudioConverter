using FFMpegCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuronaiAudioConverter;

public class FileConversionItem
{
    public FileConversionItem(string filePath, string outputDirectory, string destinationCodec)
    {
        string fileName = Path.GetFileName(filePath).Split('.')[0];
        
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

    public async Task RunFFProbe()
    {
        try
        {
            MediaAnalysis = await FFProbe.AnalyseAsync(CurrentLocation.LocalPath);
        }
        catch (Exception)
        {
            MediaAnalysis = null;
        }

    }


    public Uri CurrentLocation { get; set; }
    public Uri OutputLocation { get; set; }
    public string CurrentName { get => Path.GetFileName(CurrentLocation.LocalPath); }
    public string OutputName {  get => Path.GetFileName(OutputLocation.LocalPath); }
    public double? Progress { get; set; }
    public IMediaAnalysis? MediaAnalysis { get; set; }
    public ConversionStatus Status { get; set; }
}
