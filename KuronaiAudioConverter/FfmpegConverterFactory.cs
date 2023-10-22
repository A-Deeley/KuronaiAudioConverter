using FFMpegCore;
using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KuronaiAudioConverter;

internal static class FfmpegConverterFactory
{
    internal static Task ConvertFile(FileConversionItem item, CancellationToken? cancellationToken = null)
    {
        FFmpegConversionInstance instance = new(item);
        Task runningTask;

        if (cancellationToken is null)
            runningTask = instance.StartConversion();
        else
            runningTask = instance.StartConversion((CancellationToken)cancellationToken);

        return runningTask;
    }
}
