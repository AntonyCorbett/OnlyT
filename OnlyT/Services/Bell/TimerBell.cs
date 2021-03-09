using Microsoft.Toolkit.Mvvm.ComponentModel;
using NAudio.Wave;

namespace OnlyT.Services.Bell
{
    using System;
    using System.IO;
    using Serilog;

    /// <summary>
    /// Manages the playing of the timer bell. See also IBellService
    /// </summary>
    internal sealed class TimerBell : ObservableObject, IDisposable
    {
        private static readonly string BellFileName = "bell.mp3";
        private readonly string _bellFilePath;
        private WaveOutEvent _player;
        private Mp3FileReader _reader;
        private bool _playing;

        public TimerBell()
        {
            _bellFilePath = GetBellFilePath();
        }

        public bool IsPlaying
        {
            get => _playing;
            private set
            {
                if (_playing != value)
                {
                    _playing = value;
                    OnPropertyChanged(nameof(IsPlaying));

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Messenger.Default.Send(new BellStatusChangedMessage(value));
                    });
                }
            }
        }

        public void Play(int volumePercent)
        {
            if (!IsPlaying)
            {
                try
                {
                    if (File.Exists(_bellFilePath))
                    {
                        IsPlaying = true;

                        _player = new WaveOutEvent();
                        _reader = new Mp3FileReader(_bellFilePath);
                        _player.Init(_reader);
                        _player.PlaybackStopped += HandlePlaybackStopped;
                        _player.Volume = (float)volumePercent / 100;
                        _player.Play();
                    }
                    else
                    {
                        Log.Logger.Error($"Could not find bell file {_bellFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    IsPlaying = false;
                    Log.Logger.Error(ex, "Could not play bell");
                }
            }
        }

        public void Dispose()
        {
            ClearUp();
        }

        private static string GetBellFilePath()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(folder, BellFileName);
        }

        private void HandlePlaybackStopped(object sender, StoppedEventArgs e)
        {
            ClearUp();
            IsPlaying = false;
        }

        private void ClearUp()
        {
            if (_player != null)
            {
                _player.PlaybackStopped -= HandlePlaybackStopped;
            }

            _player?.Dispose();
            _reader?.Dispose();

            _player = null;
            _reader = null;
        }
    }
}
