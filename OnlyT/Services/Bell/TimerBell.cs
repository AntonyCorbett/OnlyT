using OnlyT.EventTracking;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NAudio.Wave;
using System;
using System.IO;
using Serilog;

namespace OnlyT.Services.Bell;

/// <summary>
/// Manages the playing of the timer bell. See also IBellService
/// </summary>
internal sealed class TimerBell : ObservableObject, IDisposable
{
    private static readonly string BellFileName = "bell.mp3";
    private readonly string _bellFilePath;
    private WaveOutEvent? _player;
    private Mp3FileReader? _reader;
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
                OnPropertyChanged();

                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => WeakReferenceMessenger.Default.Send(
                        new BellStatusChangedMessage(value))));
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
                    Log.Logger.Error("Could not find bell file {BellFilePath}", _bellFilePath);
                }
            }
            catch (Exception ex)
            {
                IsPlaying = false;

                const string errMsg = "Could not play bell";
                EventTracker.Error(ex, errMsg);
                Log.Logger.Error(ex, errMsg);
            }
        }
    }

    public void Dispose()
    {
        ClearUp();
    }

    private static string GetBellFilePath()
    {
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(folder, BellFileName);
    }

    private void HandlePlaybackStopped(object? sender, StoppedEventArgs e)
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