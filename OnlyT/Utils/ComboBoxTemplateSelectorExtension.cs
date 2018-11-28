// adapted from StackOverflow post (courtesy Mark A. Donohoe)
namespace OnlyT.Utils
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    public class ComboBoxTemplateSelectorExtension : MarkupExtension
    {
        public DataTemplate SelectedItemTemplate { get; set; }

        public DataTemplateSelector SelectedItemTemplateSelector { get; set; }

        public DataTemplate DropdownItemsTemplate { get; set; }

        public DataTemplateSelector DropdownItemsTemplateSelector { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ComboBoxTemplateSelector()
            {
                SelectedItemTemplate = SelectedItemTemplate,
                SelectedItemTemplateSelector = SelectedItemTemplateSelector,
                DropdownItemsTemplate = DropdownItemsTemplate,
                DropdownItemsTemplateSelector = DropdownItemsTemplateSelector
            };
        }
    }
}
