namespace OnlyT.Services.CommandLine
{
    public interface ICommandLineService
    {
        bool NoGpu { get; set; }

        string OptionsIdentifier { get; set; }

        bool NoSettings { get; set; }

        bool IgnoreMutex { get; set; }

        bool Automate { get; set; }
    }
}
