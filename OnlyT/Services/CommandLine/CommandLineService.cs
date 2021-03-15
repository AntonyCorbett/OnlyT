namespace OnlyT.Services.CommandLine
{
    using System;
    using System.Globalization;
    using Fclp;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CommandLineService : ICommandLineService
    {
        public CommandLineService()
        {
            var p = new FluentCommandLineParser();

            p.Setup<bool>("nogpu")
                .Callback(s => { NoGpu = s; }).SetDefault(false);

            p.Setup<string>("id")
                .Callback(s => { OptionsIdentifier = s; }).SetDefault(null!);

            p.Setup<bool>("nosettings")
                .Callback(s => { NoSettings = s; }).SetDefault(false);

            p.Setup<bool>("nomutex")
                .Callback(s => { IgnoreMutex = s; }).SetDefault(false);

            p.Setup<int>("monitor")
                .Callback(s => { TimerMonitorIndex = s; }).SetDefault(0);

            p.Setup<int>("cmonitor")
                .Callback(s => { CountdownMonitorIndex = s; }).SetDefault(0);

            p.Setup<bool>("automate")
                .Callback(s => { Automate = s; }).SetDefault(false);

            p.Setup<string>("datetime")
                .Callback(s =>
                {
                    if (DateTime.TryParseExact(
                        s, 
                        "yyyy-MM-dd:HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var result))
                    {
                        DateTimeOnLaunch = result;
                    }
                }).SetDefault(DateTime.UtcNow.ToString("yyyy-MM-dd:HH:mm"));

            p.Parse(Environment.GetCommandLineArgs());
        }

        public bool NoGpu { get; set; }

        public string? OptionsIdentifier { get; set; }

        public bool NoSettings { get; set; }

        public bool IgnoreMutex { get; set; }

        public int TimerMonitorIndex { get; set; }

        public int CountdownMonitorIndex { get; set; }

        public bool Automate { get; set; }

        public DateTime DateTimeOnLaunch { get; set; }

        public bool IsTimerMonitorSpecified => TimerMonitorIndex > 0;

        public bool IsCountdownMonitorSpecified => CountdownMonitorIndex > 0;
    }
}
