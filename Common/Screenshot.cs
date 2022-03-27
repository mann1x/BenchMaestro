﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BenchMaestro
{
    public class Screenshot : IDisposable
    {
        //Credit to Ivan's ZenTimings
        
        // GDI stuff for window screenshot without shadows
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute,
            int cbAttribute);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

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

        private Bitmap CaptureRegion(Rectangle region)
        {
            Bitmap result;

            IntPtr desktophWnd = GetDesktopWindow();
            IntPtr desktopDc = GetWindowDC(desktophWnd);
            IntPtr memoryDc = CreateCompatibleDC(desktopDc);
            IntPtr bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            IntPtr oldBitmap = SelectObject(memoryDc, bitmap);

            var success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top,
                SRCCOPY | CAPTUREBLT);

            try
            {
                if (!success) throw new Win32Exception();

                result = Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }

            return result;
        }

        private Bitmap CaptureWindow(IntPtr hWnd)
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

            return CaptureRegion(Rectangle.FromLTRB(region.left, region.top, region.right, region.bottom));
        }

        public Bitmap CaptureActiveWindow()
        {
            return CaptureWindow(GetForegroundWindow());
        }

        public Bitmap CaptureDekstop()
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