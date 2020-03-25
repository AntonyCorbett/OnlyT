namespace OnlyT.Behaviours
{
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Interop;
    using OnlyT.Utils;

    public class HideCloseButtonBehaviour : Behavior<Window>
    {
        private const int GwlStyle = -16;
        private const int WsSysMenu = 0x80000;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(AssociatedObject).Handle;
            NativeMethods.SetWindowLong(hwnd, GwlStyle, NativeMethods.GetWindowLong(hwnd, GwlStyle) & ~WsSysMenu);
        }
    }
}
