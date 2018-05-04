namespace OnlyT.Services.Options.MeetingStartTimes
{
    // ReSharper disable MemberCanBePrivate.Global
    using System;
    using System.Globalization;
    using System.Linq;
    using Models;

    public class MeetingStartTime
    {
        public DayOfWeek? DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public static MeetingStartTime FromText(string text)
        {
            var dayOfWeek = FindDayOfWeek(text);
            if (dayOfWeek != null)
            {
                text = text.Replace(dayOfWeek.UserSuppliedDayName, string.Empty);
            }

            var time = GetTimeOfDay(text);
            if (time != null)
            {
                return new MeetingStartTime
                {
                    DayOfWeek = dayOfWeek?.DayOfWeek,
                    StartTime = time.Value
                };
            }

            return null;
        }

        public void Sanitize()
        {
            if (StartTime.TotalHours > 24)
            {
                StartTime = TimeSpan.FromHours(24);
            }
        }

        public string AsText()
        {
            var timeString = StartTime.ToString(@"hh\:mm");
            
            if (DayOfWeek != null)
            {
                var dayName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(DayOfWeek.Value);
                return $"{dayName} {timeString}";
            }

            return timeString;
        }

        private static TimeSpan? GetTimeOfDay(string text)
        {
            bool hasPm = HasPm(text);
            
            int hour = -1;
            int mins = -1;

            var digits = text.Where(char.IsDigit).ToArray();
            if (digits.Length > 0 && digits.Length < 5)
            {
                switch (digits.Length)
                {
                    case 1:
                        hour = int.Parse(digits[0].ToString());
                        mins = 0;
                        break;

                    case 2:
                        hour = int.Parse($"{digits[0]}{digits[1]}");
                        mins = 0;
                        break;

                    case 3:
                        hour = int.Parse(digits[0].ToString());
                        mins = int.Parse($"{digits[1]}{digits[2]}");
                        break;
                        
                    case 4:
                        hour = int.Parse($"{digits[0]}{digits[1]}");
                        mins = int.Parse($"{digits[2]}{digits[3]}");
                        break;
                }
            }

            if (hour < 12 && hasPm)
            {
                hour += 12;
            }
            
            if (hour > -1 && mins > -1 && mins < 60 && hour <= 24)
            {
                return new TimeSpan(hour, mins, 0);
            }
            
            return null;
        }

        private static bool HasPm(string text)
        {
            var trimmedText = text.Trim();

            var currentCulturePmString = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
            var defaultCulturePmString = CultureInfo.InvariantCulture.DateTimeFormat.PMDesignator;

            return 
                (!string.IsNullOrEmpty(currentCulturePmString) && trimmedText.EndsWith(
                    currentCulturePmString, StringComparison.CurrentCultureIgnoreCase)) ||
                (!string.IsNullOrEmpty(defaultCulturePmString) && trimmedText.EndsWith(
                    defaultCulturePmString, StringComparison.InvariantCultureIgnoreCase));
        }

        private delegate string GetDayNameFunction(DayOfWeek dayOfWeek);

        private static DayOfWeekAndUserSuppliedDayName FindDayOfWeek(string text)
        {
            return FindDayOfWeek(text, (dow) => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dow)) ??
                   FindDayOfWeek(text, (dow) => CultureInfo.InvariantCulture.DateTimeFormat.GetDayName(dow)) ??
                   
                   FindDayOfWeek(text, (dow) => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(dow)) ??
                   FindDayOfWeek(text, (dow) => CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedDayName(dow)) ??
                   
                   FindDayOfWeek(text, (dow) => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(dow)) ??
                   FindDayOfWeek(text, (dow) => CultureInfo.InvariantCulture.DateTimeFormat.GetShortestDayName(dow));
        }
        
        private static DayOfWeekAndUserSuppliedDayName FindDayOfWeek(string text, GetDayNameFunction getDayNameFunction)
        {
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                var dayName = getDayNameFunction(dayOfWeek);
                if (text.IndexOf(dayName, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return new DayOfWeekAndUserSuppliedDayName
                    {
                        DayOfWeek = dayOfWeek,
                        UserSuppliedDayName = dayName
                    };
                }
            }

            return null;
        }
    }
}
