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
    public partial class CPUMINERSSE42Window 
    {
        static string Benchname = "CPUMINERSSE42";

        string BenchBinary = @".\Benchmarks\cpuminer-opt-3.19.6\cpuminer-aes-sse42.exe";
        string BenchArchive = @".\Benchmarks\cpuminer-opt-3.19.6\cpuminer-aes-sse42.7z";
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

        SolidColorBrush boxbrush1 = new SolidColorBrush();
        SolidColorBrush boxbrush2 = new SolidColorBrush();
        SolidColorBrush scorebrush = new SolidColorBrush();
        SolidColorBrush thrbgbrush = new SolidColorBrush();
        SolidColorBrush thrbrush1 = new SolidColorBrush();
        SolidColorBrush thrbrush2 = new SolidColorBrush();
        SolidColorBrush maxbrush = new SolidColorBrush();
        SolidColorBrush tempbrush = new SolidColorBrush();
        SolidColorBrush voltbrush = new SolidColorBrush();
        SolidColorBrush clockbrush1 = new SolidColorBrush();
        SolidColorBrush clockbrush2 = new SolidColorBrush();
        SolidColorBrush powerbrush = new SolidColorBrush();
        SolidColorBrush additionbrush = new SolidColorBrush();
        SolidColorBrush detailsbrush = new SolidColorBrush();
        SolidColorBrush blackbrush = new SolidColorBrush();
        SolidColorBrush whitebrush = new SolidColorBrush();

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
            DependencyProperty.Register("WinTitle", typeof(string), typeof(CPUMINERSSE42Window), new UIPropertyMetadata($"BenchMaestro-{Benchname}"));
        public CPUMINERSSE42Window()
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



            DataContext = new
            {
                //runs = VM,
                settings = BenchMaestro.Properties.Settings.Default,
                systemInfo = App.systemInfo,
                benchsettings = Properties.SettingsCPUMINERSSE42.Default,
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
            Trace.WriteLine($"Window Initialized {Properties.SettingsCPUMINERSSE42.Default.Initialized}");
            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, Width);
            SetValue(MinHeightProperty, Height);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //ClearValue(SizeToContentProperty);


            if (Properties.SettingsCPUMINERSSE42.Default.Initialized)
            {
                Trace.WriteLine($"Restoring Window Position {Properties.SettingsCPUMINERSSE42.Default.Top} {Properties.SettingsCPUMINERSSE42.Default.Left} {Properties.SettingsCPUMINERSSE42.Default.Height} {Properties.SettingsCPUMINERSSE42.Default.Width} {Properties.SettingsCPUMINERSSE42.Default.Maximized}");

                WindowState = WindowState.Normal;
                Top = Properties.SettingsCPUMINERSSE42.Default.Top;
                Left = Properties.SettingsCPUMINERSSE42.Default.Left;
                Height = Properties.SettingsCPUMINERSSE42.Default.Height;
                Width = Properties.SettingsCPUMINERSSE42.Default.Width;
                if (Properties.SettingsCPUMINERSSE42.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                CenterWindowOnScreen();
                Properties.SettingsCPUMINERSSE42.Default.Initialized = true;
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

            if (!Properties.SettingsCPUMINERSSE42.Default.Initialized) SizeToContent = SizeToContent.WidthAndHeight;
            Properties.SettingsCPUMINERSSE42.Default.Initialized = true;
            Properties.SettingsCPUMINERSSE42.Default.Save();
            Trace.WriteLine($"Saving Initialized ");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.SettingsCPUMINERSSE42.Default.Initialized)
            {
                SaveWinPos();
            }
            e.Cancel = true;
            this.Hide();

        }
        private void SaveWinPos()
        {

            if (WindowState == WindowState.Maximized)
            {
                Properties.SettingsCPUMINERSSE42.Default.Top = RestoreBounds.Top;
                Properties.SettingsCPUMINERSSE42.Default.Left = RestoreBounds.Left;
                Properties.SettingsCPUMINERSSE42.Default.Height = RestoreBounds.Height;
                Properties.SettingsCPUMINERSSE42.Default.Width = RestoreBounds.Width;
                Properties.SettingsCPUMINERSSE42.Default.Maximized = true;
            }
            else
            {
                Properties.SettingsCPUMINERSSE42.Default.Top = Top;
                Properties.SettingsCPUMINERSSE42.Default.Left = Left;
                Properties.SettingsCPUMINERSSE42.Default.Height = Height;
                Properties.SettingsCPUMINERSSE42.Default.Width = Width;
                Properties.SettingsCPUMINERSSE42.Default.Maximized = false;
            }
            Properties.SettingsCPUMINERSSE42.Default.Save();
            Trace.WriteLine($"Saving Window Position {Properties.SettingsCPUMINERSSE42.Default.Top} {Properties.SettingsCPUMINERSSE42.Default.Left} {Properties.SettingsCPUMINERSSE42.Default.Height} {Properties.SettingsCPUMINERSSE42.Default.Width} {Properties.SettingsCPUMINERSSE42.Default.Maximized}");
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
       
        private BenchScore GetRunForThreads(List<BenchScore> _scoreRun, int _thrds)
        {
            foreach (BenchScore _run in _scoreRun)
            {
                if (_run.Threads == _thrds) return _run;
            }
            return new BenchScore(_thrds, Benchname);
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
                    Thickness thickness = new Thickness(4, 3, 4, 3);
                    ScoreList.Children.Clear();
                    ScoreList.ColumnDefinitions.Clear();

                    int[] threads = App.GetThreads();

                    foreach (int _threads in threads)
                    {
                        ScoreList.ColumnDefinitions.Add(new ColumnDefinition { });
                    }

                    int _column = 0;

                    foreach (int _threads in threads)
                    {
                        BenchScore _run = GetRunForThreads(scoreRun, _threads);
                        _run.Threads = _threads;
                        _run.Benchname = Benchname;
                        _run.ConfigTag = ConfigTag.Text.Trim();
                        scoreRun.Add(_run);
                        int _row = 0;

                        TextBlock _header = new TextBlock { FontSize = 16, Background = thrbgbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_header, _column);
                        Grid.SetRow(_header, _row);
                        _header.TextAlignment = TextAlignment.Center;
                        _header.Inlines.Add(new Run { Text = $"{_threads}", FontWeight = FontWeights.Bold, Foreground = thrbrush1 });
                        _header.Inlines.Add(new Run { Text = "t", FontWeight = FontWeights.Normal, Foreground = thrbrush2 });
                        _header.Margin = thickness;
                        ScoreList.Children.Add(_header);
                        _row++;

                        TextBlock _score = new TextBlock { FontSize = 20, Background = whitebrush, Foreground = scorebrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_score, _column);
                        Grid.SetRow(_score, _row);
                        _score.TextAlignment = TextAlignment.Center;
                        _score.Inlines.Add(new Run { Text = "Queued", FontSize = 20, FontWeight = FontWeights.Bold});
                        _score.Margin = thickness;
                        ScoreList.Children.Add(_score);
                        _run.ScoreBox = _score;
                        _row++;

                        /// CPU TEMP

                        StackPanel _cputempstack = new StackPanel { Margin = thickness, Background = boxbrush1 };
                        _cputempstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cputempstack.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetColumn(_cputempstack, _column);
                        Grid.SetRow(_cputempstack, _row);

                        Grid _cputempgrid = new Grid { Margin = thickness };
                        _cputempgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cputempgrid.VerticalAlignment = VerticalAlignment.Stretch;
                        _run.CPUTempGrid = _cputempgrid;
                        Grid.SetColumn(_cputempgrid, _column);
                        Grid.SetRow(_cputempgrid, _row);

                        TextBlock _cputemp = new TextBlock { FontSize = 14, Background = boxbrush1, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_cputemp, _column);
                        Grid.SetRow(_cputemp, _row);
                        _cputemp.TextAlignment = TextAlignment.Center;
                        _cputemp.Margin = thickness;
                        _cputemp.Text = "N/A";
                        _cputempstack.Children.Add(_cputemp);
                        _run.CPUTempBox = _cputemp;
                        ScoreList.Children.Add(_cputempstack);
                        _row++;

                        /// CPU CLOCK
                        
                        StackPanel _cpuclockstack = new StackPanel { Margin = thickness , Background = boxbrush2 };
                        _cpuclockstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cpuclockstack.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetColumn(_cpuclockstack, _column);
                        Grid.SetRow(_cpuclockstack, _row);

                        Grid _cpuclockgrid = new Grid { Margin = thickness };
                        _cpuclockgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cpuclockgrid.VerticalAlignment = VerticalAlignment.Stretch;
                        _run.CPUClockGrid = _cpuclockgrid;
                        Grid.SetColumn(_cpuclockgrid, _column);
                        Grid.SetRow(_cpuclockgrid, _row);

                        TextBlock _cpuclock = new TextBlock { FontSize = 14, Background = boxbrush2, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_cpuclock, _column);
                        Grid.SetRow(_cpuclock, _row);
                        _cpuclock.TextAlignment = TextAlignment.Center;
                        _cpuclock.Margin = thickness;
                        _cpuclock.Text = "N/A";
                        _cpuclockstack.Children.Add(_cpuclock);
                        _run.CPUClockBox = _cpuclock;                       
                        ScoreList.Children.Add(_cpuclockstack);
                        _row++;

                        ///CPU POWER

                        StackPanel _cpupowerstack = new StackPanel { Margin = thickness, Background = boxbrush1 };
                        _cpupowerstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cpupowerstack.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetColumn(_cpupowerstack, _column);
                        Grid.SetRow(_cpupowerstack, _row);

                        Grid _cpupowergrid = new Grid { Margin = thickness };
                        _cpupowergrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _cpupowergrid.VerticalAlignment = VerticalAlignment.Stretch;
                        _run.CPUPowerGrid = _cpupowergrid;
                        Grid.SetColumn(_cpupowergrid, _column);
                        Grid.SetRow(_cpupowergrid, _row);

                        TextBlock _cpupowerblock = new TextBlock { FontSize = 14, Background = boxbrush1, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_cpupowerblock, _column);
                        Grid.SetRow(_cpupowerblock, _row);
                        _cpupowerblock.TextAlignment = TextAlignment.Center;
                        _cpupowerblock.Margin = thickness;
                        _cpupowerblock.Text = "N/A";
                        _cpupowerstack.Children.Add(_cpupowerblock);
                        _run.CPUPowerBox = _cpupowerblock;
                        ScoreList.Children.Add(_cpupowerstack);
                        _row++;

                        /// ADDITIONAL

                        StackPanel _additionalstack = new StackPanel { Margin = thickness, Background = boxbrush2 };
                        _additionalstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _additionalstack.VerticalAlignment = VerticalAlignment.Stretch;
                        Grid.SetColumn(_additionalstack, _column);
                        Grid.SetRow(_additionalstack, _row);

                        Grid _additionalgrid = new Grid { Margin = thickness };
                        _additionalgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _additionalgrid.VerticalAlignment = VerticalAlignment.Stretch;
                        _run.AdditionalGrid = _additionalgrid;
                        Grid.SetColumn(_additionalgrid, _column);
                        Grid.SetRow(_additionalgrid, _row);

                        TextBlock _additional = new TextBlock { FontSize = 14, Background = boxbrush2, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_additional, _column);
                        Grid.SetRow(_additional, _row);
                        _additional.TextAlignment = TextAlignment.Center;
                        _additional.Margin = thickness;
                        _additional.Text = "N/A";
                        _additionalstack.Children.Add(_additional);
                        _run.AdditionalBox = _additional;
                        ScoreList.Children.Add(_additionalstack);
                        _row++;

                        /// STARTED

                        TextBlock _started = new TextBlock { FontSize = 14, Background = boxbrush1, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_started, _column);
                        Grid.SetRow(_started, _row);
                        _started.TextAlignment = TextAlignment.Center;
                        _started.Margin = thickness;
                        _started.Text = "N/A";
                        ScoreList.Children.Add(_started);
                        _run.StartedBox = _started;
                        _row++;

                        /// FINISHED
                        
                        TextBlock _finished = new TextBlock { FontSize = 14, Background = boxbrush2, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        Grid.SetColumn(_finished, _column);
                        Grid.SetRow(_finished, _row);
                        _finished.TextAlignment = TextAlignment.Center;
                        _finished.Margin = thickness;
                        _finished.Text = "N/A";
                        ScoreList.Children.Add(_finished);
                        _run.FinishedBox = _finished;
                        _row++;

                        /// DETAILS

                        Expander _detailsexp = new Expander { Header = "Details", IsExpanded = false, FontSize = 14, Foreground = detailsbrush };
                        Grid.SetColumn(_detailsexp, _column);
                        Grid.SetRow(_detailsexp, _row);
                        _detailsexp.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _detailsexp.VerticalAlignment = VerticalAlignment.Stretch;
                        _detailsexp.Margin = thickness;
                        _detailsexp.MinHeight = 30;

                        StackPanel _detailspstack = new StackPanel { Margin = thickness, Background = boxbrush1 };
                        _detailspstack.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _detailspstack.Visibility = Visibility.Visible;

                        StackPanel _detailsstack = new StackPanel { Margin = thickness, Background = boxbrush1 };
                        _detailsstack.HorizontalAlignment = HorizontalAlignment.Center;
                        _detailsstack.Visibility = Visibility.Collapsed;
                        _detailsstack.SetBinding(HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type ScrollViewer}}, Path=ActualHeight}");

                        Grid _detailsgrid = new Grid { Margin = thickness};
                        _detailsgrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _detailsgrid.VerticalAlignment = VerticalAlignment.Stretch;
                        _detailsgrid.Visibility = Visibility.Collapsed;
                        _detailsgrid.SetBinding(HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");
                        _detailsgrid.ShowGridLines = false;

                        ScrollViewer _scroller = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, MinHeight = 100};
                        _scroller.Visibility = Visibility.Collapsed;
                        _scroller.Tag = "Details";
                        _scroller.SetBinding(HeightProperty, "{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type StackPanel}}, Path=ActualHeight}");

                        TextBlock _details = new TextBlock { FontSize = 14, Background = boxbrush1, Foreground = blackbrush, HorizontalAlignment = HorizontalAlignment.Stretch};
                        _details.TextAlignment = TextAlignment.Center;
                        _details.Text = "N/A";
                        _details.TextWrapping = TextWrapping.Wrap;
                        _run.DetailsBox = _details;

                        _run.DetailsPanel = _detailsstack;
                        _run.DetailsPPanel = _detailspstack;
                        _run.DetailsGrid = _detailsgrid;
                        _run.DetailsScroller = _scroller;
                        _scroller.Content = _detailsstack;
                        _detailsstack.Children.Add(_detailsgrid);
                        _detailspstack.Children.Add(_details);
                        _detailspstack.Children.Add(_scroller);
                        _detailsexp.Content = _detailspstack;
                        ScoreList.Children.Add(_detailsexp);

                        _row++;

                        _column++;
                    }

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

                    var gridLength1 = new GridLength(1.1, GridUnitType.Star);
                    var gridLength2 = new GridLength(1, GridUnitType.Star);

                    char maxchar = '\u2191';
                    char avgchar = '\u2259';
                    //char degreesc = '\u2103';
                    //char degreesf = '\u2109';
                    char degrees = '\u2103';

                    int _row = 0;


                    /// TEMP

                    TextBlock _textblock = App.CurrentRun.CPUTempBox;
                    Grid _gridblock = App.CurrentRun.CPUTempGrid;
                    _gridblock.VerticalAlignment = VerticalAlignment.Center;
                    _row = 0;

                    if (App.CurrentRun.CPUAvgTemp > 0)
                    {

                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgTemp, 1)}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = tempbrush });
                        _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = tempbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxTemp, 1)} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);
                    }

                    if (App.CurrentRun.CoresAvgTemp > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"Cores: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = tempbrush });
                        _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = tempbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxTemp, 1)} °C", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b); 
                        
                    }

                    ScoreList.Children.Add(_gridblock);

                    if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                    /// CLOCK

                    _textblock = App.CurrentRun.CPUClockBox;
                    _gridblock = App.CurrentRun.CPUClockGrid;
                    _gridblock.VerticalAlignment = VerticalAlignment.Center;

                    if (App.CurrentRun.CPUAvgClock > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto});

                        TextBlock _tb1a = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUAvgClock}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = clockbrush1 });
                        _tb1a.Inlines.Add(new Run { Text = $" MHz", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = clockbrush1 });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, 0);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        if (App.CurrentRun.CPUAvgLoad > 0)
                        {
                            TextBlock _tb1b = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                            _tb1b.Inlines.Add(new Run { Text = $" {avgchar} {Math.Round(App.CurrentRun.CPUAvgLoad, 2)}%", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = blackbrush });
                            _tb1b.Inlines.Add(new Run { Text = " Load", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = blackbrush });
                            Grid.SetColumn(_tb1b, 1);
                            Grid.SetRow(_tb1b, 0);
                            _tb1b.TextAlignment = TextAlignment.Left;
                            _gridblock.Children.Add(_tb1b);
                        }

                        _gridblock.RowDefinitions.Add(new RowDefinition { });

                        TextBlock _tb2a = new TextBlock {  Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb2a.Inlines.Add(new Run { Text = $"{App.CurrentRun.CPUMaxClock}", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = clockbrush2 });
                        _tb2a.Inlines.Add(new Run { Text = $" MHz {maxchar}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = clockbrush2 });
                        Grid.SetColumn(_tb2a, 0);
                        Grid.SetRow(_tb2a, 1);
                        _tb2a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb2a);

                        if (App.CurrentRun.CPUMaxLoad > 0)
                        {
                            TextBlock _tb2b = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                            _tb2b.Inlines.Add(new Run { Text = $" {Math.Round(App.CurrentRun.CPUMaxLoad, 1)} % Load", FontSize = 9, FontWeight = FontWeights.Normal, Foreground = blackbrush });
                            Grid.SetColumn(_tb2b, 1);
                            Grid.SetRow(_tb2b, 1);
                            _tb2b.TextAlignment = TextAlignment.Left;
                            _gridblock.Children.Add(_tb2b);
                        }
                    }

                    ScoreList.Children.Add(_gridblock);

                    if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                    /// POWER

                    _textblock = App.CurrentRun.CPUPowerBox;
                    _gridblock = App.CurrentRun.CPUPowerGrid;
                    _gridblock.VerticalAlignment = VerticalAlignment.Center;

                    _row = 0;

                    if (App.CurrentRun.CPUAvgPower > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgPower, 1)}", FontSize = 17, FontWeight = FontWeights.Bold, Foreground = powerbrush });
                        _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = powerbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        if (App.CurrentRun.CPUMaxPower > 0)
                        {
                            TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                            _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxPower, 1)} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                            Grid.SetColumn(_tb1b, 1);
                            Grid.SetRow(_tb1b, _row);
                            _tb1b.TextAlignment = TextAlignment.Left;
                            _gridblock.Children.Add(_tb1b);
                        }

                    }

                    if (App.CurrentRun.CoresAvgPower > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"Cores: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = powerbrush });
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgPower, 1)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = powerbrush });
                        _tb1a.Inlines.Add(new Run { Text = " W ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = powerbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxPower, 1)} W", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    if (App.CurrentRun.CPUAvgVoltage > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"vCore: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CPUAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CPUMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    if (App.CurrentRun.CoresAvgVoltage > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"VIDs: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CoresAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CoresMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    if (App.CurrentRun.SOCAvgVoltage > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"SoC: ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.SOCAvgVoltage, 3)}", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = voltbrush });
                        _tb1a.Inlines.Add(new Run { Text = " V ", FontSize = 13, FontWeight = FontWeights.Normal, Foreground = voltbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush1, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.SOCMaxVoltage, 3)} V", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);
                    }

                    ScoreList.Children.Add(_gridblock);

                    if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                    /// ADDITIONAL

                    _textblock = App.CurrentRun.AdditionalBox;
                    _gridblock = App.CurrentRun.AdditionalGrid;
                    _gridblock.VerticalAlignment = VerticalAlignment.Center;

                    _row = 0;

                    if (App.CurrentRun.CCDSAvgTemp > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        TextBlock _tb1a = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"CCDs Avg: ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{Math.Round(App.CurrentRun.CCDSAvgTemp, 1)} {degrees}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    if (App.CurrentRun.CCD1AvgTemp > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"CCD1: {Math.Round(App.CurrentRun.CCD1AvgTemp, 1)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD1MaxTemp, 1)} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    if (App.CurrentRun.CCD2AvgTemp > 0)
                    {
                        if (_gridblock.ColumnDefinitions.Count == 0)
                        {
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength1 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                        }
                        _gridblock.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        _row++;

                        TextBlock _tb1a = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1a.Inlines.Add(new Run { Text = $"CCD2: {Math.Round(App.CurrentRun.CCD2AvgTemp, 1)}", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        _tb1a.Inlines.Add(new Run { Text = $" {degrees} ", FontSize = 12, FontWeight = FontWeights.Normal, Foreground = additionbrush });
                        Grid.SetColumn(_tb1a, 0);
                        Grid.SetRow(_tb1a, _row);
                        _tb1a.TextAlignment = TextAlignment.Right;
                        _gridblock.Children.Add(_tb1a);

                        TextBlock _tb1b = new TextBlock { Background = boxbrush2, VerticalAlignment = VerticalAlignment.Center };
                        _tb1b.Inlines.Add(new Run { Text = $"{maxchar} {Math.Round(App.CurrentRun.CCD2MaxTemp, 1)} {degrees}", FontSize = 11, FontWeight = FontWeights.Normal, Foreground = maxbrush });
                        Grid.SetColumn(_tb1b, 1);
                        Grid.SetRow(_tb1b, _row);
                        _tb1b.TextAlignment = TextAlignment.Left;
                        _gridblock.Children.Add(_tb1b);

                    }

                    ScoreList.Children.Add(_gridblock);

                    if (_gridblock.RowDefinitions.Count > 0) _textblock.Text = "";

                    /// DETAILS

                    StackPanel _stackpanel = App.CurrentRun.DetailsPanel;
                    StackPanel _stackppanel = App.CurrentRun.DetailsPPanel;
                    ScrollViewer _scroller = App.CurrentRun.DetailsScroller;
                    _textblock = App.CurrentRun.DetailsBox;
                    _gridblock = App.CurrentRun.DetailsGrid;
                    _gridblock.MinHeight = 200;

                    _row = 0;

                    void AddDetails(List<DetailsGrid> _thislist, string _header)
                    {

                        Trace.WriteLine($"Start {_header}");

                        Thickness dthickness = new Thickness(4, 3, 4, 3);
                        GridLength _rowheigth = new GridLength(1, GridUnitType.Star);

                        if (_thislist == null) return;

                        if (_thislist.Any())
                        {
                            _stackpanel.Visibility = Visibility.Visible;
                            _scroller.Visibility = Visibility.Visible;
                            _gridblock.Visibility = Visibility.Visible;
                            _textblock.Visibility = Visibility.Collapsed;

                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });
                            _gridblock.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength2 });

                            _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });

                            TextBlock _tbh = new TextBlock { Margin = dthickness, Background = boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                            _tbh.Inlines.Add(new Run { Text = _header, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = clockbrush1 });
                            Grid.SetColumn(_tbh, 0);
                            Grid.SetRow(_tbh, _row);
                            Grid.SetColumnSpan(_tbh, 6);
                            _tbh.TextAlignment = TextAlignment.Center;
                            _gridblock.Children.Add(_tbh);

                            _row++;
                                               
                        

                            int _colspan = 1;
                            if (_thislist.Count == 1) _colspan = 2;
                            int _index = 1;
                            int _col = 0;

                            foreach (DetailsGrid _item in _thislist)
                            {
                                string Label = _item.Label.ToString();
                                string Val1 = _item.Val1.ToString();
                                string Val2 = _item.Val2.ToString();
                                String.Format("{0:" + _item.Format + "}", Val1);
                                String.Format("{0:" + _item.Format + "}", Val2);
                                FontWeight _weight = FontWeights.Normal;
                                if (_item.Bold) _weight = FontWeights.Bold;

                                TextBlock _tb1a = new TextBlock { Margin = dthickness, Background = boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                                _tb1a.Inlines.Add(new Run { Text = Label, FontSize = 9, FontWeight = _weight, Foreground = blackbrush });
                                Grid.SetColumn(_tb1a, _col);
                                Grid.SetRow(_tb1a, _row);
                                Grid.SetColumnSpan(_tb1a, _colspan);
                                _tb1a.TextAlignment = TextAlignment.Left;
                                _gridblock.Children.Add(_tb1a);

                                _col++;
                                if (_thislist.Count == 1) _col++;

                                TextBlock _tb1b = new TextBlock { Margin = dthickness, Background = boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                                _tb1b.Inlines.Add(new Run { Text = Val1, FontSize = 9, FontWeight = _weight, Foreground = blackbrush });
                                Grid.SetColumn(_tb1b, _col);
                                Grid.SetRow(_tb1b, _row);
                                Grid.SetColumnSpan(_tb1b, _colspan);
                                _tb1b.TextAlignment = TextAlignment.Right;
                                _gridblock.Children.Add(_tb1b);

                                _col++;
                                if (_thislist.Count == 1) _col++;

                                TextBlock _tb1c = new TextBlock { Margin = dthickness, Background = boxbrush1, HorizontalAlignment = HorizontalAlignment.Stretch };
                                _tb1c.Inlines.Add(new Run { Text = Val2, FontSize = 9, FontWeight = _weight, Foreground = blackbrush });
                                Grid.SetColumn(_tb1c, _col);
                                Grid.SetRow(_tb1c, _row);
                                Grid.SetColumnSpan(_tb1c, _colspan);
                                _tb1c.TextAlignment = TextAlignment.Right;
                                _gridblock.Children.Add(_tb1c);

                                _col++;
                                _index++;

                                if (_index % 2 == 1 || _thislist.Count == 1)
                                {
                                    _col = 0;
                                    _row++;
                                    _gridblock.RowDefinitions.Add(new RowDefinition { Height = _rowheigth });
                                    Trace.WriteLine($"Add Row {_item.Label}");
                                }
                                Trace.WriteLine($"Finish {_header}");

                            }
                        }

                    }

                    AddDetails(App.CurrentRun.CPUCoresEffClocks, $"Cores Effective Clocks MHz [ Core - Average - Max ]");
                    AddDetails(App.CurrentRun.CPUCoresClocks, $"Cores Clocks MHz [ Core - Average - Max ]");
                    AddDetails(App.CurrentRun.CPUCoresTemps, $"Cores Temps {degrees} [ Core - Average - Max ]");
                    AddDetails(App.CurrentRun.CPUCoresPower, $"Cores Power Watt [ Core - Average - Max ]");
                    AddDetails(App.CurrentRun.CPUCoresVoltages, $"Cores VIDs Volt [ Core - Average - Max ]");
                    AddDetails(App.CurrentRun.CPULogicalsLoad, $"CPU Load % [ Thread - Average - Max ]");
                    AddDetails(App.CurrentRun.CPUCoresScores, $"Cores Scores [ Core - Average - Max ]");

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

                    BenchScore _scoreRun = GetRunForThreads(scoreRun, _thrds);

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
