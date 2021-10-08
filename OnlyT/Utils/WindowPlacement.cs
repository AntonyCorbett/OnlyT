namespace OnlyT.Utils
{
#pragma warning disable S101 // Types should be named in PascalCase
#pragma warning disable U2U1004 // Public value types should implement equality

    // ReSharper disable CommentTypo
    // ReSharper disable InconsistentNaming
    // ReSharper disable StyleCop.SA1307
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable StyleCop.SA1203
    // ReSharper disable StyleCop.SA1310
    // ReSharper disable UnusedMember.Global
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Interop;
    using System.Xml;
    using System.Xml.Serialization;

    // adapted from david Rickard's Tech Blog

    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    public static class WindowPlacement
    {
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        private static readonly Encoding Encoding = new UTF8Encoding();
        private static readonly XmlSerializer Serializer = new(typeof(WINDOWPLACEMENT));

        public static Rect SetPlacement(this Window window, string placementJson, Size overrideSize = default)
        {
            var width = window.Width;
            var height = window.Height;

            if (overrideSize != default)
            {
                width = overrideSize.Width;
                height = overrideSize.Height;
            }

            return SetPlacement(new WindowInteropHelper(window).Handle, placementJson, width, height);
        }

        public static string GetPlacement(this Window window)
        {
            return GetPlacement(new WindowInteropHelper(window).Handle);
        }

        public static (int x, int y) GetDpiSettings()
        {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

            if (dpiXProperty == null || dpiYProperty == null)
            {
                return (96, 96);
            }

            var x = dpiXProperty.GetValue(null, null);
            var y = dpiYProperty.GetValue(null, null);

            if(x == null || y == null)
            {
                return (96, 96);
            }

            return ((int)x, (int)y);
        }

        private static Rect SetPlacement(IntPtr windowHandle, string placementJson, double width, double height)
        {
            if (!string.IsNullOrEmpty(placementJson))
            {
                var xmlBytes = Encoding.GetBytes(placementJson);
                try
                {
                    WINDOWPLACEMENT placement;
                    using (MemoryStream memoryStream = new(xmlBytes))
                    {
                        var p = Serializer.Deserialize(memoryStream);
                        if (p == null)
                        {
                            return default;
                        }

                        placement = (WINDOWPLACEMENT)p;
                    }

                    var adjustedDimensions = GetAdjustedWidthAndHeight(width, height);

                    placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    placement.flags = 0;
                    placement.showCmd = placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd;
                    placement.normalPosition.Right = placement.normalPosition.Left + (int)adjustedDimensions.Item1;
                    placement.normalPosition.Bottom = placement.normalPosition.Top + (int)adjustedDimensions.Item2;
                    NativeMethods.SetWindowPlacement(windowHandle, ref placement);

                    return new Rect(
                        placement.normalPosition.Left,
                        placement.normalPosition.Top,
                        (int)adjustedDimensions.Item1,
                        (int)adjustedDimensions.Item2);
                }
#pragma warning disable CC0004 // Catch block cannot be empty
                catch (InvalidOperationException)
                {
                    // Parsing placement XML failed. Fail silently.
                }
#pragma warning restore CC0004 // Catch block cannot be empty
            }

            return default;
        }

        private static Tuple<double, double> GetAdjustedWidthAndHeight(double width, double height)
        {
            var (x, y) = GetDpiSettings();

            var adjustedWidth = (width * x) / 96.0;
            var adjustedHeight = (height * y) / 96.0;

            return new Tuple<double, double>(adjustedWidth, adjustedHeight);
        }

        private static string GetPlacement(IntPtr windowHandle)
        {
            NativeMethods.GetWindowPlacement(windowHandle, out var placement);

            using var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            Serializer.Serialize(xmlTextWriter, placement);
            var xmlBytes = memoryStream.ToArray();
            return Encoding.GetString(xmlBytes);
        }
    }
#pragma warning restore U2U1004 // Public value types should implement equality
#pragma warning restore S101 // Types should be named in PascalCase
}
