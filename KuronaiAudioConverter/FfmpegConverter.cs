using FFMpegCore;
using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KuronaiAudioConverter;

internal class FfmpegConverter
{
    private string _workingDir;

    public FfmpegConverter(string workingDir)
    {
        _workingDir = workingDir;
    }

    public event EventHandler<double> ProgressChanged;

    internal async Task Convert(string inputName, string outputName)
    {
        var analysis = await FFProbe.AnalyseAsync($@"{_workingDir}\{inputName}");

        await FFMpegArguments
            .FromFileInput($@"{_workingDir}\{inputName}")
            .OutputToFile($@"{_workingDir}\{outputName}.mp3", true, options => options
                .WithFastStart())
            .NotifyOnProgress(ProgressHandler, analysis.Duration)
            .ProcessAsynchronously();
    }

    private void ProgressHandler(double progress) => ProgressChanged.Invoke(this, progress);
}
