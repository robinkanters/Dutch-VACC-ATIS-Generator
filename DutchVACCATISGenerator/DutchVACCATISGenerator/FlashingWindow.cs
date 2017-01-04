using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DutchVACCATISGenerator
{
    /// <summary>
    /// Flashing window class.
    /// </summary>
    public static class FlashingWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }
        
        /// <summary>
        /// Flashes a form on the task bar.
        /// </summary>
        /// <param name="form">Form - form to flash</param>
        /// <returns>Boolean - indicates if the window can be flashed</returns>
        public static bool FlashWindowEx(Form form)
        {
            var hWnd = form.Handle;
            var fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = uint.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }
}
