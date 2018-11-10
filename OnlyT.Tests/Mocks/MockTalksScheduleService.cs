namespace OnlyT.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OnlyT.Models;
    using OnlyT.Services.TalkSchedule;

    internal class MockTalksScheduleService : ITalkScheduleService
    {
        private readonly int _talkIdStart;
        private readonly int _numTalks;

        private List<TalkScheduleItem> _talks;

        public MockTalksScheduleService(int talkIdStart, int numTalks)
        {
            _talkIdStart = talkIdStart;
            _numTalks = numTalks;
        }

        public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
        {
            if (_talks == null)
            {
                _talks = new List<TalkScheduleItem>();

                int talkId = _talkIdStart;

                for (int n = 0; n < _numTalks; ++n)
                {
                    _talks.Add(new TalkScheduleItem
                    {
                        Id = talkId + n, 
                        Name = $"Talk {n + 1}", 
                        OriginalDuration = TimeSpan.FromMinutes(n + 1) 
                    });
                }
            }

            return _talks;
        }

        public TalkScheduleItem GetTalkScheduleItem(int id)
        {
            return _talks.FirstOrDefault(t => t.Id.Equals(id));
        }

        public int GetNext(int currentTalkId)
        {
            for (int n = 0; n < _talks.Count; ++n)
            {
                if (_talks[n].Id.Equals(currentTalkId) && n < _talks.Count - 1)
                {
                    return _talks[n + 1].Id;
                }
            }

            return 0;
        }

        public void Reset()
        {
            _talks = null;
        }
    }
}
