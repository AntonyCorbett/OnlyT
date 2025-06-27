using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnlyT.Behaviours;
   
/// <summary>
/// Helper class to disable effect of gratuitous double-clicking
/// </summary>
public class DoubleClickBehaviour : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseDoubleClick += AssociatedObjectOnPreviewMouseDoubleClick;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseDoubleClick -= AssociatedObjectOnPreviewMouseDoubleClick;
        base.OnDetaching();
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