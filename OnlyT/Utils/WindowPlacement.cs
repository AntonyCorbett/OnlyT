﻿namespace OnlyT.Utils
{
#pragma warning disable S101 // Types should be named in PascalCase
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1310 // Field names must not contain underscore

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
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

        public static Rect SetPlacement(this Window window, string placementJson, Size overrideSize = default(Size))
        {
            double width = window.Width;
            double height = window.Height;

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
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            if (dpiXProperty == null || dpiYProperty == null)
            {
                return (96, 96);
            }

            return ((int)dpiXProperty.GetValue(null, null), (int)dpiYProperty.GetValue(null, null));
        }

        private static Rect SetPlacement(IntPtr windowHandle, string placementJson, double width, double height)
        {
            if (!string.IsNullOrEmpty(placementJson))
            {
                byte[] xmlBytes = Encoding.GetBytes(placementJson);
                try
                {
                    WINDOWPLACEMENT placement;
                    using (MemoryStream memoryStream = new MemoryStream(xmlBytes))
                    {
                        placement = (WINDOWPLACEMENT)Serializer.Deserialize(memoryStream);
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
                catch (InvalidOperationException)
                {
                    // Parsing placement XML failed. Fail silently.
                }
            }

            return default;
        }

        private static Tuple<double, double> GetAdjustedWidthAndHeight(double width, double height)
        {
            var dpi = GetDpiSettings();

            var adjustedWidth = (width * dpi.x) / 96.0;
            var adjustedHeight = (height * dpi.y) / 96.0;

            return new Tuple<double, double>(adjustedWidth, adjustedHeight);
        }

        private static string GetPlacement(IntPtr windowHandle)
        {
            NativeMethods.GetWindowPlacement(windowHandle, out var placement);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                Serializer.Serialize(xmlTextWriter, placement);
                byte[] xmlBytes = memoryStream.ToArray();
                return Encoding.GetString(xmlBytes);
            }
        }
    }
#pragma warning restore SA1310 // Field names must not contain underscore
#pragma warning restore SA1649 // File name must match first type name
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter
#pragma warning restore S101 // Types should be named in PascalCase
}
