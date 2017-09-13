using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.MeetingSongsFile
{
   internal class MeetingSongsAndTimers
   {
      public const string LIVING_TIMER1_KEY = "timerL1";
      public const string LIVING_TIMER2_KEY = "timerL2";

      private readonly List<int> _songNos;
      private readonly Dictionary<string, int> _timerValues;

      public IReadOnlyList<int> SongNos => _songNos;

      public IReadOnlyDictionary<string, int> TimerValues => _timerValues;

      public MeetingSongsAndTimers()
      {
         _songNos = new List<int>();
         _timerValues = new Dictionary<string, int>();
      }

      public void AddSong(int songNum)
      {
         _songNos.Add(songNum);
      }

      public void AddTimer(string key, int mins)
      {
         _timerValues.Add(key, mins);
      }

      public int SongCount => _songNos.Count;

      public int TimersCount => _timerValues.Count;

      public void AddNullSong()
      {
         _songNos.Insert(0, 0);
      }
   }
}
