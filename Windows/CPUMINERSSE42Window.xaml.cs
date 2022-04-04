using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading;
using BenchMaestro.Windows;
using System.Windows.Interop;

namespace BenchMaestro
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    using Module1 = BenchModule1;
    using Module2 = BenchModule2_CpuMiner;
    using WindowSettings = Properties.SettingsCPUMINERSSE42;
    public partial class CPUMINERSSE42Window 
    {
        static string Benchname = "CPUMINERSSE42";

        string BenchBinary = @".\Benchmarks\cpuminer-opt-3.19.7\cpuminer-aes-sse42.exe";
        string BenchArchive = @".\Benchmarks\cpuminer-opt-3.19.7\cpuminer-aes-sse42.7z";
        string BenchPath = @".\Benchmarks\cpuminer-opt-3.19.7\";
        string BenchArgs = $" --benchmark --no-color --time-limit=###runtime### --threads=###threads### --cpu-affinity ###affinity### --algo=scrypt:512 --no-redirect --no-extranonce --no-stratum --no-gbt --no-getwork --no-longpoll --stratum-keepalive";
        bool BenchArchived = true;
        bool EndCheckLowLoad = false;

        List<BenchScore> scoreRun = new List<BenchScore>();

        const int WM_SIZING = 0x214;
        const int WM_EXITSIZEMOVE = 0x232;
        private static bool WindowWasResized = false;

        // Using a DependencyProperty as the backing store for MyTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(CPUMINERSSE42Window), new UIPropertyMetadata($"BenchMaestro-{App.version}-{Benchname}"));
        public CPUMINERSSE42Window()
        {
            InitializeComponent();
            Loaded += Window_Loaded;

            DataContext = new
            {
                //runs = VM,
                settings = BenchMaestro.Properties.Settings.Default,
                systemInfo = App.systemInfo,
                benchsettings = WindowSettings.Default,
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
        public string WinTitle
        {
            get { return (string)GetValue(WinTitleProperty); }
            set { SetValue(WinTitleProperty, value); }
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Trace.WriteLine($"Window Initialized {WindowSettings.Default.Initialized}");
            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, Width);
            SetValue(MinHeightProperty, Height);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //ClearValue(SizeToContentProperty);

            if (WindowSettings.Default.Initialized)
            {
                Trace.WriteLine($"Restoring Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");

                WindowState = WindowState.Normal;
                Top = WindowSettings.Default.Top;
                Left = WindowSettings.Default.Left;
                Height = WindowSettings.Default.Height;
                Width = WindowSettings.Default.Width;
                if (WindowSettings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                CenterWindowOnScreen();
                WindowSettings.Default.Initialized = true;
                SaveWinPos();

            }

        }
        private void UpdateConfigTag(Object sender, DataTransferEventArgs args)
        {
            Trace.WriteLine($"UpdateConfigTag to {ConfigTag.Text.Trim()}");
            WinTitle = $"BenchMaestro-{App.version}-{Benchname}-{ConfigTag.Text.Trim()}";
        }


        private void Window_SizeChanged(object sender, EventArgs e)
        {

            if (!WindowSettings.Default.Initialized) SizeToContent = SizeToContent.WidthAndHeight;
            WindowSettings.Default.Initialized = true;
            WindowSettings.Default.Save();
            Trace.WriteLine($"Saving Initialized ");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowSettings.Default.Initialized)
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
                WindowSettings.Default.Top = RestoreBounds.Top;
                WindowSettings.Default.Left = RestoreBounds.Left;
                WindowSettings.Default.Height = RestoreBounds.Height;
                WindowSettings.Default.Width = RestoreBounds.Width;
                WindowSettings.Default.Maximized = true;
            }
            else
            {
                WindowSettings.Default.Top = Top;
                WindowSettings.Default.Left = Left;
                WindowSettings.Default.Height = Height;
                WindowSettings.Default.Width = Width;
                WindowSettings.Default.Maximized = false;
            }
            WindowSettings.Default.Save();
            Trace.WriteLine($"Saving Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
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
                        foreach (BenchScore bscore in scoreRun)
                        {
                            ScrollViewer sv = bscore.DetailsScroller;
                            Expander exp = bscore.DetailsExpander;
                            bool expanded = exp.IsExpanded;
                            double _scrollh = Height - sv.TranslatePoint(new Point(0, 0), null).Y;
                            double _scrollmh = MaxHeight - sv.TranslatePoint(new Point(0, 0), null).Y;
                            double _scrollth = sv.TranslatePoint(new Point(0, 0), null).Y;
                            double _tsh = sv.ScrollableHeight + sv.ExtentHeight;
                            //Trace.WriteLine($"_scroller tTP={App.CurrentRun.DetailsBox.TranslatePoint(new Point(0, this.Height), this)} TP={App.CurrentRun.DetailsScroller.TranslatePoint(new Point(0, 0), null)} H={App.CurrentRun.DetailsScroller.ActualHeight} WH={this.Height}");
                            if (expanded)
                            {
                                if (_scrollh >= _tsh - 8)
                                {
                                    sv.Height = _tsh - 8;
                                }
                                else if (_scrollh >= _scrollmh - 8)
                                {
                                    sv.Height = _scrollmh - 8;
                                }
                                else
                                {
                                    //Trace.WriteLine($"exitszm_scroller_p aH={sv.ActualHeight} eH={sv.ExtentHeight} scH={sv.ViewportHeight}");
                                    sv.Height = _scrollh - 8;
                                }
                            }
                            //Trace.WriteLine($"exitszm_scroller aH={sv.ActualHeight} sH={_scrollh} tH={_scrollth} isexpanded={expanded}");
                            //Trace.WriteLine($"exitszm_scroller tsh={_tsh} eH={sv.ExtentHeight} scH={sv.ViewportHeight} isexpanded={expanded}");
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
                    _score = $"{Math.Round((decimal)App.CurrentRun.Score, 2)}";
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
                Module2.UpdateProgress2(value, ProgressBar);
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
                Module2.UpdateRunSettings2(RunSettingsLabel, RunSettingsBlock);
            }));
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
                        bool _extract = App.ExtractBench(BenchArchive, BenchPath, BenchBinary);

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
                    //SizeToContent = SizeToContent.WidthAndHeight;

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
                if (App.CurrentRun != null) App.CurrentRun.RunLog += $"\n\rMAIN STATUS MESSAGE: {_msgstatus}";


            }));
        }

        public void StartBench()
        {

            Trace.WriteLine("BENCH START");
            int sync = Interlocked.CompareExchange(ref App.InterlockBench, 1, 0);
            if (sync == 0)
            {
                Trace.WriteLine("BENCH START2");
                Module2.RunBench(Benchname, scoreRun, BenchArgs, BenchBinary, BenchArchived, EndCheckLowLoad,
                    UpdateProgress,
                    UpdateScore,
                    UpdateMainStatus,
                    UpdateFinished,
                    SetStart,
                    SetEnabled,
                    UpdateRunStop,
                    UpdateMonitoring,
                    UpdateRunSettings,
                    UpdateStarted,
                    UpdateRunStart
                    );
                App.InterlockBench = 0;
            }

        }


    }

}
