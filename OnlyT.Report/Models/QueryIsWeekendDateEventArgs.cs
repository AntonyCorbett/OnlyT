namespace OnlyT.Report.Models
{
    using System;

    public class QueryIsWeekendDateEventArgs : EventArgs
    {
        public DateTime Date { get; set; }

        public bool IsWeekend { get; set; }
    }
}
