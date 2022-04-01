using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using System.Timers;
using BenchMaestro.Windows;
using SharpCompress;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using System.Windows.Interop;
using System.Globalization;

namespace BenchMaestro
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    using Module1 = BenchModule1;
    public partial class CPUMINERSSE2Window 
    {
        static string Benchname = "CPUMINERSSE2";

        string BenchBinary = @".\Benchmarks\cpuminer-opt-3.19.6\cpuminer-sse2.exe";
        string BenchArchive = @".\Benchmarks\cpuminer-opt-3.19.6\cpuminer-sse2.7z";
        string BenchPath = @".\Benchmarks\cpuminer-opt-3.19.6\";
        string BenchArgs = $" --benchmark --no-color --time-limit=###runtime### --threads=###threads### --cpu-affinity ###affinity### --algo=scrypt:512 --no-redirect --no-extranonce --no-stratum --no-gbt --no-getwork --no-longpoll --stratum-keepalive";
        bool BenchArchived = true;

        DateTime TSRunStart = DateTime.Now;

        int BenchCurrentIteration = 0;
        int BenchIterations = 1;
        int IterationRuntime = 20;
        int IterationPretime = App.GetIdleStableTime();
        int IterationPostime = 5;

        bool parseStatus = true;
        string parseMsg = "";
        double parseDouble = 0;
        string parseString1 = "";
        string parseString2 = "";
        bool _benchrunning = false;
        bool _benchclosed = false;

        string RunLog;

        List<BenchScore> scoreRun = new List<BenchScore>();


        const int WM_SIZING = 0x214;
        const int WM_EXITSIZEMOVE = 0x232;
        private static bool WindowWasResized = false;

        public string WinTitle
        {
            get { return (string)GetValue(WinTitleProperty); }
            set { SetValue(WinTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(CPUMINERSSE2Window), new UIPropertyMetadata($"BenchMaestro-{Benchname}"));
        public CPUMINERSSE2Window()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            
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

            DataContext = new
            {
                //runs = VM,
                settings = BenchMaestro.Properties.Settings.Default,
                systemInfo = App.systemInfo,
                benchsettings = Properties.SettingsCPUMINERSSE2.Default,
                ProgressBar
            };

            SystemParameters.StaticPropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SystemParameters.WorkArea))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MaxHeight = SystemParameters.WorkArea.Height;
                        Height = SystemParameters.WorkArea.Height;
                        WindowState = WindowState.Normal;  // Updates the windows new sizes
                        WindowState = WindowState.Maximized;
                    });
                }
            };

        }
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Trace.WriteLine($"Window Initialized {Properties.SettingsCPUMINERSSE2.Default.Initialized}");
            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, Width);
            SetValue(MinHeightProperty, Height);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //ClearValue(SizeToContentProperty);


            if (Properties.SettingsCPUMINERSSE2.Default.Initialized)
            {
                Trace.WriteLine($"Restoring Window Position {Properties.SettingsCPUMINERSSE2.Default.Top} {Properties.SettingsCPUMINERSSE2.Default.Left} {Properties.SettingsCPUMINERSSE2.Default.Height} {Properties.SettingsCPUMINERSSE2.Default.Width} {Properties.SettingsCPUMINERSSE2.Default.Maximized}");

                WindowState = WindowState.Normal;
                Top = Properties.SettingsCPUMINERSSE2.Default.Top;
                Left = Properties.SettingsCPUMINERSSE2.Default.Left;
                Height = Properties.SettingsCPUMINERSSE2.Default.Height;
                Width = Properties.SettingsCPUMINERSSE2.Default.Width;
                if (Properties.SettingsCPUMINERSSE2.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                CenterWindowOnScreen();
                Properties.SettingsCPUMINERSSE2.Default.Initialized = true;
                SaveWinPos();

            }

        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static IEnumerable<T> FindVisualParent<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(depObj);
                if (parent != null && parent is T)
                {
                    yield return (T)parent;
                }

                foreach (T parentOfparent in FindVisualParent<T>(parent))
                {
                    yield return parentOfparent;
                }
            }
        }
        private void UpdateConfigTag(Object sender, DataTransferEventArgs args)
        {
            Trace.WriteLine($"UpdateConfigTag to {ConfigTag.Text.Trim()}");
            WinTitle = $"BenchMaestro-{Benchname}-{ConfigTag.Text.Trim()}";
        }


        private void Window_SizeChanged(object sender, EventArgs e)
        {

            if (!Properties.SettingsCPUMINERSSE2.Default.Initialized) SizeToContent = SizeToContent.WidthAndHeight;
            Properties.SettingsCPUMINERSSE2.Default.Initialized = true;
            Properties.SettingsCPUMINERSSE2.Default.Save();
            Trace.WriteLine($"Saving Initialized ");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ( Properties.SettingsCPUMINERSSE2.Default.Initialized) {

                SaveWinPos();
            }
            e.Cancel = true;
            this.Hide();

        }
        private void SaveWinPos()
        {

            if (WindowState == WindowState.Maximized)
            {
                Properties.SettingsCPUMINERSSE2.Default.Top = RestoreBounds.Top;
                Properties.SettingsCPUMINERSSE2.Default.Left = RestoreBounds.Left;
                Properties.SettingsCPUMINERSSE2.Default.Height = RestoreBounds.Height;
                Properties.SettingsCPUMINERSSE2.Default.Width = RestoreBounds.Width;
                Properties.SettingsCPUMINERSSE2.Default.Maximized = true;
            }
            else
            {
                Properties.SettingsCPUMINERSSE2.Default.Top = Top;
                Properties.SettingsCPUMINERSSE2.Default.Left = Left;
                Properties.SettingsCPUMINERSSE2.Default.Height = Height;
                Properties.SettingsCPUMINERSSE2.Default.Width = Width;
                Properties.SettingsCPUMINERSSE2.Default.Maximized = false;
            }
            Properties.SettingsCPUMINERSSE2.Default.Save();
            Trace.WriteLine($"Saving Window Position {Properties.SettingsCPUMINERSSE2.Default.Top} {Properties.SettingsCPUMINERSSE2.Default.Left} {Properties.SettingsCPUMINERSSE2.Default.Height} {Properties.SettingsCPUMINERSSE2.Default.Width} {Properties.SettingsCPUMINERSSE2.Default.Maximized}");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WinTitle = $"BenchMaestro-{App.version}-{Benchname}-{ConfigTag.Text.Trim()}";
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZING)
            {
                if (WindowWasResized == false)
                {
                    WindowWasResized = true;
                }
            }

            if (msg == WM_EXITSIZEMOVE)
            {
                if (WindowWasResized == true)
                {
                    var _check = App.CurrentRun;
                    if (_check != null)
                    {                        
                        IEnumerable<ScrollViewer> elements = FindVisualChildren<ScrollViewer>(this).Where(x => x.Tag != null && x.Tag.ToString().StartsWith("Details"));
                        foreach (ScrollViewer sv in elements)
                        {

                            double _scrollh = Height - sv.TranslatePoint(new Point(0, 0), null).Y;
                            double _scrollth = sv.TranslatePoint(new Point(0, 0), null).Y;
                            //Trace.WriteLine($"_scroller tTP={App.CurrentRun.DetailsBox.TranslatePoint(new Point(0, this.Height), this)} TP={App.CurrentRun.DetailsScroller.TranslatePoint(new Point(0, 0), null)} H={App.CurrentRun.DetailsScroller.ActualHeight} WH={this.Height}");
                            IEnumerable<Expander> expanders = FindVisualParent<Expander>(sv);
                            bool expanded = false;
                            Expander exp = expanders.FirstOrDefault();
                            if (exp != null) expanded = exp.IsExpanded;
                            if (_scrollh >= sv.ExtentHeight+8) _scrollh = sv.ExtentHeight+8;
                            if (_scrollh > sv.MinHeight && _scrollh < _scrollth && expanded) sv.Height = _scrollh;
                            Trace.WriteLine($"_scroller aH={sv.ActualHeight} rH={_scrollh} tH={_scrollth} isexpanded={expanded} expcount={expanders.Count()}");
                        }
                    }
                    WindowWasResized = false;
                }
            }

            return IntPtr.Zero;
        }


        private void ButtonScreenshot_Click(object sender, RoutedEventArgs e)
        {
            var screenshot = new Screenshot();
            var bitmap = screenshot.CaptureActiveWindow();

            App.ss_filename = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_") + WinTitle + ".png";

            using (var saveWnd = new SaveWindow(bitmap))
            {
                saveWnd.Owner = this;
                saveWnd.ShowDialog();
                screenshot.Dispose();
            }
        }

        public void SetStart()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                BtnStartLabel.Text = "Start";
            }));
        }
        public void SetEnabled()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                BtnStartLabel.IsEnabled = true;
            }));
        }
      
        public void UpdateStarted()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                TextBlock _textblock = App.CurrentRun.StartedBox;
                _textblock.FontSize = 12;
                _textblock.Text = $"Started: {App.CurrentRun.Started}";
                if (App.CurrentRun.StartedTemp > -999) _textblock.Text += $" @ {App.CurrentRun.StartedTemp} °C";
            }));
        }
        public void STCWin()
        {
            double _height = Height;
            double _width = Width;
            SizeToContent = SizeToContent.WidthAndHeight;
            double _stcheight = Height;
            double _stcwidth = Width;
            if (_height > _stcheight) Height = _height;
            if (_width > _stcwidth) Width = _width;

        }
        public void UpdateFinished(string _exitstatus = "")
        {
            Dispatcher.Invoke((Action)(() =>
            {
                TextBlock _textblock = App.CurrentRun.FinishedBox;
                _textblock.FontSize = 12;
                _textblock.Text = $"Finish: {App.CurrentRun.Finished}";
                if (_exitstatus.Length > 0)
                {
                    _textblock.Inlines.Add(new LineBreak { });
                    _textblock.Inlines.Add(new Run { Text = _exitstatus, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                }
            }));
        }
        public void UpdateScore(string _score = "")
        {
            Dispatcher.Invoke((Action)(() =>
            {
                TextBlock _textblock = App.CurrentRun.ScoreBox;
                _textblock.Text = "";
                if (_score.Length == 0)
                {
                    _score = $"{Math.Round(App.CurrentRun.Score, 2)}";
                    _textblock.Inlines.Add(new Run { Text = $"{_score}", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.Green });
                    _textblock.Inlines.Add(new Run { Text = $"  {App.CurrentRun.ScoreUnit}", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = Brushes.Black });
                }
                else
                {
                    _textblock.Inlines.Add(new Run { Text = $"{_score}", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.Green });
                }
            }));
        }
        public void UpdateRunStop()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                LoadingCircle.Visibility = Visibility.Collapsed;
                ProgressBar.Visibility = Visibility.Collapsed;
            }));
        }
        public void UpdateProgress(int value = -1)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                //App.CurrentRun.ProgressBar = $"Running benchmark iteration {BenchCurrentIteration}/{BenchIterations}";

                if (value >= 0)
                {
                    ProgressBar.Value = value;
                }
                else
                {
                    TimeSpan _runningspan = DateTime.Now - TSRunStart;

                    int _projectedruntime = (IterationPretime + IterationRuntime + IterationPostime + 5) * BenchIterations;

                    DateTime _endingts = TSRunStart.AddSeconds(_projectedruntime);

                    TimeSpan _remaining = _endingts - TSRunStart;

                    int _percentage = (int)_runningspan.TotalSeconds * 95 / _projectedruntime;

                    ProgressBar.Value = _percentage > 95 ? 95 : _percentage;

                    //Trace.WriteLine($"PB% {_percentage} REM {(int)_remaining.TotalSeconds - (int)_runningspan.TotalSeconds} ELA {(int)_runningspan.TotalSeconds} PROJ {_projectedruntime} PRE {IterationPretime} RUN {IterationRuntime} POST {IterationPostime} ITER {BenchCurrentIteration}");
                }
            }));
        }

        public void UpdateRunStart()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                LoadingCircle.Visibility = Visibility.Visible;
                ProgressBar.Visibility = Visibility.Visible;
            }));
        }

        public void UpdateRunSettings()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RunSettingsLabel.Visibility = Visibility.Visible;
                RunSettingsBlock.Inlines.Clear();
                if (App.CurrentRun.Runtime > 0) RunSettingsBlock.Inlines.Add(new Run { Text = $"Runtime {App.CurrentRun.Runtime} seconds" });
                if (App.CurrentRun.Algo.Length > 0)
                {
                    RunSettingsBlock.Inlines.Add(new LineBreak());
                    RunSettingsBlock.Inlines.Add(new Run { Text = $"Algo: {App.CurrentRun.Algo} with: {App.CurrentRun.AlgoFeatures} using: {App.CurrentRun.Features}"});
                    RunSettingsBlock.Inlines.Add(new LineBreak());
                    RunSettingsBlock.Inlines.Add(new Run { Text = $"Features CPU: {App.CurrentRun.CPUFeatures} SW: {App.CurrentRun.SWFeatures}" });
                }
            }));
        }

        private bool ExtractBench()
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
            catch(Exception ex)
            {
                Trace.WriteLine($"ExtractBench exception: {ex}");
                return false;
            }
        }

        private void StartBench(object sender, RoutedEventArgs e)
        {
            try 
            { 

                if (BtnStartLabel.Text == "Stop")
                {
                    BtnStartLabel.IsEnabled = false;
                    
                    UpdateMainStatus("Aborting run...");
                    // Request cancellation.

                    App.benchcts.Cancel();

                    Trace.WriteLine("Cancellation set in token source...");

                    Thread.Sleep(1000);

                    if (App.RunningProcess > 0)
                    {
                        Trace.WriteLine($"User aborted, killing ProcID={App.RunningProcess}");
                        BenchRun.KillProcID(App.RunningProcess);
                    }

                    try
                    {
                        App.mresbench.Wait(App.benchcts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Trace.WriteLine($"Benchmark canceled");
                    }

                    UpdateMainStatus("User aborted");

                    App.benchcts = new CancellationTokenSource();

                    App.TaskRunning = false;

                    BtnStartLabel.Text = "Start";
                }
                else
                {

                    int _runtask = App.RunTask();

                    if (_runtask == 1)
                    {
                        UpdateMainStatus("Another task is already running");
                        return;
                    }
                    else if (_runtask == 2) 
                    {
                        UpdateMainStatus("Multi Bench is already running");
                        return;
                    }

                    UpdateMainStatus("Starting benchmark...");

                    if (BenchArchived)
                    {
                        bool _extract = ExtractBench();

                        if (!_extract)
                        {
                            UpdateMainStatus("Failed to extract benchmark, check your AntiVirus");
                            return;
                        }
                    }

                    BtnStartLabel.Text = "Stop";

                    /*
                    TO BE FIXED
                    
                    bool _hwmonitor = false;

                    if (App.thrMonitor != null)
                    {
                        if (App.thrMonitor.IsAlive)
                        {
                            _hwmonitor = true;
                        }

                    }

                    if (!_hwmonitor)
                    {
                        Trace.WriteLine($"Restarting HWM Thread");
                        App.HWMStart();
                        Thread.Sleep(1000);
                    }

                    */

                    ScoreList.HorizontalAlignment = HorizontalAlignment.Stretch;
                    ScoreList.VerticalAlignment = VerticalAlignment.Center;
                    ScoreList.Children.Clear();
                    ScoreList.ColumnDefinitions.Clear();

                    int[] threads = App.GetThreads();

                    foreach (int _threads in threads)
                    {
                        ScoreList.ColumnDefinitions.Add(new ColumnDefinition { });
                    }

                    Module1.ScoresLayout(ScoreList, scoreRun, threads, Benchname, ConfigTag.Text.Trim());

                    App.thrBench = new Thread(StartBench);
                    App.thridBench = App.thrBench.ManagedThreadId;
                    App.thrBench.Start();


                    SizeToContent = SizeToContent.WidthAndHeight;
                    SetValue(MinWidthProperty, Width);
                    SetValue(MinHeightProperty, Height);
                    SizeToContent = SizeToContent.WidthAndHeight;

                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine($"Start Button Exception {ex}");
            }

        }

        public void UpdateMonitoring()
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.UpdateMonitoring2(ScoreList);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateMonitoring Exception: {e}");
            }
        }


        public void UpdateMainStatus(string _msgstatus)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                MainStatus.Text = $"{_msgstatus}";
                RunLog += $"\n\rMAIN STATUS MESSAGE: {_msgstatus}";
            }));
        }

        public (bool, string, double, string, string) ParseLogLine(string _line)
        {
            Trace.WriteLine($"BENCH LINE=={_line}");
            if (_line.Contains("invalid config"))
            {
                return (false, "CONFIG", 0, "", "");
            }
            string cpufeat_pattern = @"^CPU features: (?<cpufeatures>.*)";
            string algofeat_pattern = @"^Algo features: (?<algofeatures>.*)";
            string swfeat_pattern = @"^SW features: (?<swfeatures>.*)";
            string feat_pattern = @"^Starting miner with (?<features>.*)...";
            string start_pattern = @"^\[.*\] \d+ of \d+ miner threads started using '(?<algo>.*)' algorithm";
            string end_pattern = @"^\[.*\] Benchmark: (?<score>.*) (?<scoreunit>.*)";
            Regex start_rgx = new Regex(start_pattern, RegexOptions.Multiline);
            Match start_m = start_rgx.Match(_line);
            if (start_m.Success)
            {
                return (true, "START", 0, start_m.Groups[1].Value, "");
            }
            Regex feat_rgx = new Regex(feat_pattern, RegexOptions.Multiline);
            Match feat_m = feat_rgx.Match(_line);
            if (feat_m.Success)
            {
                return (true, "FEATURES", 0, feat_m.Groups[1].Value, "");
            }
            Regex cpufeat_rgx = new Regex(cpufeat_pattern, RegexOptions.Multiline);
            Match cpufeat_m = cpufeat_rgx.Match(_line);
            if (cpufeat_m.Success)
            {
                return (true, "CPUFEATURES", 0, cpufeat_m.Groups[1].Value, "");
            }
            Regex swfeat_rgx = new Regex(swfeat_pattern, RegexOptions.Multiline);
            Match swfeat_m = swfeat_rgx.Match(_line);
            if (swfeat_m.Success)
            {
                return (true, "SWFEATURES", 0, swfeat_m.Groups[1].Value, "");
            }
            Regex algofeat_rgx = new Regex(algofeat_pattern, RegexOptions.Multiline);
            Match algofeat_m = algofeat_rgx.Match(_line);
            if (algofeat_m.Success)
            {
                return (true, "ALGOFEATURES", 0, algofeat_m.Groups[1].Value, "");
            }
            Regex end_rgx = new Regex(end_pattern, RegexOptions.Multiline);
            Match end_m = end_rgx.Match(_line);
            if (end_m.Success)
            {
                string[] results = end_rgx.GetGroupNames();
                string score = "";
                string scoreunit = ""; 
                
                foreach (var name in results)
                {
                    Group grp = end_m.Groups[name];
                    if (name == "score" && grp.Value.Length > 0)
                    {
                        score = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                    if (name == "scoreunit" && grp.Value.Length > 0)
                    {
                        scoreunit = grp.Value.TrimEnd('\r', '\n').Trim();
                    }
                }
               
                if (score.Length > 0 && scoreunit.Length > 0)
                {
                    score = score.Replace(".", ",");
                    Trace.WriteLine($"LogLine End Result: {score} {scoreunit}");
                    double dscore = Convert.ToDouble(score);
                    return (true, "END", dscore, scoreunit, "");
                }
                else 
                {
                    Trace.WriteLine($"LogLine End Result error: {score} {scoreunit}");
                    return (false, "END", 0, "", "");
                }
            }
            return (true, "", 0, "", "");
        }
        private void RunningProcessClear(object sender, System.EventArgs e)
        {
            Trace.WriteLine(
                $"Exit time    : {App.BenchProc.ExitTime}\n" +
                $"Exit code    : {App.BenchProc.ExitCode}\n" +
                $"Elapsed time : {Math.Round((App.BenchProc.ExitTime - App.BenchProc.StartTime).TotalMilliseconds)}");
            App.RunningProcess = -1;
            if (App.BenchProc.ExitCode != 0 && _benchrunning)
            {
                UpdateMainStatus($"Benchmark execution error, exitcode: {App.BenchProc.ExitCode}");
                UpdateScore("Error");
                _benchrunning = false;
                HWMonitor.MonitoringParsed = true;
            }

        }
        public void RunBench()
        {

            try
            {
                App.TaskRunning = true;

                CancellationToken benchtoken = new CancellationToken();
                benchtoken = (CancellationToken)App.benchcts.Token;

                Trace.WriteLine($"RUN BENCH 1");

                App.hwmtimer.Interval = HWMonitor.MonitoringPooling;

                int[] threads = App.GetThreads();
                int CPUCores = App.systemInfo.CPUCores;
                int[] CPPC = App.systemInfo.CPPCActiveOrder;

                Trace.WriteLine($"RUN BENCH 1A");

                TSRunStart = DateTime.Now;
                DateTime IterationPretimeTS = DateTime.MinValue;
                DateTime IterationRuntimeTS = DateTime.MinValue;
                DateTime IterationPostimeTS = DateTime.MinValue;
                BenchIterations = threads.Count();
                IterationPretime = App.GetIdleStableTime();
                IterationRuntime = App.GetRuntime(Benchname);
                IterationPostime = 5;
                BenchCurrentIteration = 0;

                UpdateRunStart();

                Trace.WriteLine($"RUN BENCH 1B {threads.Count()}");

                foreach (int _thrds in threads)
                {
                    BenchCurrentIteration++;

                    IterationPretimeTS = DateTime.Now;

                    Trace.WriteLine($"RUN BENCH 2");

                    BenchScore _scoreRun = Module1.GetRunForThreads(scoreRun, _thrds, Benchname);

                    App.CurrentRun = _scoreRun;

                    _scoreRun.ClearRun();
                    _scoreRun.Runtime = IterationRuntime;

                    UpdateProgress();

                    if (benchtoken.IsCancellationRequested) return;

                    UpdateRunSettings();

                    HWMonitor.MonitoringStart = DateTime.MinValue;
                    HWMonitor.MonitoringEnd = DateTime.MinValue;
                    HWMonitor.MonitoringPause = false;
                    HWMonitor.MonitoringParsed = false;
                    HWMonitor.MonitoringBenchStarted = false;
                    HWMonitor.MonitoringStarted = false;
                    HWMonitor.MonitoringStopped = false;

                    string CPPCOrder = String.Join(", ", CPPC);

                    Trace.WriteLine($"CPPC: {CPPCOrder}");


                    int _cpu = 0;
                    int bitMask = 0;

                    for (int i = 0; i < _thrds; i++)
                    {
                        int _cppcidx = i;

                        if (i > CPUCores - 1) _cppcidx = i - CPUCores;

                        int _core = CPPC[_cppcidx] + 1;

                        if (App.systemInfo.HyperThreading)
                        {
                            _cpu = i < CPUCores ? CPPC[_cppcidx] * 2 : (CPPC[_cppcidx] * 2) + 1;
                        }
                        else
                        {
                            _cpu = CPPC[i];
                            if (!_scoreRun.RunCores.Contains(_cpu)) _scoreRun.RunCores.Add(_cpu);
                        }

                        int _cpu1 = _cpu + 1;

                        if (!_scoreRun.RunCores.Contains(_core)) _scoreRun.RunCores.Add(_core);
                        if (!_scoreRun.RunLogicals.Contains(_cpu1)) _scoreRun.RunLogicals.Add(_cpu1);

                        bitMask |= 1 << (_cpu);

                        Trace.WriteLine($"BitMask {bitMask}");
                        Trace.WriteLine($"Affinity (0 based) : {_cpu} (1): {_cpu1}");

                    }

                    Trace.WriteLine($"RunCores: {string.Join(", ", _scoreRun.RunCores.ToArray())}");
                    Trace.WriteLine($"RunLogicals: {string.Join(", ", _scoreRun.RunLogicals.ToArray())}");

                    string _args = BenchArgs.Replace("###runtime###", App.CurrentRun.Runtime.ToString());
                    _args = _args.Replace("###threads###", _thrds.ToString());
                    _args = _args.Replace("###affinity###", "0x" + bitMask.ToString("X8"));

                    App.BenchProc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = BenchBinary,
                            Arguments = _args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
                            //StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
                        }
                    };

                    HWMonitor.MonitoringPause = true;
                    HWMonitor.MonitoringIdle = true;

                    App.hwmtimer.Enabled = false;

                    HWMonitor.ReInit(true);

                    App.hwmtimer.Enabled = true;

                    HWMonitor.MonitoringPause = false;

                    Thread.Sleep(500);

                    bool IamIdling = false;

                    DateTime TSIdleStart = DateTime.Now;
                    DateTime TSIdleUpdated = DateTime.Now;

                    int IdleLowestTemp = HWMonitor.IdleCurrentCPUTemp;
                    int IdleAbsoluteCPULimit = App.GetIdleAbsoluteCPULimit();
                    int IdleLowestLoad = App.GetIdleLowestLoad();
                    int IdleStableTime = App.GetIdleStableTime();
                    int IdleStartTimeout = App.GetIdleStartTimeout();

                    while (!IamIdling)
                    {
                        if (benchtoken.IsCancellationRequested)
                        {
                            _scoreRun.Finished = DateTime.Now;
                            _scoreRun.ExitStatus = "Aborted by user";
                            UpdateScore("N/A");
                            UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                            UpdateFinished("Aborted by user");
                            Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                            return;
                        }
                        UpdateProgress();
                        Thread.Sleep(250);
                        TimeSpan _idlestart = DateTime.Now - TSIdleStart;
                        if (!HWMonitor.IdleCPUTempSensor)
                        {
                            Trace.WriteLine($"CPU Temp Sensor not found waiting static {HWMonitor.IdleStaticWait} ms");
                            UpdateMainStatus($"CPU Temp Sensor not found, waiting {TimeSpan.FromMilliseconds(HWMonitor.IdleStaticWait).TotalSeconds} seconds before start...");
                            int _slices = HWMonitor.IdleStaticWait / 100;
                            for (int i = 1; i <= _slices; i++)
                            {
                                if (benchtoken.IsCancellationRequested)
                                {
                                    _scoreRun.Finished = DateTime.Now;
                                    _scoreRun.ExitStatus = "Aborted by user";
                                    UpdateScore("N/A");
                                    UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                                    UpdateFinished("Aborted by user");
                                    Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                                    throw new OperationCanceledException();
                                }
                                UpdateProgress();
                                Trace.WriteLine($"Seelping for staticwait");
                                Thread.Sleep(100);
                            }
                            IamIdling = true;
                        }
                        else if (_idlestart.TotalSeconds > IdleStartTimeout)
                        {
                            UpdateMainStatus($"CPU idle scan timeout, starting benchmark");
                            Trace.WriteLine($"Check idling timeout: {_idlestart.TotalSeconds}");
                            _scoreRun.StartedTemp = HWMonitor.IdleCurrentCPUTemp;
                            IamIdling = true;
                        }
                        else
                        {
                            if (HWMonitor.IdleCurrentCPUTemp < IdleLowestTemp)
                            {
                                TSIdleUpdated = DateTime.Now;
                                IdleLowestTemp = HWMonitor.IdleCurrentCPUTemp;
                                Trace.WriteLine($"Check idling temp new lowest: {IdleLowestTemp}");
                            }
                            if (HWMonitor.IdleCurrentCPUTemp > IdleLowestTemp + HWMonitor.IdleHysteresis)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling temp higher the hysteresis: {HWMonitor.IdleCurrentCPUTemp}");
                            }
                            if (HWMonitor.IdleCurrentCPULoad > IdleLowestLoad)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling load too high: {HWMonitor.IdleCurrentCPULoad}");
                            }
                            if (HWMonitor.IdleCurrentCPUTemp > IdleAbsoluteCPULimit)
                            {
                                TSIdleUpdated = DateTime.Now;
                                Trace.WriteLine($"Check idling temp higher the absolute minimum: {HWMonitor.IdleCurrentCPUTemp}");
                            }
                            TimeSpan _idlestable = DateTime.Now - TSIdleUpdated;
                            Trace.WriteLine($"Waiting for idling CPU temperature: {HWMonitor.IdleCurrentCPUTemp} °C, still {Math.Round(IdleStableTime - _idlestable.TotalSeconds),0} seconds before start");
                            UpdateMainStatus($"Waiting for CPU idling : {HWMonitor.IdleCurrentCPUTemp} °C @ {HWMonitor.IdleCurrentCPULoad}% load, {Math.Round(IdleStableTime - _idlestable.TotalSeconds),0} seconds to start");
                            if (_idlestable.TotalSeconds > IdleStableTime)
                            {
                                _scoreRun.StartedTemp = HWMonitor.IdleCurrentCPUTemp;
                                _idlestart = DateTime.Now - TSIdleStart;
                                Trace.WriteLine($"Check idling success in: {_idlestart.TotalSeconds} stable for: {_idlestable.TotalSeconds} current temp: {HWMonitor.IdleCurrentCPUTemp} lowest temp: {IdleLowestTemp}");
                                IamIdling = true;
                            }

                        }
                    }

                    HWMonitor.MonitoringIdle = false;

                    HWMonitor.MonitoringPause = true;

                    HWMonitor.ReInit(false);
                    
                    HWMonitor.MonitoringPause = false;

                    DateTime _started = DateTime.Now;
                    _scoreRun.Started = _started;
                    UpdateStarted();

                    string _startstr = $"Benchmark {_thrds}T started at {_started}";
                    if (_scoreRun.StartedTemp > -999) _startstr += $" with CPU temperature at {_scoreRun.StartedTemp} °C";
                    Trace.WriteLine(_startstr);
                    UpdateMainStatus(_startstr);

                    App.BenchProc.EnableRaisingEvents = true;
                    App.BenchProc.Exited += new EventHandler(RunningProcessClear);

                    App.BenchProc.Start();

                    if (_thrds < App.systemInfo.CPULogicalProcessors)
                        App.BenchProc.PriorityClass = ProcessPriorityClass.AboveNormal;

                    HWMonitor.MonitoringPooling = HWMonitor.MonitoringPoolingFast;

                    BenchRun.SetRunProcID(App.BenchProc.Id);

                    parseStatus = true;
                    parseMsg = "";
                    parseDouble = 0;
                    parseString1 = "";
                    parseString2 = "";
                    _benchrunning = true;
                    _benchclosed = false;

                    App.BenchProc.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null)
                        {
                            _benchclosed = true;
                            Trace.WriteLine("BENCH FINISHED");
                        }
                        else
                        {
                            string _line = e.Data.ToString();
                            RunLog += _line;
                            (parseStatus, parseMsg, parseDouble, parseString1, parseString2) = ParseLogLine(_line);
                            
                            if (!parseStatus)
                            {
                                UpdateMainStatus($"Benchmark execution error: {parseMsg}");
                                UpdateScore("Error");
                                _benchrunning = false;
                                return;
                            }
                            else
                            {
                                if (parseMsg == "START")
                                {
                                    TimeSpan _pretimespan = DateTime.Now - IterationPretimeTS;
                                    IterationPretime = (int)_pretimespan.TotalSeconds;
                                    IterationRuntimeTS = DateTime.Now;
                                    HWMonitor.MonitoringBenchStarted = true;
                                    UpdateScore("Running...");
                                    UpdateMainStatus($"Benchmark {_thrds}t running...");
                                    Trace.WriteLine("Benchmark START");
                                    App.CurrentRun.Algo = parseString1.Trim();
                                    UpdateRunSettings();
                                    parseMsg = "";
                                }
                                if (parseMsg == "FEATURES")
                                {
                                    Trace.WriteLine($"Features: {parseString1}");
                                    App.CurrentRun.Features = parseString1.Trim();
                                    parseMsg = "";
                                }
                                if (parseMsg == "CPUFEATURES")
                                {
                                    Trace.WriteLine($"CPU Features: {parseString1}");
                                    App.CurrentRun.CPUFeatures = parseString1.Trim();
                                    parseMsg = "";
                                }
                                if (parseMsg == "SWFEATURES")
                                {
                                    Trace.WriteLine($"SW Features: {parseString1}");
                                    App.CurrentRun.SWFeatures = parseString1.Trim();
                                    parseMsg = "";
                                }
                                if (parseMsg == "ALGOFEATURES")
                                {
                                    Trace.WriteLine($"ALGO Features: {parseString1}");
                                    App.CurrentRun.AlgoFeatures = parseString1.Trim();
                                    parseMsg = "";
                                }
                                if (parseMsg == "END")
                                {
                                    TimeSpan _runtimespan = DateTime.Now - IterationRuntimeTS;
                                    IterationRuntime = (int)_runtimespan.TotalSeconds;
                                    IterationPostimeTS = DateTime.Now;
                                    _scoreRun.Score = parseDouble;
                                    _scoreRun.ScoreUnit = parseString1;
                                    UpdateScore();
                                    UpdateMainStatus("Benchmark finished");
                                    Trace.WriteLine("Benchmark END");
                                    parseMsg = "";
                                    _benchrunning = false;
                                }

                            }

                        }
                    };

                    App.BenchProc.BeginOutputReadLine();

                    while (_benchrunning)
                    {
                        if (benchtoken.IsCancellationRequested)
                        {
                            _scoreRun.Finished = DateTime.Now;
                            _scoreRun.ExitStatus = "Aborted by user";
                            //hwmcts.Cancel();
                            UpdateScore("N/A");
                            UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                            UpdateFinished("Aborted by user");
                            Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                            return;
                        }
                        UpdateProgress();

                        Thread.Sleep(500);
                    }

                    App.BenchProc.WaitForExit();

                    if (HWMonitor.MonitoringEnd == DateTime.MinValue) HWMonitor.MonitoringEnd = DateTime.Now;

                    DateTime _finished = DateTime.Now;
                    _scoreRun.Finished = _finished;
                    UpdateFinished();
                    Trace.WriteLine($"Finish: {_finished}");

                    while (!HWMonitor.MonitoringParsed && !benchtoken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    UpdateMonitoring();

                    TimeSpan _postimespan = DateTime.Now - IterationPostimeTS;
                    IterationPostime = (int)_postimespan.TotalSeconds;

                    UpdateProgress();

                    if (benchtoken.IsCancellationRequested)
                    {
                        UpdateScore("N/A");
                        UpdateMainStatus($"Benchmark aborted while running {_thrds}T");
                        UpdateFinished("Aborted by user");
                        Trace.WriteLine($"{Benchname} Out of Loop at Threads: {_thrds}");
                        return;
                    }
                    else
                    {
                        UpdateMainStatus($"Benchmark {_thrds}T finished at {_finished}");
                    }
                    _scoreRun.OutputLog = RunLog;
                }
                UpdateProgress(99);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{Benchname} Exception: {e}");
                UpdateScore("Error");
                UpdateMainStatus($"Benchmark run exception error: {e}");
            }
            finally
            {
                HWMonitor.MonitoringPause = false;
                HWMonitor.MonitoringIdle = false;
                HWMonitor.MonitoringParsed = false;
                HWMonitor.MonitoringBenchStarted = false;
                HWMonitor.MonitoringStarted = false;
                HWMonitor.MonitoringStopped = false;
                Trace.WriteLine($"{Benchname} Finally {App.RunningProcess}");
                if (App.RunningProcess != -1) BenchRun.KillProcID(App.RunningProcess);
                App.TaskRunning = false;
                if (BenchArchived) File.Delete(BenchBinary);
                SetStart();
                SetEnabled();
                UpdateRunStop();
            }

        }

        public void StartBench()
        {

            Trace.WriteLine("BENCH START");
            int sync = Interlocked.CompareExchange(ref App.InterlockBench, 1, 0);
            if (sync == 0)
            {
                Trace.WriteLine("BENCH START2");
                RunBench();
                App.InterlockBench = 0;
            }

        }


    }

}
