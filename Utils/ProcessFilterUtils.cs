using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UltrawideOverlays.Utils
{
    public static class ProcessFilterUtils
    {
        public static readonly HashSet<string> FilteredProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // 🧠 Essential Windows System Processes
            "System",
            "Idle",
            "wininit",
            "winlogon",
            "lsass",
            "csrss",
            "smss",
            "services",
            "svchost",
            "explorer",
            "dwm",
            "fontdrvhost",

            // 🛠️ Background & Service Processes
            "audiodg",
            "spoolsv",
            "wmiapsrv",
            "taskhostw",
            "searchindexer",
            "systemsettings",
            "sihost",
            "runtimebroker",
            "conhost",
            "ApplicationFrameHost",
            "UltrawideOverlays",
            "TextInputHost",

            // 🌐 Network & Security
            "vpnclient",
            "openvpn",
            "nordvpn-service",
            "antimalware",
            "msmpeng",

            // 💻 Hardware-Related / OEM
            "igfxtray",
            "nvcontainer",
            "nvsphelper64",
            "RtkNGUI64",
            "RTKAUDIOSERVICE64",
            "atieclxx"
        };

        public static List<Process> FilterProcesses(IEnumerable<Process> processes)
        {
            if (processes == null)
                throw new ArgumentNullException(nameof(processes), "The process list cannot be null.");

            var ProcessesWithWindows = processes.Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            var outList = ProcessesWithWindows.Where(p => !FilteredProcesses.Contains(p.ProcessName)).OrderBy(p => p.ProcessName).ToList();

            return outList;
        }
    }
}
