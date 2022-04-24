using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BenchMaestro.Windows
{
    /// <summary>
    /// Interaction logic for PleaseWait.xaml
    /// </summary>
    public partial class PleaseWait : IDisposable
    {
        private double OwnerHeigth;
        public PleaseWait(double ownerHeigth)
        {
            OwnerHeigth = ownerHeigth;
            InitializeComponent();
        }


        public void Dispose()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = (OwnerHeigth / 2) - 34;
            Dispatcher.BeginInvoke(new Action(() => {
                long sync = 0;
                Trace.WriteLine($"PleaseWait Waitfor Rendered");
                while (sync == 0)
                {
                    sync = Interlocked.Read(ref App.bscreenshotrendered);
                }
                Trace.WriteLine($"PleaseWait Waitforsyncdone");
                Close();
            }), DispatcherPriority.ContextIdle);

        }
    }
}