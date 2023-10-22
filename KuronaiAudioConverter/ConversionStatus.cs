using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuronaiAudioConverter;

public enum ConversionStatus
{
    Unknown,
    Analysing,
    Analysed,
    Converting,
    Converted,
    Failed,
    NotStarted
}
