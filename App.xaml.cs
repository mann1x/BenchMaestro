using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using System.Timers;
using ZenStates.Core;

namespace BenchMaestro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {

        public static int InterlockBench = 0;
        public static int InterlockHWM = 0;

        public static CancellationTokenSource hwmcts = new CancellationTokenSource();
        public static CancellationTokenSource benchcts = new CancellationTokenSource();

        public static ManualResetEventSlim mresbench = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mreshwm = new ManualResetEventSlim(false);

        public static System.Timers.Timer hwmtimer = new System.Timers.Timer();

        public static Thread thrMonitor;
        public static int thridMonitor;
        public static Thread thrBench;
        public static int thridBench;

        public static BenchScore CurrentRun;

        public static Process BenchProc = new Process();
        public static int RunningProcess = -1;

        public static bool TaskRunning;
        public static bool MultiRunning;

        public static SystemInfo systemInfo = new();
        public static string version;
        public static string ss_filename;
        static string _versionInfo;

        public static List<HWSensorItem> hwsensors;


        // Sleep Control
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        public class SettingBindingExtension : Binding
        {
            public SettingBindingExtension()
            {
                Initialize();
            }

            public SettingBindingExtension(string path)
                : base(path)
            {
                Initialize();
            }

            private void Initialize()
            {
                Source = BenchMaestro.Properties.Settings.Default;
                Mode = BindingMode.TwoWay;
            }
        }

        public static int[] GetThreads()
        {
            if (!App.systemInfo.STMT)
            {
                List<int> activethreads = new();

                foreach (string thr in BenchMaestro.Properties.Settings.Default.Threads)
                {
                    if (thr == "Max")
                    {
                        if (systemInfo.CPULogicalProcessors > 32) activethreads.Add(systemInfo.CPULogicalProcessors);

                    }
                    else
                    {
                        int _threads = Convert.ToInt32(thr);
                        if (_threads <= systemInfo.CPULogicalProcessors) activethreads.Add(_threads);
                    }
                }
                int[] _return = new int[activethreads.Count];
                int i = 0;

                activethreads.Sort();

                foreach (int athr in activethreads)
                {
                    _return[i] = athr;
                    i++;
                }
                return _return;
            }
            return new int[] { 1, systemInfo.CPULogicalProcessors };
        }
        public static int GetLastThread(int _base = 1)
        {
            int _thr = systemInfo.CPUCores;
            if (App.systemInfo.HyperThreading)
            {
                _thr = _thr * 2;
            }
            if (_base == 0)
            {
                _thr = _thr - 1;
            }
            return _thr;
        }

        public static int GetRuntime(string BenchName)
        {
            if (BenchName == "XMRSTAKRX")
            {
                return BenchMaestro.Properties.Settings.Default.RunTimeXMRSTAKRX;
            }
            else if (BenchName.StartsWith("CPUMINER"))
            {
                return BenchMaestro.Properties.Settings.Default.RunTimeCPUMINER;
            }
            return 180;
        }
        public static string GetCustomLabel()
        {
            string CPPCLabel = "";
            for (int i = 0; i < systemInfo.CPPCCustomOrder.Length; i++)
            {
                CPPCLabel += String.Format("{0} ", systemInfo.CPPCCustomOrder[i]);
                if (i != systemInfo.CPPCCustomOrder.Length - 1) CPPCLabel += ", ";

            }
            return CPPCLabel;
        }
        public static int GetIdleStableTime()
        {
            return BenchMaestro.Properties.Settings.Default.IdleStableTime;
        }
        public static int GetIdleAbsoluteCPULimit()
        {
            return BenchMaestro.Properties.Settings.Default.IdleAbsoluteCPULimit;
        }
        public static int GetIdleStartTimeout()
        {
            return BenchMaestro.Properties.Settings.Default.IdleStartTimeout;
        }
        public static int GetIdleLowestLoad()
        {
            return BenchMaestro.Properties.Settings.Default.IdleLowestLoad;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            if (principal.IsInRole(WindowsBuiltInRole.Administrator) == false && principal.IsInRole(WindowsBuiltInRole.User) == true)
            {
                ProcessStartInfo objProcessInfo = new ProcessStartInfo();
                objProcessInfo.UseShellExecute = true;
                objProcessInfo.FileName = System.AppContext.BaseDirectory + Assembly.GetExecutingAssembly().GetName().Name + ".exe";
                //objProcessInfo.UseShellExecute = true;
                objProcessInfo.Verb = "runas";
                try
                {
                    Process proc = Process.Start(objProcessInfo);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"OnStartup Exception: {ex.Message}");
                }
            }

            base.OnStartup(e);

            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);


            Cpu test = new Cpu();

            MainWindow window = new();

            Version _version = Assembly.GetExecutingAssembly().GetName().Version;
            version = string.Format("v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build, _version.Revision);
            systemInfo.AppVersion = version;

            SettingsInit();

            RunsViewModel runs = new RunsViewModel();

            window.DataContext = new
            {
                runs,
                settings = BenchMaestro.Properties.Settings.Default,
                systemInfo
            };

            var name = Assembly.GetExecutingAssembly().GetName();
            _versionInfo = string.Format($"{name.Version.Major:0}.{name.Version.Minor:0}.{name.Version.Build:0}");

            AutoUpdater.ReportErrors = false;
            AutoUpdater.InstalledVersion = new Version(_versionInfo);
            AutoUpdater.DownloadPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Synchronous = true;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/mann1x/BenchMaestro/master/BenchMaestro/AutoUpdaterBenchMaestro.json");

            HWMonitor.Init();

            HWMStart();

            Trace.WriteLine($"OnStartup App End");

            window.ShowDialog();

        }

        public static void HWMStart()
        {
            thrMonitor = new Thread(RunHWM);
            thridMonitor = thrMonitor.ManagedThreadId;
            thrMonitor.Priority = ThreadPriority.Highest;

            hwmtimer.Enabled = true;

            thrMonitor.Start();
            hwmtimer.Start();

            Trace.WriteLine($"HWMonitor Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static int RunTask()
        {
            if (MultiRunning)
            {
                return 2;
            }
            if (TaskRunning || InterlockBench != 0)
            {
                return 1;
            }
            return 0;
            //thrBench = new Thread(BenchRun.RunBench);
            //thridBench = thrBench.ManagedThreadId;

        }
        public static void SettingsInit()
        {
            if (BenchMaestro.Properties.Settings.Default.CustomCPPC.Length > 0 && BenchMaestro.Properties.Settings.Default.CustomCPPC.Length == systemInfo.CPUCores)
            {
                string[] _split = BenchMaestro.Properties.Settings.Default.CustomCPPC.ToString().Split(", ");
                Trace.WriteLine($"RESTORE CUSTOMCPPC {BenchMaestro.Properties.Settings.Default.CustomCPPC}");
                int i = 0;
                systemInfo.CPPCCustomOrder = new int[_split.Length];
                foreach (string _core in _split)
                {
                    systemInfo.CPPCCustomOrder[i] = Convert.ToInt32(_core);
                    i++;
                }
            }
            else
            {
                systemInfo.CPPCCustomOrder = new int[systemInfo.CPUCores];
                Trace.WriteLine($"CPUCORES {systemInfo.CPUCores} SIZE CUSTOM {systemInfo.CPPCCustomOrder.Length}");
                Trace.WriteLine($"CPPCORDER {String.Join(", ", systemInfo.CPPCOrder)}");
                for (int i = 0; i < systemInfo.CPPCOrder.Length; ++i)
                {
                    Trace.WriteLine($"CPUCORES {systemInfo.CPPCOrder[i]} INDEX CUSTOM {i}");
                    systemInfo.CPPCCustomOrder[i] = systemInfo.CPPCOrder[i];
                }
                BenchMaestro.Properties.Settings.Default.CustomCPPC = String.Join(", ", systemInfo.CPPCCustomOrder);
                BenchMaestro.Properties.Settings.Default.Save();
            }

            systemInfo.CPPCActiveOrder = systemInfo.CPPCOrder;
            systemInfo.CPPCActiveLabel = systemInfo.CPPCLabel;
            if (BenchMaestro.Properties.Settings.Default.cbCustomCPPC)
            {
                systemInfo.CPPCActiveOrder = systemInfo.CPPCCustomOrder;
                systemInfo.CPPCActiveLabel = GetCustomLabel();
            }

            if (BenchMaestro.Properties.Settings.Default.Threads == null) BenchMaestro.Properties.Settings.Default.Threads = new StringCollection();

            Trace.WriteLine($"STMT {BenchMaestro.Properties.Settings.Default.BtnSTMT}");
            systemInfo.STMT = BenchMaestro.Properties.Settings.Default.BtnSTMT;
            systemInfo.CPPCCustomEnabled = BenchMaestro.Properties.Settings.Default.BtnSTMT;

            Process thisprocess = Process.GetCurrentProcess();

            thisprocess.ProcessorAffinity = (IntPtr)(1L << App.GetLastThread(0));

        }
        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                DownloadURL = json.url,
                Mandatory = new Mandatory
                {
                    Value = json.mandatory.value,
                    UpdateMode = json.mandatory.mode,
                    MinimumVersion = json.mandatory.minVersion
                },
                CheckSum = new CheckSum
                {
                    Value = json.checksum.value,
                    HashingAlgorithm = json.checksum.hashingAlgorithm
                }
            };
        }

        private static void Monitor_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //Trace.WriteLine("HWM ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockHWM, 1, 0);
            if (sync == 0)
            {
                HWMonitor.OnHWM(sender, e);
                InterlockHWM = 0;
                //Trace.WriteLine("HWM ELAPSED ON");
            }
        }
        public static void RunHWM()
        {

            Trace.WriteLine("RUN HWM");
            hwmtimer.Interval = HWMonitor.MonitoringPooling;
            hwmtimer.Elapsed += new ElapsedEventHandler(Monitor_ElapsedEventHandler);
            if (hwmtimer.Enabled)
            {
                Trace.WriteLine("START HWM");
                hwmtimer.Start();
            }

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (RunningProcess != -1) BenchRun.KillProcID(RunningProcess);

            HWMonitor.Close();

            hwmcts.Dispose();
            benchcts.Dispose();
            
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

    }
}
