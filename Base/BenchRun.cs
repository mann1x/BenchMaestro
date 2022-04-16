using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BenchMaestro
{
    class BenchRun
    {

        public static void SetRunProcID(int ProcId)
        {
            App.RunningProcess = ProcId;
            Trace.WriteLine($"Running ProcID {ProcId} -> {App.RunningProcess}");
        }

        public static bool KillProcID(int ProcId)
        {
            bool success = false;

            Trace.WriteLine($"KillProcID {ProcId}");

            Process ProcById = Process.GetProcessById(ProcId);
            ProcById.Kill();

            TimeSpan timeSpan = TimeSpan.FromSeconds(10);

            int elapsed = 0;
            while ((!success) && (elapsed < timeSpan.TotalMilliseconds))
            {
                Thread.Sleep(100);
                elapsed += 100;
                success = ProcById.HasExited;
            }
            if (!success)
            {
                ProcById.Kill();
                Thread.Sleep(250);
                success = ProcById.HasExited;
            }
            Trace.WriteLine($"Killing ProcID {ProcId} -> {success}");
            if (success)
            {
                App.RunningProcess = -1;
            }

            return success;
        }

    }
    public static class TimeSpanFormattingExtension
    {
        public static string ToReadableString(this TimeSpan span)
        {
            return string.Join(", ", span.GetReadableStringElements()
               .Where(str => !string.IsNullOrWhiteSpace(str)));
        }

        private static IEnumerable<string> GetReadableStringElements(this TimeSpan span)
        {
            yield return GetDaysString((int)Math.Floor(span.TotalDays));
            yield return GetHoursString(span.Hours);
            yield return GetMinutesString(span.Minutes);
            yield return GetSecondsString(span.Seconds);
        }

        private static string GetDaysString(int days)
        {
            if (days == 0)
                return string.Empty;

            if (days == 1)
                return "1d";

            return string.Format("{0:0}d", days);
        }

        private static string GetHoursString(int hours)
        {
            if (hours == 0)
                return string.Empty;

            if (hours == 1)
                return "1h";

            return string.Format("{0:0}h", hours);
        }

        private static string GetMinutesString(int minutes)
        {
            if (minutes == 0)
                return string.Empty;

            if (minutes == 1)
                return "1m";

            return string.Format("{0:0}m", minutes);
        }

        private static string GetSecondsString(int seconds)
        {
            if (seconds == 0)
                return string.Empty;

            if (seconds == 1)
                return "1s";

            return string.Format("{0:0}s", seconds);
        }
    }
}
