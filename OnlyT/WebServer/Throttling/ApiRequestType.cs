namespace OnlyT.WebServer.Throttling
{
    internal enum ApiRequestType
    {
        Unknown,
        ClockPage,
        ClockData,
        Timer,
        TimerControlStart,
        TimerControlStop,
        Version,
        Bell,
        DateTime,
        System
    }
}
