using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                result = Image.FromHbitmap(ibitmap);
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

                        UIntPtr hbitmap = CreateCompatibleBitmap(hDC, bounds.Width, bounds.Height); //Create a bitmap handle, compatible with hDC
                        if (hbitmap != UIntPtr.Zero)
                        {
                            UIntPtr hOld = SelectObject(hMemDC, hbitmap); //Select hbitmap into hMemDC
                            UIntPtr wparamtrue = new UIntPtr(1);
                            UIntPtr wparamfalse = new UIntPtr(0);
                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            Window owner = App.screenshotwin;
                            if (owner != null && App.bscreenshotdetails)
                            {
                                App.bscreenshotrendered = false;
                                owner.Dispatcher.Invoke(new Action(() => {
                                    App.screenshotwin.UpdateLayout();
                                }), DispatcherPriority.ContextIdle);
                                DateTime _start = DateTime.Now;
                                while (!App.bscreenshotrendered)
                                {
                                    TimeSpan _delta = DateTime.Now - _start;
                                    if (_delta.TotalSeconds > 5) App.bscreenshotrendered = true;
                                }
                                App.bscreenshotrendered = false;
                                owner.Dispatcher.Invoke(new Action(() => {
                                    DispatcherFrame frame = new DispatcherFrame();
                                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
                                    {
                                        frame.Continue = false;
                                        return null;
                                    }), null);
                                    Dispatcher.PushFrame(frame);
                                }), DispatcherPriority.ContextIdle);
                                _start = DateTime.Now;
                                while (!App.bscreenshotrendered)
                                {
                                    TimeSpan _delta = DateTime.Now - _start;
                                    if (_delta.TotalSeconds > 5) App.bscreenshotrendered = true;
                                }
                                owner.Dispatcher.Hooks.OperationCompleted -= App.SetSSRendered;
                            }
                            long style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            PrintWindow(hWnd, hMemDC, 0);
                            PrintWindow(hWnd, hDC, 0);

                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            PrintWindow(hWnd, hMemDC, 0);
                            PrintWindow(hWnd, hDC, 0);

                            SendMessage(hWnd, WM_SETREDRAW, wparamfalse, IntPtr.Zero);
                            DefWindowProc(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_PRINT, hMemDC, (IntPtr)(PRF_CLIENT | PRF_NONCLIENT));
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);
                            RedrawWindow(hWnd, UIntPtr.Zero, UIntPtr.Zero, 0);
                            style = GetWindowLong(hWnd, GWL_STYLE);
                            SetWindowLong(hWnd, GWL_STYLE, style & ~WS_CAPTION);
                            PrintWindow(hWnd, hMemDC, 0);
                            PrintWindow(hWnd, hDC, 0);

                            var success = BitBlt(hMemDC, 0, 0, bounds.Width, bounds.Height, hDC, 0, 0,
                                SRCCOPY);
                            IntPtr ihbitmap = unchecked((IntPtr)(long)(ulong)hbitmap);
                            bitmap = Image.FromHbitmap(ihbitmap); //Create a managed Bitmap out of hbitmap
                            SendMessage(hWnd, WM_SETREDRAW, wparamtrue, IntPtr.Zero);

                            SelectObject(hMemDC, hOld); //Select hOld into hMemDC (the previously replaced object), to leave hMemDC as found
                            DeleteObject(hbitmap); //Free hbitmap
                        }
                        DeleteDC(hMemDC); //Free hdcMem
                    }
                    ReleaseDC(hWnd, hDC); //Free hDC
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