using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Interop;
using OnlyT.Utils;

namespace OnlyT.Behaviours
{
    public class HideCloseButtonBehaviour : Behavior<Window>
    {
        private const int GwlStyle = -16;
        private const int WsSysMenu = 0x80000;

        protected override void OnAttached()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        protected override void OnDetaching()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            AssociatedObject.Loaded -= OnLoaded;
            base.OnDetaching();
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var hwnd = new WindowInteropHelper(AssociatedObject).Handle;
#pragma warning restore CA1416 // Validate platform compatibility
            
            _ = NativeMethods.SetWindowLong(hwnd, GwlStyle, NativeMethods.GetWindowLong(hwnd, GwlStyle) & ~WsSysMenu);
        }
    }
}
