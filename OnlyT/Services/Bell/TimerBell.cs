using System;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using NAudio.Wave;
using Serilog;

namespace OnlyT.Services.Bell
{
   /// <summary>
   /// Manages the playing of the timer bell. See also IBellService
   /// </summary>
   internal sealed class TimerBell : ObservableObject, IDisposable
   {
      private static readonly string _bellFileName = "bell.mp3";
      private WaveOutEvent _player;
      private Mp3FileReader _reader;
      private readonly string _bellFilePath;
      
      public TimerBell()
      {
         _bellFilePath = GetBellFilePath();
      }

      private bool _playing;

      public bool IsPlaying
      {
         get => _playing;
         private set
         {
            if (_playing != value)
            {
               _playing = value;
               RaisePropertyChanged(nameof(IsPlaying));

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
                  _player.Volume = (float) volumePercent / 100;
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

      private void HandlePlaybackStopped(object sender, StoppedEventArgs e)
      {
         Clearup();
         IsPlaying = false;
      }

      private void Clearup()
      {
         _player?.Dispose();
         _reader?.Dispose();

         _player = null;
         _reader = null;
      }

      private static string GetBellFilePath()
      {
         string folder = AppDomain.CurrentDomain.BaseDirectory;
         return Path.Combine(folder, _bellFileName);
      }

      public void Dispose()
      {
         Clearup();
      }

   }
}
