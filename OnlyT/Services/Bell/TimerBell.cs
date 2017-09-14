using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using OnlyT.Services.Options;
using Serilog;

namespace OnlyT.Services.Bell
{
   internal sealed class TimerBell : IDisposable
   {
      private static readonly string _bellFileName = "bell.mp3";
      private readonly WaveOutEvent _player;
      private Mp3FileReader _reader;

      public TimerBell()
      {
         _player = new WaveOutEvent();
      }

      public void Play(int volumePercent)
      {
         try
         {
            _reader = new Mp3FileReader(GetBellFilePath());
            _player.Init(_reader);
            _player.PlaybackStopped += HandlePlaybackStopped;
            _player.Volume = (float) volumePercent / 100;
            _player.Play();
         }
         catch (Exception ex)
         {
            Log.Logger.Error(ex, "Could not play bell");
         }
      }

      private void HandlePlaybackStopped(object sender, StoppedEventArgs e)
      {
         _reader?.Dispose();
      }

      private static string GetBellFilePath()
      {
         string folder = AppDomain.CurrentDomain.BaseDirectory;
         return Path.Combine(folder, _bellFileName);
      }

      public void Dispose()
      {
         _player?.Dispose();
         _reader?.Dispose();
      }
   }
}
