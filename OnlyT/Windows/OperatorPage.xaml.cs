namespace OnlyT.Windows
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using ViewModel;

    /// <summary>
    /// Interaction logic for OperatorPage.xaml
    /// </summary>
    public partial class OperatorPage : UserControl
    {
        public OperatorPage()
        {
            InitializeComponent();
        }

        private void ManualDurationTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox textBox && e.NewValue is bool isVisible && isVisible)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }, DispatcherPriority.Input);
            }
        }

        private void ManualDurationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not OperatorPageViewModel viewModel)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Return:
                    if (viewModel.AcceptManualDurationEditCommand.CanExecute(null))
                    {
                        viewModel.AcceptManualDurationEditCommand.Execute(null);
                        e.Handled = true;
                    }

                    break;

                case Key.Escape:
                    if (viewModel.CancelManualDurationEditCommand.CanExecute(null))
                    {
                        viewModel.CancelManualDurationEditCommand.Execute(null);
                        e.Handled = true;
                    }

                    break;
            }
        }
    }
}
