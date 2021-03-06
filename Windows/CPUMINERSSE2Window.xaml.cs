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
    using WindowSettings = Properties.SettingsCPUMINERSSE2;
    public partial class CPUMINERSSE2Window 
    {
        static string Benchname = "CPUMINERSSE2";

        string BenchBinary = @".\Benchmarks\cpuminer-opt-3.19.7\cpuminer-sse2.exe";
        string BenchArchive = @".\Benchmarks\cpuminer-opt-3.19.7\cpuminer-sse2.7z";
        string BenchPath = @".\Benchmarks\cpuminer-opt-3.19.7\";
        string BenchArgs = $" --benchmark --hash-meter --no-color --time-limit=###runtime### --threads=###threads### --cpu-affinity ###affinity### --algo=scrypt:512 --no-redirect --no-extranonce --no-stratum --no-gbt --no-getwork --no-longpoll --stratum-keepalive";
        string BenchScoreUnit = "H/s";
        bool BenchArchived = true;
        bool EndCheckLowLoad = false;

        List<BenchScore> scoreRun = new List<BenchScore>();

        const int WM_SIZING = 0x214;
        const int WM_EXITSIZEMOVE = 0x232;
        private static bool WindowWasResized = false;
        private static bool WindowIsInit = false;

        // Using a DependencyProperty as the backing store for MyTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(CPUMINERSSE2Window), new UIPropertyMetadata($"BenchMaestro-{App.version}-{Benchname}"));
        public CPUMINERSSE2Window()
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
                Top = WindowSettings.Default.Top < SystemParameters.WorkArea.Top ? SystemParameters.WorkArea.Top : WindowSettings.Default.Top;
                Left = WindowSettings.Default.Left < SystemParameters.WorkArea.Left ? SystemParameters.WorkArea.Left : WindowSettings.Default.Left;
                Height = WindowSettings.Default.Height > SystemParameters.WorkArea.Height ? SystemParameters.WorkArea.Height : WindowSettings.Default.Height;
                Width = WindowSettings.Default.Width > SystemParameters.WorkArea.Width ? SystemParameters.WorkArea.Width : WindowSettings.Default.Width;
                if (WindowSettings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                MainWindow.CenterWindowTopScreen(this);
                WindowSettings.Default.Initialized = true;
                SaveWinPos();
            }
            WindowIsInit = true;

        }
        private void UpdateConfigTag(Object sender, DataTransferEventArgs args)
        {
            Trace.WriteLine($"UpdateConfigTag to {ConfigTag.Text.Trim()}");
            WinTitle = $"BenchMaestro-{App.version}-{Benchname}-{ConfigTag.Text.Trim()}";
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            if (!App.bscreenshotdetails)
            {
                Trace.WriteLine($"SizeChanged");
                if (WindowSettings.Default.Initialized && WindowIsInit) SaveWinPos();
                UpdateLayout();
            }
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
            if (!App.bscreenshot)
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
                            double _tsh = sv.ExtentHeight;
                            //Trace.WriteLine($"_scroller tTP={App.CurrentRun.DetailsBox.TranslatePoint(new Point(0, this.Height), this)} TP={App.CurrentRun.DetailsScroller.TranslatePoint(new Point(0, 0), null)} H={App.CurrentRun.DetailsScroller.ActualHeight} WH={this.Height}");
                            if (expanded)
                            {
                                double svHeight = 0;
                                if (_scrollh >= _tsh - 8)
                                {
                                    svHeight = _tsh - 8;
                                }
                                else if (_scrollh >= _scrollmh - 8)
                                {
                                    svHeight = _scrollmh - 8;
                                }
                                else
                                {
                                    //Trace.WriteLine($"exitszm_scroller_p aH={sv.ActualHeight} eH={sv.ExtentHeight} scH={sv.ViewportHeight}");
                                    svHeight = _scrollh - 8;
                                }
                                sv.Height = svHeight > 0 ? svHeight : 0;
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
            Module1.ButtonScreenshot_Click2(sender, e, this, WinTitle);
        }

        public void SetStart()
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    BtnStartLabel.Text = "Start";
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"SetStart Exception: {e}");
            }
        }
        public void SetEnabled()
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    BtnStartLabel.IsEnabled = true;
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"SetEnabled Exception: {e}");
            }
        }
        public void SetLiveBindings(BenchScore _scoreRun, bool enabled)
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.SetLiveBindings2(_scoreRun, enabled);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"SetLiveBindings Exception: {e}");
            }
        }
        public void UpdateStarted()
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.UpdateStarted2();
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateStarted Exception: {e}");
            }
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
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.UpdateFinished2(_exitstatus);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateFinished Exception: {e}");
            }
        }
        public void UpdateScore(string _score = "")
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.UpdateScore2(_score);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateScore Exception: {e}");
            }
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

                    if (App.CurrentRun != null) App.CurrentRun.FinishString = "Aborted by user";
                    
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

                    Module1.ScoresLayout(ScoreList, scoreRun, threads, Benchname, ConfigTag.Text.Trim(), BenchScoreUnit);

                    Double minh = 100;
                    SetValue(MinWidthProperty, minh);
                    SetValue(MinHeightProperty, minh);
                    SizeToContent = SizeToContent.WidthAndHeight;
                    UpdateLayout();
                    SetValue(MinWidthProperty, ActualWidth);
                    SetValue(MinHeightProperty, ActualHeight);

                    App.thrBench = new Thread(StartBench);
                    App.thridBench = App.thrBench.ManagedThreadId;
                    App.thrBench.Start();
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
                    Module1.UpdateMonitoring2(ScoreList, this);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateMonitoring Exception: {e}");
            }
        }


        public void UpdateMainStatus(string _msgstatus)
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    MainStatus.Text = $"{_msgstatus}";
                    if (App.CurrentRun != null) App.CurrentRun.RunLog += $"\n\rMAIN STATUS MESSAGE: {_msgstatus}";


                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateMainStatus Exception: {e}");
            }
        }
        public void UpdateHeadersWidth()
        {
            try
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Module1.UpdateHeadersWidth2(this, scoreRun);
                }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"UpdateMonitoring Exception: {e}");
            }
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
                    UpdateRunStart,
                    SetLiveBindings,
                    UpdateHeadersWidth
                    );
                App.InterlockBench = 0;
            }

        }


    }

}
