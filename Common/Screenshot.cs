using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BenchMaestro
{
    public class Screenshot : IDisposable
    {
        //Credit to Ivan's ZenTimings
        
        // GDI stuff for window screenshot without shadows
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(UIntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight,
            UIntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern UIntPtr CreateCompatibleBitmap(UIntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern UIntPtr CreateCompatibleDC(UIntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern UIntPtr DeleteDC(UIntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern UIntPtr DeleteObject(UIntPtr hObject);

        [DllImport("user32.dll")]
        private static extern UIntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern UIntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(UIntPtr hWnd, UIntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern UIntPtr SelectObject(UIntPtr hdc, UIntPtr hObject);       

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(UIntPtr hwnd, out RECT lpRect);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(UIntPtr hwnd, int dwAttribute, out RECT pvAttribute,
            int cbAttribute);

        [DllImport("user32.dll")]
        private static extern UIntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern UIntPtr GetWindowDC(UIntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int DefWindowProc(UIntPtr hwnd, uint Message, UIntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int SendMessage(UIntPtr hwnd, uint Message, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int PrintWindow(UIntPtr hwnd, UIntPtr wParam, int nFlags);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(UIntPtr hwnd, int Style, long sFlags);

        [DllImport("user32.dll")]
        private static extern long GetWindowLong(UIntPtr hwnd, int Index);
        
        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(UIntPtr hwnd, UIntPtr lprcupdate, UIntPtr hrgnUpdate, int flags);
        //private static extern bool RedrawWindow(UIntPtr hwnd, RECT lprcupdate, UIntPtr hrgnUpdate, int flags);
        [DllImport("user32.dll")]
        private static extern long UpdateWindow(UIntPtr hwnd);



        [StructLayout(LayoutKind.Sequential)]
        private readonly struct RECT
        {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;
        }

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private const int SRCCOPY = 0x00CC0020;
        private const int CAPTUREBLT = 0x40000000;
        private bool disposedValue;
        private const Int32 PW_CLIENTONLY = 0x1;
        private const UInt32 WM_PAINT = 0x000F;
        private const UInt32 WM_ERASEBKGND = 0x0014;
        private const UInt32 WM_PRINTCLIENT = 0x318;
        private const UInt32 WM_PRINT = 0x317, PRF_CLIENT = 4, PRF_CHILDREN = 0x10, PRF_OWNED = 0x20, PRF_NONCLIENT = 2;
        private const int GWL_STYLE = -16;              //hex constant for style changing
        private const int WS_BORDER = 0x00800000;       //window with border
        private const int WS_CAPTION = 0x00C00000;      //window with a title bar
        private const int WS_SYSMENU = 0x00080000;      //window with no borders etc.
        private const int WS_MINIMIZEBOX = 0x00020000;  //window with minimizebox
        private const int WM_SETREDRAW = 11;
        private const int WM_ACTIVATE = 0x0006;

        private Bitmap CaptureRegion(Rectangle region, UIntPtr hWnd)
        {
            Bitmap result;

            UIntPtr desktopDc = GetWindowDC(hWnd);
            UIntPtr memoryDc = CreateCompatibleDC(desktopDc);
            UIntPtr bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            UIntPtr oldBitmap = SelectObject(memoryDc, bitmap);

            var success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, 0, 0,
            SRCCOPY);

            try
            {
                if (!success) throw new Win32Exception();

                IntPtr ibitmap = unchecked((IntPtr)(long)(ulong)bitmap);
                result = System.Drawing.Image.FromHbitmap(ibitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(hWnd, desktopDc);
            }

            return result;
        }

        /// <summary>
        /// Captures a HWND, using a WM_PRINT windows message to draw into a memory DC.
        /// </summary>
        /// <param name="hWnd">The HWND.</param>
        /// <returns>Bitmap of the captured visual window, if the operation succeeds. Null, if the operation fails.</returns>
        public static Bitmap CaptureWindow(UIntPtr hWnd)
        {
            Bitmap bitmap = null;
            try
            {
                UIntPtr wparamtrue = new UIntPtr(1);
                UIntPtr wparamfalse = new UIntPtr(0);

                UIntPtr hDC = GetWindowDC(hWnd); //Get the device context of hWnd
                if (hDC != UIntPtr.Zero)
                {
                    UIntPtr hMemDC = CreateCompatibleDC(hDC); //Create a memory device context, compatible with hDC
                    if (hMemDC != UIntPtr.Zero)
                    {
                        RECT region;

                        if (Environment.OSVersion.Version.Major < 6)
                        {
                            GetWindowRect(hWnd, out region);
                        }
                        else
                        {
                            if (DwmGetWindowAttribute(hWnd, DWMWA_EXTENDED_FRAME_BOUNDS, out region,
                                Marshal.SizeOf(typeof(RECT))) != 0) GetWindowRect(hWnd, out region);
                        }
                        Rectangle bounds;
                        bounds = new Rectangle(region.left, region.top, region.right - region.left, region.bottom - region.top);
                        Trace.WriteLine($"bounds w:{bounds.Width} h:{bounds.Height}");
                        int _bwidth = bounds.Width;
                        int _bheight = bounds.Height;
                        Window owner = App.screenshotwin;
                        if (owner != null && App.bscreenshotdetails)
                        {
                            _bwidth += 256;
                            _bheight += 256;
                        }

                        UIntPtr hbitmap = CreateCompatibleBitmap(hDC, _bwidth, _bheight); //Create a bitmap handle, compatible with hDC
                        if (hbitmap != UIntPtr.Zero)
                        {
                            UIntPtr hOld = SelectObject(hMemDC, hbitmap); //Select hbitmap into hMemDC
                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            long style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            SendMessage(hWnd, WM_PAINT, hMemDC, IntPtr.Zero);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            UpdateWindow(hWnd);
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            PrintWindow(hWnd, hMemDC, 0);

                            if (owner != null && App.bscreenshotdetails)
                            {
                                Trace.WriteLine($"Details sv updatelayout");
                                owner.Dispatcher.Invoke(new Action(() => {

                                    Trace.WriteLine($"owner.MinHeight {owner.MinHeight} App.screenshotminheigth {App.screenshotminheigth}");
                                    owner.MinHeight = App.screenshotminheigth;
                                    IEnumerable<ScrollViewer> elements = MainWindow.FindVisualChildren<ScrollViewer>(owner).Where(x => x.Tag != null && x.Tag.ToString().StartsWith("Details"));
                                    foreach (var sv in elements)
                                    {
                                        double svHeight = sv.ExtentHeight;
                                        sv.Height = svHeight;
                                        sv.InvalidateScrollInfo();
                                        sv.InvalidateVisual();
                                        sv.UpdateLayout();
                                        Trace.WriteLine($"sv {sv.Tag} h:{sv.ActualHeight} eh:{sv.ExtentHeight}");
                                    }
                                    owner.SizeToContent = SizeToContent.WidthAndHeight;
                                }), DispatcherPriority.Send);

                                Trace.WriteLine($"Details 1st updatelayout");

                                owner.Dispatcher.Invoke(new Action(() => {

                                    App.screenshotwin.InvalidateVisual();
                                    App.screenshotwin.UpdateLayout();
                                }), DispatcherPriority.Send);

                                DispatcherOperation operation = owner.Dispatcher.BeginInvoke(new Action(() => {
                                    App.screenshotwin.UpdateLayout();
                                }), DispatcherPriority.Background);

                                DispatcherOperationStatus opstatus = operation.Wait(TimeSpan.FromSeconds(5));

                                Trace.WriteLine($"UpdateLayout Operation status: {opstatus}");

                                if (opstatus != DispatcherOperationStatus.Completed) Trace.WriteLine($"Operation not completed: {opstatus}");

                                owner.Dispatcher.Hooks.DispatcherInactive += App.SetSSRendered;

                                Interlocked.CompareExchange(ref App.bscreenshotrendered, 0, 1);

                                DateTime _start = DateTime.Now;
                                long sync = 0;
                                Trace.WriteLine($"Waitfor Rendered");
                                while (sync == 0)
                                {
                                    TimeSpan _delta = DateTime.Now - _start;
                                    if (_delta.TotalSeconds > 5)
                                    {
                                        Trace.WriteLine($"Waitfor Rendered timeout");
                                        Interlocked.CompareExchange(ref App.bscreenshotrendered, 1, 0);
                                    }
                                    sync = Interlocked.Read(ref App.bscreenshotrendered);
                                }
                                Trace.WriteLine($"Waitforsyncdone");

                                owner.Dispatcher.Hooks.DispatcherInactive -= App.SetSSRendered;
                            }

                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            SendMessage(hWnd, WM_PAINT, hMemDC, IntPtr.Zero);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            UpdateWindow(hWnd);
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            PrintWindow(hWnd, hMemDC, 0);

                            if (owner != null && App.bscreenshotdetails)
                            {
                                Trace.WriteLine($"Details 2nd updatelayout");
                                DispatcherOperation operation = owner.Dispatcher.BeginInvoke(new Action(() => {
                                    App.screenshotwin.UpdateLayout();
                                }), DispatcherPriority.Send);

                                DispatcherOperationStatus opstatus = operation.Wait();
                            }

                            for (int i = 0; i < 10; ++i)
                            {
                                SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                                DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                                SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                                SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                                style = GetWindowLong(hWnd, GWL_STYLE);
                                SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                                SendMessage(hWnd, WM_PAINT, hMemDC, IntPtr.Zero);
                                RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                                UpdateWindow(hWnd);
                                PrintWindow(hWnd, hMemDC, 0);
                            }

                            if (owner != null && App.bscreenshotdetails)
                            {
                                Trace.WriteLine($"Details 2nd DC");
                                UIntPtr hbitmap2 = CreateCompatibleBitmap(hMemDC, (int)owner.ActualWidth, (int)owner.ActualHeight); //Create a bitmap handle, compatible with hDC
                                UIntPtr hMemDC2 = CreateCompatibleDC(hMemDC); //Create a memory device context, compatible with hDC
                                SelectObject(hMemDC2, hbitmap2); //Select hbitmap into hMemDC
                                var success = BitBlt(hMemDC2, 0, 0, (int)owner.ActualWidth, (int)owner.ActualHeight, hMemDC, 0, 0,
                                    SRCCOPY);
                                IntPtr ihbitmap2 = unchecked((IntPtr)(long)(ulong)hbitmap2);
                                bitmap = System.Drawing.Image.FromHbitmap(ihbitmap2); //Create a managed Bitmap out of hbitmap
                                DeleteObject(hbitmap2); //Free hbitmap
                                DeleteDC(hMemDC2); //Free hdcMem
                            }
                            else
                            {
                                IntPtr ihbitmap = unchecked((IntPtr)(long)(ulong)hbitmap);
                                bitmap = System.Drawing.Image.FromHbitmap(ihbitmap); //Create a managed Bitmap out of hbitmap
                            }

                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hDC, (IntPtr)PRF_CLIENT);
                            SendMessage(hWnd, WM_PRINT, hDC, (IntPtr)PRF_CLIENT);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            SendMessage(hWnd, WM_PAINT, hMemDC, IntPtr.Zero);
                            UpdateWindow(hWnd);
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            PrintWindow(hWnd, hDC, 0);

                            //SelectObject(hMemDC, hOld); //Select hOld into hMemDC (the previously replaced object), to leave hMemDC as found
                            DeleteObject(hbitmap); //Free hbitmap
                        }
                        DeleteDC(hMemDC); //Free hdcMem
                    }
                    ReleaseDC(hWnd, hDC); //Free hDC
                    SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                    UpdateWindow(hWnd);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception: {ex}");
            }
            return bitmap;
        }


        private Bitmap CaptureWindow2(UIntPtr hWnd)
        {
            RECT region;

            if (Environment.OSVersion.Version.Major < 6)
            {
                GetWindowRect(hWnd, out region);
            }
            else
            {
                if (DwmGetWindowAttribute(hWnd, DWMWA_EXTENDED_FRAME_BOUNDS, out region,
                    Marshal.SizeOf(typeof(RECT))) != 0) GetWindowRect(hWnd, out region);
            }
            Rectangle bounds;
            bounds = new Rectangle(region.left, region.top, region.right - region.left, region.bottom - region.top);

            var result = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(result))
                g.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);

            return result;
            //return CaptureRegion(Rectangle.FromLTRB(region.left, region.top, region.right, region.bottom), hWnd);
        }

        public Bitmap CaptureActiveWindow()
        {
            return CaptureWindow(GetForegroundWindow());
        }
        public UIntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }
        public Bitmap CaptureThisWindow(UIntPtr hWnd)
        {
            return CaptureWindow(hWnd);
        }

        public Bitmap CaptureDesktop()
        {
            return CaptureWindow(GetDesktopWindow());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}