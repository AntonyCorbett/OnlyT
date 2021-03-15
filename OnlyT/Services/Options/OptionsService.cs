using Microsoft.Toolkit.Mvvm.Messaging;

namespace OnlyT.Services.Options
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Markup;
    using CommandLine;
    using Newtonsoft.Json;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Services.LogLevelSwitch;
    using OnlyT.Services.Monitors;
    using OnlyT.ViewModel.Messages;
    using Serilog;
    using Utils;

    /// <summary>
    /// Service to deal with program settings
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class OptionsService : IOptionsService
    {
        private readonly ICommandLineService _commandLineService;
        private readonly ILogLevelSwitchService _logLevelSwitchService;
        private readonly IMonitorsService _monitorsService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IQueryWeekendService _queryWeekendService;
        private readonly int _optionsVersion = 1;
        private Options? _options;
        private string _optionsFilePath;
        private string _originalOptionsSignature;
        
        public OptionsService(
            ICommandLineService commandLineService,
            ILogLevelSwitchService logLevelSwitchService,
            IMonitorsService monitorsService,
            IDateTimeService dateTimeService,
            IQueryWeekendService queryWeekendService)
        {
            _commandLineService = commandLineService;
            _logLevelSwitchService = logLevelSwitchService;
            _monitorsService = monitorsService;
            _dateTimeService = dateTimeService;
            _queryWeekendService = queryWeekendService;

            WeakReferenceMessenger.Default.Register<LogLevelChangedMessage>(this, OnLogLevelChanged);
        }

        public Options Options
        {
            get
            {
                Init();
                return _options!;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the timer monitor is specified
        /// </summary>
        public bool IsTimerMonitorSpecified
        {
            get
            {
                Init();
                return !string.IsNullOrEmpty(Options.TimerMonitorId);
            }
        }

        public bool CanDisplayTimerWindow => IsTimerMonitorSpecified || Options.MainMonitorIsWindowed;

        public bool CanDisplayCountdownWindow => IsCountdownMonitorSpecified || Options.CountdownMonitorIsWindowed;

        /// <summary>
        /// Gets a value indicating whether the countdown monitor is specified
        /// </summary>
        public bool IsCountdownMonitorSpecified
        {
            get
            {
                Init();
                return !string.IsNullOrEmpty(Options.CountdownMonitorId);
            }
        }

        public bool IsTimerMonitorSetByCommandLine { get; private set; }

        public bool IsCountdownMonitorSetByCommandLine { get; private set; }

        public bool Use24HrClockFormat()
        {
            switch (Options.ClockHourFormat)
            {
                case ClockHourFormat.Format24:
                case ClockHourFormat.Format24LeadingZero:
                    return true;

                default:
                    return false;
            }
        }

        public AdaptiveMode GetAdaptiveMode()
        {
            AdaptiveMode result = AdaptiveMode.None;

            if (Options.OperatingMode == OperatingMode.Automatic)
            {
                switch (Options.MidWeekOrWeekend)
                {
                    case MidWeekOrWeekend.MidWeek:
                        return Options.MidWeekAdaptiveMode;

                    case MidWeekOrWeekend.Weekend:
                        return Options.WeekendAdaptiveMode;
                }
            }

            return result;
        }

        public bool IsWeekend(DateTime theDate)
        {
            return _queryWeekendService.IsWeekend(theDate, _options!.WeekendIncludesFriday);
        }

        public bool IsNowWeekend()
        {
            return IsWeekend(_dateTimeService.Now());
        }

        /// <summary>
        /// Saves the settings (if they have changed since they were last read)
        /// </summary>
        public void Save()
        {
            try
            {
                var newSignature = GetOptionsSignature(_options!);
                if (_originalOptionsSignature != newSignature)
                {
                    // changed...
                    WriteOptions();
                    Log.Logger.Information("Settings changed and saved");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save settings");
            }
        }

        private void Init()
        {
            if (_options == null)
            {
                try
                {
                    string commandLineIdentifier = _commandLineService.OptionsIdentifier;
                    _optionsFilePath = FileUtils.GetUserOptionsFilePath(commandLineIdentifier, _optionsVersion);
                    var path = Path.GetDirectoryName(_optionsFilePath);
                    if (path != null)
                    {
                        FileUtils.CreateDirectory(path);
                        ReadOptions();
                    }

                    _options ??= new Options();

                    // store the original settings so that we can determine if they have changed
                    // when we come to save them
                    _originalOptionsSignature = GetOptionsSignature(_options);

                    _logLevelSwitchService.SetMinimumLevel(Options.LogEventLevel);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not read options file");
                    _options = new Options();
                }
            }
        }

        private void SetCulture()
        {
            var culture = _options!.Culture;

            if (string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.Name;
            }

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not set culture");
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
                using var file = File.OpenText(_optionsFilePath);

                var serializer = new JsonSerializer();
                _options = (Options?)serializer.Deserialize(file, typeof(Options));
                if (_options != null)
                {
                    SetMidWeekOrWeekend();
                    ResetCircuitVisit();

                    _options.Sanitize();

                    CommandLineMonitorOverride();

                    SetCulture();
                }
            }
        }
        
        private void CommandLineMonitorOverride()
        {
            // if the monitors are specified on the command-line then override those
            // stored in options...
            if (_commandLineService.IsTimerMonitorSpecified || _commandLineService.IsCountdownMonitorSpecified)
            {
                var monitors = _monitorsService.GetSystemMonitors().ToArray();

                IsTimerMonitorSetByCommandLine =
                    _commandLineService.IsTimerMonitorSpecified &&
                    _commandLineService.TimerMonitorIndex <= monitors.Length;

                if (IsTimerMonitorSetByCommandLine)
                {
                    _options!.TimerMonitorId = monitors[_commandLineService.TimerMonitorIndex - 1].MonitorId;
                    _options.MainMonitorIsWindowed = false;
                }

                IsCountdownMonitorSetByCommandLine =
                    _commandLineService.IsCountdownMonitorSpecified &&
                    _commandLineService.CountdownMonitorIndex <= monitors.Length;

                if (IsCountdownMonitorSetByCommandLine)
                {
                    _options!.CountdownMonitorId = monitors[_commandLineService.CountdownMonitorIndex - 1].MonitorId;
                    _options.CountdownMonitorIsWindowed = false;
                }
            }
        }

        private void ResetCircuitVisit()
        {
            // when the settings are read we ignore this saved setting 
            // and reset to false...
            _options!.IsCircuitVisit = false;
        }

        private void SetMidWeekOrWeekend()
        {
            // when the settings are read we ignore this saved setting 
            // and reset according to current day of week.
            _options!.MidWeekOrWeekend = IsNowWeekend()
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
                using StreamWriter file = File.CreateText(_optionsFilePath);

                JsonSerializer serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, _options);
                _originalOptionsSignature = GetOptionsSignature(_options);
            }
        }

        private void OnLogLevelChanged(object recipient, LogLevelChangedMessage message)
        {
            _logLevelSwitchService.SetMinimumLevel(Options.LogEventLevel);
        }
    }
}
