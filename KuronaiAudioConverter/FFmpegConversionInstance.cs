using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KuronaiAudioConverter;

internal class FFmpegConversionInstance
{
    private readonly FileConversionItem item;
    private List<string> errors = new();
    private event EventHandler<double> ProgressChanged;
    public FFmpegConversionInstance(FileConversionItem item)
    {
        this.item = item;
        ProgressChanged += item.UpdateProgress;
    }

    internal Task StartConversion(CancellationToken cancellationToken)
    {
        return FFMpegArguments
            .FromFileInput(item.CurrentLocation.LocalPath)
            .OutputToFile(item.OutputLocation.LocalPath, true, options => options
                .WithFastStart())
            .NotifyOnProgress(ProgressHandler, item.MediaAnalysis.Duration)
            .CancellableThrough(cancellationToken)
            .NotifyOnError(OnError)
            .ProcessAsynchronously();
    }

    private void OnError(string error)
    {
        errors.Add(error);
    }

    internal Task StartConversion()
    {
        return FFMpegArguments
            .FromFileInput(item.CurrentLocation.LocalPath)
            .OutputToFile(item.OutputLocation.LocalPath, true, options => options
                .WithFastStart())
            .NotifyOnProgress(ProgressHandler, item.MediaAnalysis.Duration)
            .ProcessAsynchronously();
    }

    private void ProgressHandler(double progress) => ProgressChanged?.Invoke(this, progress);
}
