using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;
using BenchMaestro.Windows;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Web;
using System.Reflection;
using AutoUpdaterDotNET;
using Newtonsoft.Json;


namespace BenchMaestro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using WindowSettings = Properties.Settings;
    public partial class MainWindow
    {
        static string Benchname = "MainMenu";
        static bool InitUI = true;
        static bool WinLoaded = false;
        
        XMRSTAKRXWindow XMRSTAKRXWin;
        CPUMINERSSE2Window CPUMINERSSE2Win;
        CPUMINERSSE42Window CPUMINERSSE42Win;
        CPUMINERAVXWindow CPUMINERAVXWin;
        CPUMINERAVX2Window CPUMINERAVX2Win;
        CPUMINERAVX2SHAWindow CPUMINERAVX2SHAWin;
        CPUMINERAVX2SHAVAESWindow CPUMINERAVX2SHAVAESWin;
        CPUMINERAVX512Window CPUMINERAVX512Win;
        CPUMINERAVX512SHAVAESWindow CPUMINERAVX512SHAVAESWin;
        public string WinTitle
        {
            get { return (string)GetValue(WinTitleProperty); }
            set { SetValue(WinTitleProperty, value); }
        }

        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(MainWindow), new UIPropertyMetadata($"BenchMaestro-{App.version}-{Benchname}"));
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Trace.WriteLine($"SourceInit Window Initialized {WindowSettings.Default.Initialized}");
            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, Width);
            SetValue(MinHeightProperty, Height);
            ClearValue(SizeToContentProperty);
            App.systemInfo.WinMaxSize = System.Windows.SystemParameters.WorkArea.Height;


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
                SizeToContent = SizeToContent.WidthAndHeight;
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                SaveWinPos();
                Trace.WriteLine($"SizeChanged Set Center and Save");
                WindowSettings.Default.Initialized = true;
                SaveWinPos();
            }
            WinLoaded = true;
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            Trace.WriteLine($"SizeChanged Window Initialized {WindowSettings.Default.Initialized}");
            if (WindowSettings.Default.Initialized && WinLoaded)
            {
                SaveWinPos();
            }
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsActive && WindowSettings.Default.Initialized)
            {
                SaveWinPos();
                Trace.WriteLine($"Saving Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
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
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitMainUI();
            string LicensePath = @".\LICENSE";
            if (File.Exists(LicensePath))
            {
                boxLicense.Text = File.ReadAllText(LicensePath);
            }
            if (App.systemInfo.ZenCOb)
            {
                COCounts.Visibility = Visibility.Visible;
                COCountsLabel.Visibility = Visibility.Visible;
            }
            if (!Avx.IsSupported)
            {
                BtnBenchCPUMINERAVX.Visibility = Visibility.Collapsed;
                BtnBenchCPUMINERAVX2SHA.Visibility = Visibility.Collapsed;
                BtnBenchCPUMINERAVX2SHAVAES.Visibility = Visibility.Collapsed;
            }
            if (!Avx2.IsSupported)
            {
                BtnBenchCPUMINERAVX2.Visibility = Visibility.Collapsed;
                BtnBenchCPUMINERAVX2SHA.Visibility = Visibility.Collapsed;
                BtnBenchCPUMINERAVX2SHAVAES.Visibility = Visibility.Collapsed;
            }
            if (!Sse42.IsSupported)
            {
                BtnBenchCPUMINERSSE42.Visibility = Visibility.Collapsed;
            }

            if (App.ZenPTSubject.Length > 0)
            {
                BtnNewZenPT.Visibility = Visibility.Visible;
            }

            AutoUpdater.ReportErrors = false;
            AutoUpdater.InstalledVersion = new Version(App._versionInfo);
            AutoUpdater.DownloadPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Synchronous = false;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.Start("https://raw.githubusercontent.com/mann1x/BenchMaestro/master/BenchMaestro/AutoUpdaterBenchMaestro1.json");
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

        private void RadioMode(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.IsChecked == true)
            {
                if ((string)rb.Tag == "Custom")
                {
                    App.systemInfo.STMT = false;
                    WindowSettings.Default.BtnSTMT = false;
                    if (App.GetThreads().Length < 1)
                    {
                        Trace.WriteLine($"RB BACK TO STMT");
                        RadioSTMT.IsChecked = true;
                    }
                }
                else
                {
                    App.systemInfo.STMT = true;
                    WindowSettings.Default.BtnSTMT = true;
                }
                WindowSettings.Default.Save();
                Trace.WriteLine($"RB CHECKED {rb.Name} {rb.Tag} SETTINGS BtnSTMT {WindowSettings.Default.BtnSTMT}");
            }

        }
        private void CheckThreads(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = sender as ToggleButton;

            string Tag = (string)cb.Tag;

            if (cb.IsChecked == true)
            {
                if (!WindowSettings.Default.Threads.Contains(Tag)) WindowSettings.Default.Threads.Add(Tag);
                Trace.WriteLine($"CB CHECKED {cb.Tag}");
                if (!InitUI) { 
                    RadioCustom.IsChecked = true;
                    RadioSTMT.IsChecked = false;
                    App.systemInfo.STMT = false;
                    WindowSettings.Default.BtnSTMT = false;
                }
            }
            else
            {
                if (WindowSettings.Default.Threads.Contains(Tag)) WindowSettings.Default.Threads.Remove(Tag);
                Trace.WriteLine($"CB UNCHECKED {cb.Tag}");
                RadioCustom.IsChecked = true;
                RadioSTMT.IsChecked = false;
                App.systemInfo.STMT = false;
                WindowSettings.Default.BtnSTMT = false;
            }
            WindowSettings.Default.Save();

        }
        private void CheckCustomCPPC(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            Trace.WriteLine($"CUSTOMCPPC {WindowSettings.Default.CustomCPPC}");

            if (cb.IsChecked == true)
            {
                WindowSettings.Default.cbCustomCPPC = true;
                Trace.WriteLine($"CB CHECKED {cb.Tag}");
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
                CPPCActiveLabel.Text = App.GetCustomLabel();
                Trace.WriteLine($"LABEL {App.GetCustomLabel()} {App.systemInfo.CPPCActiveLabel}");
            }
            else
            {
                WindowSettings.Default.cbCustomCPPC = false;
                Trace.WriteLine($"CB UNCHECKED {cb.Tag}");
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCOrder;
                App.systemInfo.CPPCActiveLabel = App.systemInfo.CPPCLabel;
                CPPCActiveLabel.Text = App.systemInfo.CPPCLabel;
                Trace.WriteLine($"LABEL {App.systemInfo.CPPCLabel} {App.systemInfo.CPPCActiveLabel}");
            }
            WindowSettings.Default.Save();

        }
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
        private void CustomCPPC_Save(object sender, RoutedEventArgs e)
        {

            int i = 0;
            foreach (ListBoxItem _core in CustomCPPC.Items)
            {
                App.systemInfo.CPPCCustomOrder[i] = Convert.ToInt32(_core.Tag);
                i++;
                Trace.WriteLine($"1 = {_core.Tag}");
            }

            WindowSettings.Default.CustomCPPC = App.GetCustomLabel();
            WindowSettings.Default.Save();

            if (cbCustomCPPC.IsChecked == true)
            {
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
                CPPCActiveLabel.Text = App.GetCustomLabel();
                Trace.WriteLine($"LABEL {App.GetCustomLabel()} {App.systemInfo.CPPCActiveLabel}");
            }

            Trace.WriteLine($"CUSTOMCPPC {WindowSettings.Default.CustomCPPC}");
            Trace.WriteLine($"CUSTOMLABEL {App.GetCustomLabel()}");
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
        private void ButtonReset(object sender, RoutedEventArgs e)
        {
            WindowSettings.Default.Reset();
            
            App.SettingsInit();

            InitMainUI();

        }

        private void InitMainUI()
        {

            InitUI = true;

            Trace.WriteLine($"STMT={WindowSettings.Default.BtnSTMT}");
            if (WindowSettings.Default.BtnSTMT)
            {
                RadioSTMT.IsChecked = true;
                RadioCustom.IsChecked = false;
            }
            else
            {
                RadioSTMT.IsChecked = false;
                RadioCustom.IsChecked = true;
            }

            foreach (string thr in WindowSettings.Default.Threads)
            {
                Trace.WriteLine($" RESTORING CB {thr}");
                IEnumerable<CheckBox> elements = FindVisualChildren<CheckBox>(this).Where(x => x.Tag != null && x.Tag.ToString() == thr.ToString());
                foreach (CheckBox cb in elements)
                {
                    cb.IsChecked = true;
                }
            }

            if (App.systemInfo.CPULogicalProcessors <= 32) tMax.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 32) t32.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 24) t24.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 16) t16.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 12) t12.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 8) t8.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 6) t6.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 4) t4.Visibility = Visibility.Collapsed;
            if (App.systemInfo.CPULogicalProcessors < 2) t2.Visibility = Visibility.Collapsed;

            Thickness coreborder = new Thickness(1, 1, 1, 1);
            Thickness coremargin = new Thickness(3, 3, 3, 3);

            CustomCPPC.Items.Clear();


            foreach (int _core in App.systemInfo.CPPCCustomOrder)
            {
                string _strcore = _core.ToString();
                CustomCPPC.Items.Add(new ListBoxItem { Tag = _strcore, Content = _strcore, BorderBrush = Brushes.Lavender, BorderThickness = coreborder, Margin = coremargin });
            }


            if (WindowSettings.Default.cbCustomCPPC)
            {
                cbCustomCPPC.IsChecked = true;
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
            }
            else
            {
                cbCustomCPPC.IsChecked = false;
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCOrder;
                App.systemInfo.CPPCActiveLabel = App.systemInfo.CPPCLabel;
            }

            InitUI = false;
        }
        private void NewBenchXMRSTAKRX(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<XMRSTAKRXWindow>(""))
            {
                XMRSTAKRXWin = new XMRSTAKRXWindow();
                XMRSTAKRXWin.DataContext = this.DataContext;
            }
            XMRSTAKRXWin.Show();

        }
        private void NewBenchCPUMINERSSE2(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERSSE2Window>(""))
            {
                CPUMINERSSE2Win = new CPUMINERSSE2Window();
                CPUMINERSSE2Win.DataContext = this.DataContext;
            }
            CPUMINERSSE2Win.Show();

        }
        private void NewBenchCPUMINERSSE42(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERSSE42Window>(""))
            {
                CPUMINERSSE42Win = new CPUMINERSSE42Window();
                CPUMINERSSE42Win.DataContext = this.DataContext;
            }
            CPUMINERSSE42Win.Show();

        }
        private void NewBenchCPUMINERAVX(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVXWindow>(""))
            {
                CPUMINERAVXWin = new CPUMINERAVXWindow();
                CPUMINERAVXWin.DataContext = this.DataContext;
            }
            CPUMINERAVXWin.Show();

        }
        private void NewBenchCPUMINERAVX2(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVX2Window>(""))
            {
                CPUMINERAVX2Win = new CPUMINERAVX2Window();
                CPUMINERAVX2Win.DataContext = this.DataContext;
            }
            CPUMINERAVX2Win.Show();

        }
        private void NewBenchCPUMINERAVX2SHA(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVX2SHAWindow>(""))
            {
                CPUMINERAVX2SHAWin = new CPUMINERAVX2SHAWindow();
                CPUMINERAVX2SHAWin.DataContext = this.DataContext;
            }
            CPUMINERAVX2SHAWin.Show();

        }

        private void NewBenchCPUMINERAVX2SHAVAES(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVX2SHAVAESWindow>(""))
            {
                CPUMINERAVX2SHAVAESWin = new CPUMINERAVX2SHAVAESWindow();
                CPUMINERAVX2SHAVAESWin.DataContext = this.DataContext;
            }
            CPUMINERAVX2SHAVAESWin.Show();

        }
        private void NewBenchCPUMINERAVX512(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVX512Window>(""))
            {
                CPUMINERAVX512Win = new CPUMINERAVX512Window();
                CPUMINERAVX512Win.DataContext = this.DataContext;
            }
            CPUMINERAVX512Win.Show();

        }
        private void NewBenchCPUMINERAVX512SHAVAES(object sender, RoutedEventArgs e)
        {

            if (!IsWindowOpen<CPUMINERAVX512SHAVAESWindow>(""))
            {
                CPUMINERAVX512SHAVAESWin = new CPUMINERAVX512SHAVAESWindow();
                CPUMINERAVX512SHAVAESWin.DataContext = this.DataContext;
            }
            CPUMINERAVX512SHAVAESWin.Show();

        }

        private void BtnNewZenPT_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(App.ZenPTBody);
            string url = $"https://github.com/mann1x/BSManager/issues/new?title={HttpUtility.UrlEncode(App.ZenPTSubject)}&body={HttpUtility.UrlEncode("Please paste from Clipboard (Ctrl+V) the content of the PowerTable")})";
            try 
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        public static void CenterWindowTopScreen(Window _this) { 
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double windowWidth = _this.Width;
            _this.Left = (screenWidth / 2) - (windowWidth / 2);
            _this.Top = SystemParameters.WorkArea.Top;
        }
        private void btnRefreshInfo_Click(object sender, RoutedEventArgs e)
        {
            App.systemInfo.ZenRefreshStatic(true);
            App.systemInfo.ZenRefreshCO();
            App.systemInfo.RefreshLabels();
        }
    }
}
