using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using OnlyT.Services.Options;
using Serilog;

namespace OnlyT.Services.Bell
{
   internal sealed class TimerBell
   {
      private static readonly string _bellFileName = "bell.mp3";
      private static readonly Lazy<Mp3FileReader> _mp3Reader = new Lazy<Mp3FileReader>(Mp3ReaderFactory);
      private static readonly Lazy<WaveOut> _waveOut = new Lazy<WaveOut>(WaveOutFactory);

      private static Mp3FileReader Mp3ReaderFactory()
      {
         Mp3FileReader result = null;

         var path = GetBellFilePath();
         if (!File.Exists(path))
         {
            Log.Logger.Error($"Could not find file: {path}");
         }
         else
         {
            result = new Mp3FileReader(path);
         }

         return result;
      }

      private static WaveOut WaveOutFactory()
      {
         WaveOut result = null;

         if (_mp3Reader.Value != null)
         {
            try
            {
               result = new WaveOut();
               result.Init(_mp3Reader.Value);
            }
            catch (Exception ex)
            {
               Log.Logger.Error(ex, "Could not init wave out");
            }
         }

         return result;
      }

      public void Play(int volumePercent)
      {
         try
         {
            if(_waveOut.Value != null)
            { 
               _waveOut.Value.Volume = (float) volumePercent / 100;
               _waveOut.Value.Play();
            }
         }
         catch (Exception ex)
         {
            Log.Logger.Error(ex, "Could not play bell");
         }
      }

      private static string GetBellFilePath()
      {
         string folder = AppDomain.CurrentDomain.BaseDirectory;
         return Path.Combine(folder, _bellFileName);
      }
   }
}
