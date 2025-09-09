using System;
using System.Collections.Generic;
using System.Windows;

namespace ScreenDimmer
{
    public static class ScreenService
    {
        public static List<Rect> GetAllScreens()
        {
            var screens = new List<Rect>();
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
                {
                    var rect = new Rect(
                        lprcMonitor.Left,
                        lprcMonitor.Top,
                        lprcMonitor.Right - lprcMonitor.Left,
                        lprcMonitor.Bottom - lprcMonitor.Top);
                    screens.Add(rect);
                    return true;
                }, IntPtr.Zero);
            return screens;
        }
    }
}
