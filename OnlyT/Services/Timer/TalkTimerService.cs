namespace OnlyT.Services.Timer
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;
    using EventArgs;
    using Models;

    /// <summary>
    /// Timer service
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class TalkTimerService : ITalkTimerService
    {
        private readonly Stopwatch _stopWatch = new();
        private readonly DispatcherTimer _timer = new(DispatcherPriority.Render);
        private readonly TimeSpan _timerInterval = TimeSpan.FromMilliseconds(100);
        private int _targetSecs = 600;
        private int _closingSecs = TalkScheduleItem.DefaultClosingSecs;
        private int? _talkId;
        private TimeSpan _currentTimeElapsed = TimeSpan.Zero;
        private int _currentSecondsElapsed;
        private bool _isCountingUp;

        public TalkTimerService()
        {
            _timer.Interval = _timerInterval;
            _timer.Tick += TimerElapsedHandler;
        }

        public event EventHandler<TimerChangedEventArgs>? TimerChangedEvent;

        public event EventHandler<TimerStartStopEventArgs>? TimerStartStopFromApiEvent;

        public event EventHandler<TimerDurationChangeEventArgs>? TimerDurationChangeFromApiEvent;

        /// <summary>
        /// Gets a value indicating whether the timer is running
        /// </summary>
        public bool IsRunning => _stopWatch.IsRunning;

        /// <summary>
        /// Gets or sets the current number of seconds elapsed
        /// </summary>
        public int CurrentSecondsElapsed
        {
            get => _currentSecondsElapsed;
            set
            {
                if (_currentSecondsElapsed != value)
                {
                    _currentSecondsElapsed = value;
                    OnTimerChangedEvent(new TimerChangedEventArgs
                    {
                        TargetSecs = _targetSecs,
                        ElapsedSecs = _currentSecondsElapsed,
                        IsRunning = IsRunning,
                        ClosingSecs = _closingSecs,
                    });
                }
            }
        }

        /// <summary>
        /// Gets or sets the current elapsed time
        /// </summary>
        private TimeSpan CurrentTimeElapsed
        {
            get => _currentTimeElapsed;
            set
            {
                if (_currentTimeElapsed != value)
                {
                    _currentTimeElapsed = value;
                    CurrentSecondsElapsed = (int)_currentTimeElapsed.TotalSeconds;
                }
            }
        }

        public void SetupTalk(int talkId, int targetSeconds, int closingSecs)
        {
            _talkId = talkId;
            _targetSecs = targetSeconds;
            _closingSecs = closingSecs;
        }

        public TimerStartStopEventArgs StartTalkTimerFromApi(int talkId)
        {
            var result = new TimerStartStopEventArgs
            {
                TalkId = talkId,
                Command = StartStopTimerCommands.Start
            };

            OnTimerStartStopFromApiEvent(result);
            return result;
        }

        public TimerStartStopEventArgs StopTalkTimerFromApi(int talkId)
        {
            var result = new TimerStartStopEventArgs
            {
                TalkId = talkId,
                Command = StartStopTimerCommands.Stop
            };

            OnTimerStartStopFromApiEvent(result);
            return result;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        /// <param name="targetSecs">The target duration of the talk.</param>
        /// <param name="talkId">The Id of the talk that is being timed.</param>
        /// <param name="isCountingUp">Indicates if the timer is counting up rather than down.</param>
        public void Start(int targetSecs, int talkId, bool isCountingUp)
        {
            _targetSecs = targetSecs;
            _talkId = talkId;
            _isCountingUp = isCountingUp;
            _stopWatch.Start();
            UpdateTimerValue();
            _timer.Start();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _talkId = null;

            _stopWatch.Reset();
            UpdateTimerValue();
        }

        public TimerStatus GetStatus()
        {
            return new TimerStatus
            {
                TalkId = _talkId,
                TargetSeconds = _targetSecs,
                IsRunning = IsRunning,
                TimeElapsed = CurrentTimeElapsed,
                ClosingSecs = _closingSecs
            };
        }
        
        public ClockRequestInfo GetClockRequestInfo()
        {
            return new ClockRequestInfo
            {
                TargetSeconds = _targetSecs,
                ElapsedTime = _currentTimeElapsed,
                IsRunning = IsRunning,
                IsCountingUp = _isCountingUp,
                ClosingSecs = _closingSecs,
            };
        }

        private void OnTimerChangedEvent(TimerChangedEventArgs e)
        {
            TimerChangedEvent?.Invoke(this, e);
        }

        private void TimerElapsedHandler(object? sender, EventArgs e)
        {
            _timer.Stop();
            UpdateTimerValue();
            _timer.Start();
        }

        private void UpdateTimerValue()
        {
            CurrentTimeElapsed = _stopWatch.Elapsed;
        }

        public TimerDurationChangeEventArgs ChangeDurationFromApi(int talkId, int deltaSeconds)
        {
            var result = new TimerDurationChangeEventArgs
            {
                TalkId = talkId,
                DeltaSeconds = deltaSeconds
            };

            OnTimerDurationChangeFromApiEvent(result);
            return result;
        }

        private void OnTimerStartStopFromApiEvent(TimerStartStopEventArgs e)
        {
            TimerStartStopFromApiEvent?.Invoke(this, e);
        }

        private void OnTimerDurationChangeFromApiEvent(TimerDurationChangeEventArgs e)
        {
            TimerDurationChangeFromApiEvent?.Invoke(this, e);
        }
    }
}
