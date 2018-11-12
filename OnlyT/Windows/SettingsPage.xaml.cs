namespace OnlyT.Windows
{
    using System.Diagnostics;
    using System.Windows.Controls;
    using OnlyT.Services.CommandLine;
    using OnlyT.Utils;

    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        private readonly ICommandLineService _commandlineService;

        public SettingsPage(ICommandLineService commandlineService)
        {
            _commandlineService = commandlineService;
            InitializeComponent();
        }

        private void ReportIconMouseLeftButtonDown(
            object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(FileUtils.GetTimingReportsFolder(_commandlineService.OptionsIdentifier));
        }
    }
}
