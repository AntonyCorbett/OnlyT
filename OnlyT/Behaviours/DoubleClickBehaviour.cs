using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace OnlyT.Behaviours
{
    /// <summary>
    /// Helper class to disable effect of gratuitous double-clicking
    /// </summary>
    public class DisableDoubleClickBehavior : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDoubleClick += AssociatedObjectOnPreviewMouseDoubleClick;
        }

        private void AssociatedObjectOnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            mouseButtonEventArgs.Handled = true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDoubleClick -= AssociatedObjectOnPreviewMouseDoubleClick;
            base.OnDetaching();
        }
    }
}
