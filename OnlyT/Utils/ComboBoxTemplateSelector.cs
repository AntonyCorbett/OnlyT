// adapted from StackOverflow post (courtesy Mark A. Donohoe)
namespace OnlyT.Utils
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ComboBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }

        public DataTemplateSelector SelectedItemTemplateSelector { get; set; }

        public DataTemplate DropdownItemsTemplate { get; set; }

        public DataTemplateSelector DropdownItemsTemplateSelector { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parent = container;

            // Search up the visual tree, stopping at either a ComboBox or
            // a ComboBoxItem (or null). This will determine which template to use
            while (parent != null && !(parent is ComboBoxItem) && !(parent is ComboBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            // If you stopped at a ComboBoxItem, you're in the dropdown
            var inDropDown = parent is ComboBoxItem;

            return inDropDown
                ? DropdownItemsTemplate ?? DropdownItemsTemplateSelector?.SelectTemplate(item, container)
                : SelectedItemTemplate ?? SelectedItemTemplateSelector?.SelectTemplate(item, container);
        }
    }
}
