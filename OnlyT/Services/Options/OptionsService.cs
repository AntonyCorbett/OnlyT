using System;
using System.IO;
using Newtonsoft.Json;
using OnlyT.Utils;
using Serilog;

namespace OnlyT.Services.Options
{
    /// <summary>
    /// Service to deal with program settings
    /// </summary>
    public class OptionsService : IOptionsService
    {
        private Options _options;
        private readonly int _optionsVersion = 1;
        private string _optionsFilePath;
        private string _originalOptionsSignature;

        public Options Options
        {
            get
            {
                Init();
                return _options;
            }
        }

        private void Init()
        {
            if (_options == null)
            {
                try
                {
                    string commandLineIdentifier = CommandLineParser.Instance.GetId();
                    _optionsFilePath = FileUtils.GetUserOptionsFilePath(commandLineIdentifier, _optionsVersion);
                    var path = Path.GetDirectoryName(_optionsFilePath);
                    if (path != null)
                    {
                        Directory.CreateDirectory(path);
                        ReadOptions();
                    }

                    if (_options == null)
                    {
                        _options = new Options();
                    }

                    // store the original settings so that we can determine if they have changed
                    // when we come to save them
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
                // ReSharper disable once CatchAllClause
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not read options file");
                    _options = new Options();
                }
            }
        }

        private string GetOptionsSignature(Options options)
        {
            // config data is small so simple solution is best...
            return JsonConvert.SerializeObject(options);
        }

        private void ReadOptions()
        {
            if (!File.Exists(_optionsFilePath))
            {
                WriteDefaultOptions();
            }
            else
            {
                using (StreamReader file = File.OpenText(_optionsFilePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _options = (Options)serializer.Deserialize(file, typeof(Options));
                    
                    SetMidWeekOrWeekend();
                    ResetCircuitVisit();
                    
                    _options.Sanitize();
                }
            }
        }

        private void ResetCircuitVisit()
        {
            // when the settings are read we ignore this saved setting 
            // and reset to false...
            _options.IsCircuitVisit = false;
        }

        private bool IsWeekend()
        {
            var now = DateTime.Now;
            return now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;
        }

        private void SetMidWeekOrWeekend()
        {
            // when the settings are read we ignore this saved setting 
            // and reset according to current day of week...

            _options.MidWeekOrWeekend = IsWeekend()
               ? MidWeekOrWeekend.Weekend
               : MidWeekOrWeekend.MidWeek;
        }

        private void WriteDefaultOptions()
        {
            _options = new Options();
            WriteOptions();
        }

        private void WriteOptions()
        {
            if (_options != null)
            {
                using (StreamWriter file = File.CreateText(_optionsFilePath))
                {
                    JsonSerializer serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, _options);
                    _originalOptionsSignature = GetOptionsSignature(_options);
                }
            }
        }


        /// <summary>
        /// Saves the settings (if they have changed since they were last read)
        /// </summary>
        public void Save()
        {
            try
            {
                var newSignature = GetOptionsSignature(_options);
                if (_originalOptionsSignature != newSignature)
                {
                    // changed...
                    WriteOptions();
                    Log.Logger.Information("Settings changed and saved");
                }
            }
            // ReSharper disable once CatchAllClause
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save settings");
            }
        }

        /// <summary>
        /// Determines if the timer monitor is specified
        /// </summary>
        public bool IsTimerMonitorSpecified
        {
            get
            {
                Init();
                return !string.IsNullOrEmpty(Options.TimerMonitorId);
            }
        }
    }
}
