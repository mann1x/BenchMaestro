using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace BenchMaestro.Windows
{
    /// <summary>
    /// Interaction logic for SaveWindow.xaml
    /// </summary>
    public partial class SaveWindow : IDisposable
    {
        public static Bitmap screenshot;

        public SaveWindow(Bitmap bitmap)
        {
            screenshot = bitmap;
            DataContext = new
            {
                //runs = VM,
                settings = BenchMaestro.Properties.Settings.Default,
                systemInfo = App.systemInfo
            };
            InitializeComponent();
        }

        private void SaveToFile(string filename = "")
        {
            if (filename.Length < 1) filename = App.ss_filename;
            screenshot.Save(filename);
            screenshot.Dispose();
            Close();
        }
        private string PrepFolder()
        {
            string _folder = $"ScreenShots\\{Properties.Settings.Default.ConfigTag.ToString().Trim()}";
            System.IO.Directory.CreateDirectory("ScreenShots");
            System.IO.Directory.CreateDirectory($"{_folder}");
            return _folder;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string _folder = PrepFolder();
            string filename = _folder + "\\" + App.ss_filename;
            SaveToFile(filename);
        }

        private void ButtonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            PrepFolder();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "png files (*.png)|*.png|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "png",
                FileName = App.ss_filename,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == true) SaveToFile(saveFileDialog.FileName);
        }
        private void ButtonRename_Click(object sender, RoutedEventArgs e)
        {
            string initialDir = System.AppContext.BaseDirectory + PrepFolder() + "\\";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "png files (*.png)|*.png|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = "png",
                FileName = App.ss_filename,
                InitialDirectory = initialDir,
                RestoreDirectory = false
            };

            if (saveFileDialog.ShowDialog() == true) SaveToFile(saveFileDialog.FileName);
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(screenshot);
            statusStrip1.Visibility = Visibility.Visible;
            stripCliboard.Visibility = Visibility.Visible;
            stripScreenshotsFolder.Visibility = Visibility.Collapsed;
        }
        private void ButtonScreenshotsFolder(object sender, RoutedEventArgs e)
        {
            string _folder = $".\\ScreenShots\\{Properties.Settings.Default.ConfigTag.ToString().Trim()}";
            if (!Directory.Exists(_folder)) _folder = @".\Screenshots";
            Process.Start("explorer.exe", _folder);
            statusStrip1.Visibility = Visibility.Visible;
            stripCliboard.Visibility = Visibility.Collapsed;
            stripScreenshotsFolder.Visibility = Visibility.Visible;
        }

        public void Dispose()
        {
            //((IDisposable) screenshot).Dispose();
        }
    }
}