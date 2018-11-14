namespace OnlyT.Services.CommandLine
{
    using System;
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
                .Callback(s => { OptionsIdentifier = s; }).SetDefault(null);

            p.Setup<bool>("nosettings")
                .Callback(s => { NoSettings = s; }).SetDefault(false);

            p.Setup<bool>("nomutex")
                .Callback(s => { IgnoreMutex = s; }).SetDefault(false);

            p.Setup<bool>("automate")
                .Callback(s => { Automate = s; }).SetDefault(false);

            p.Parse(Environment.GetCommandLineArgs());
        }

        public bool NoGpu { get; set; }

        public string OptionsIdentifier { get; set; }

        public bool NoSettings { get; set; }

        public bool IgnoreMutex { get; set; }

        public bool Automate { get; set; }
    }
}
