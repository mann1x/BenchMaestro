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
}
