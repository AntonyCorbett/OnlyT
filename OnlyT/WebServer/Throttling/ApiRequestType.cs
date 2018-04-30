namespace OnlyT.WebServer.Throttling
{
    internal enum ApiRequestType
    {
        Unknown,
        ClockPage,
        ClockData,
        Timer,
        TimerControl,
        Version,
        Bell,
        DateTime,
        System
    }
}
