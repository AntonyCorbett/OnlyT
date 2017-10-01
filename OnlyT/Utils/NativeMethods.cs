using System;
using System.Runtime.InteropServices;

namespace OnlyT.Utils
{
    /// <summary>
    /// Misc Native methods 
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
    }
}
