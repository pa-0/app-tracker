﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class WindowHelper
    {
        public static string GetActiveWindowName()
        {
            IntPtr foregroundWindow = WinAPI.GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(WinAPI.GetWindowTextLength(foregroundWindow) + 1);
            if (WinAPI.GetWindowText(foregroundWindow, windowTitle, windowTitle.Capacity) > 0)
            {
                if (string.IsNullOrEmpty(windowTitle.ToString().Trim())) return "No Title";
                return windowTitle.ToString();
            }
            return "No Title";
        }

        public static IntPtr GetActiveWindowHandle()
        {
            return WinAPI.GetForegroundWindow();
        }

        public static IAppInfo GetActiveWindowAppInfo()
        {
            var handle = GetActiveWindowHandle();
            if (handle == IntPtr.Zero)
                return null;

            var process = GetProcessFromHandle(handle);
            return AppInfo.GetAppInfo(process);
        }

        private static Process GetProcessFromHandle(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                if (processID != 0)
                    return System.Diagnostics.Process.GetProcessById(Convert.ToInt32(processID));

            }
            return null;
        }
    }
}
