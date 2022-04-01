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
using System.Windows.Media;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Archives;
using ZenStates.Core;

namespace BenchMaestro
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	/// 

	public partial class App : Application
	{
		public static DailyTraceListener dailylistener = new DailyTraceListener(@".\TraceLog.txt");
		
		internal const string mutexName = "Local\\BenchMaestro";
		internal static Mutex instanceMutex;
		internal bool bMutex;

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

		public static SystemInfo systemInfo;
		public static string version;
		public static string ss_filename;
		public static string _versionInfo;

		public static string ZenPTSubject = "";
		public static string ZenPTBody = "";

		public static DateTime TSRunStart = DateTime.Now;
		public static bool benchrunning = false;
		public static bool benchclosed = true;

		public static int BenchIterations = 1;
		public static int IterationPretime = 20;
		public static int IterationRuntime = 100;
		public static int IterationPostime = 5;
		public static DateTime IterationPretimeTS = DateTime.MinValue;
		public static DateTime IterationRuntimeTS = DateTime.MinValue;
		public static DateTime IterationPostimeTS = DateTime.MinValue;

		public static List<HWSensorItem> hwsensors;

		public static SolidColorBrush boxbrush1 = new SolidColorBrush();
		public static SolidColorBrush boxbrush2 = new SolidColorBrush();
		public static SolidColorBrush scorebrush = new SolidColorBrush();
		public static SolidColorBrush thrbgbrush = new SolidColorBrush();
		public static SolidColorBrush thrbrush1 = new SolidColorBrush();
		public static SolidColorBrush thrbrush2 = new SolidColorBrush();
		public static SolidColorBrush maxbrush = new SolidColorBrush();
		public static SolidColorBrush tempbrush = new SolidColorBrush();
		public static SolidColorBrush voltbrush = new SolidColorBrush();
		public static SolidColorBrush clockbrush1 = new SolidColorBrush();
		public static SolidColorBrush clockbrush2 = new SolidColorBrush();
		public static SolidColorBrush powerbrush = new SolidColorBrush();
		public static SolidColorBrush additionbrush = new SolidColorBrush();
		public static SolidColorBrush detailsbrush = new SolidColorBrush();
		public static SolidColorBrush blackbrush = new SolidColorBrush();
		public static SolidColorBrush whitebrush = new SolidColorBrush();
		public static Thickness thickness;

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
		public static void SetCPUSource(HWSensorSource _source)
		{
			HWMonitor.CPUSource = _source;
			if (_source == HWSensorSource.Zen)
			{
				App.systemInfo.CPUSensorsSource = "Zen PowerTable";
			}
			if (_source == HWSensorSource.Libre)
			{
				App.systemInfo.CPUSensorsSource = "LibreHardwareMonitor";
			}
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
			instanceMutex = new Mutex(true, mutexName, out bMutex);

			if (!bMutex)
			{
				InteropMethods.PostMessage((IntPtr)InteropMethods.HWND_BROADCAST, InteropMethods.WM_SHOWME,
					IntPtr.Zero, IntPtr.Zero);
				Current.Shutdown();
				Environment.Exit(0);
			}

			GC.KeepAlive(instanceMutex);

			Trace.Listeners.Add(dailylistener);

			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);

			if (principal.IsInRole(WindowsBuiltInRole.Administrator) == false && principal.IsInRole(WindowsBuiltInRole.User) == true)
			{
				ProcessStartInfo objProcessInfo = new ProcessStartInfo();
				objProcessInfo.UseShellExecute = true;
				objProcessInfo.FileName = System.AppContext.BaseDirectory + Assembly.GetExecutingAssembly().GetName().Name + ".exe";
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

			systemInfo = new();

			MainWindow window = new();

			Version _version = Assembly.GetExecutingAssembly().GetName().Version;
			version = string.Format("v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
			systemInfo.AppVersion = version;

			SettingsInit();

			RunsViewModel runs = new RunsViewModel();

			window.DataContext = new
			{
				runs,
				settings = BenchMaestro.Properties.Settings.Default,
				systemInfo
			};

			HWMonitor.Init();

			InitColors();

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

			Trace.Flush();
		}
		private void InitColors()
		{
			/*
			//boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#648585");
			//boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#648585");
			boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			thrbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#CEEDE2");
			thrbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#C3E0D6");
			//maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#6D006 750E17");
			maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#A10008");
			tempbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D70");
			voltbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#8300A3");
			clockbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#140D4F");
			//clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#115C6B");
			clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#A31746"); 
			powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D95D04");
			//powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D16700");

			additionbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#431571");
			blackbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A");
			whitebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F4F4F6");
			*/
			//DOPO

			// BOX BACKGROUND ODD
			boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			// BOX BACKGROUND EVEN
			boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			// BOX BACKGROUND THREADS
			thrbgbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2F4F4F");
			// FONT SCORE RESULT
			scorebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#007300");
			// FONT NUM THREADS
			thrbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#CEEDE2");
			// FONT t THREADS
			thrbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#C3E0D6");
			// FONT ALL Max VALUES
			maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#A10008");
			// FONT CPU TEMP
			tempbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#115C6B");
			// FONT VOLTAGES
			voltbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#C51B54");
			// FONT AVERAGE CLOCK
			clockbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#251AED");
			// FONT MAX CLOCK
			clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#8300A3");
			// FONT POWER 
			powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#750E17");
			// FONT ADDITIONAL BOX (CCDS)
			additionbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#431571");
			// FONT EXPANDER DETAILS
			detailsbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F2DFC2");
			// FONT ALL BLACK (N/A, STARTED, FINISHED, LOAD, SCORE UNITS)
			blackbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2A2B34");
			// FONT ALL WHITE (BOX SCORE BG)
			whitebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F4F5F6");

			thickness = new Thickness(4, 3, 4, 3);

		}
		public static bool ExtractBench(string BenchArchive, string BenchPath, string BenchBinary)
		{
			try
			{
				if (!File.Exists(BenchArchive)) return false;
				var opts = new SharpCompress.Readers.ReaderOptions();
				opts.Password = "benchmaestro";
				using (var archive = SevenZipArchive.Open(BenchArchive, opts))
				{
					foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
					{
						entry.WriteToDirectory(BenchPath, new ExtractionOptions()
						{
							ExtractFullPath = true,
							Overwrite = true
						});
					}
				}
				if (File.Exists(BenchBinary)) return true;
				return false;
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ExtractBench exception: {ex}");
				return false;
			}
		}

	}
}
