namespace OnlyT.Behaviours
{
    using Microsoft.Xaml.Behaviors;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    
    /// <summary>
    /// Helper class to disable effect of gratuitous double-clicking
    /// </summary>
    public class DoubleClickBehaviour : Behavior<Button>
    {
        protected override void OnAttached()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            base.OnAttached();
            AssociatedObject.PreviewMouseDoubleClick += AssociatedObjectOnPreviewMouseDoubleClick;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        protected override void OnDetaching()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            AssociatedObject.PreviewMouseDoubleClick -= AssociatedObjectOnPreviewMouseDoubleClick;
            base.OnDetaching();
#pragma warning restore CA1416 // Validate platform compatibility
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new System.NotImplementedException();
        }

        private void AssociatedObjectOnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            mouseButtonEventArgs.Handled = true;
        }
    }
}
