using System.Collections.Generic;

namespace OnlyT.MeetingSongsFile
{
    /// <summary>
    /// Stores the meeting songs and talk timers for use in "Automatic mode". These
    /// values are read from an online xml feed. This code extracted from SoundBox.
    /// </summary>
    internal class MeetingSongsAndTimers
    {
        public const string LivingTimer1Key = "timerL1";
        public const string LivingTimer2Key = "timerL2";

        public const string MinistryTimer1Key = "timerM1";
        public const string MinistryTimer2Key = "timerM2";
        public const string MinistryTimer3Key = "timerM3";

        private readonly List<int> _songNos;
        private readonly Dictionary<string, TimerValueAndBellFlag> _timerValues;

        public IReadOnlyList<int> SongNos => _songNos;

        public IReadOnlyDictionary<string, TimerValueAndBellFlag> TimerValues => _timerValues;

        public MeetingSongsAndTimers()
        {
            _songNos = new List<int>();
            _timerValues = new Dictionary<string, TimerValueAndBellFlag>();
        }

        public void AddSong(int songNum)
        {
            _songNos.Add(songNum);
        }

        public void AddTimer(string key, int mins, bool useBell)
        {
            _timerValues.Add(key, new TimerValueAndBellFlag { TimerMinutes = mins, UseBell = useBell });
        }

        public int SongCount => _songNos.Count;

        public int TimersCount => _timerValues.Count;

        public void AddNullSong()
        {
            _songNos.Insert(0, 0);
        }
    }
}
