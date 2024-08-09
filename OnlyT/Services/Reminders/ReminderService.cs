using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Common.Services.DateTime;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.ViewModel.Messages;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace OnlyT.Services.Reminders;

internal class ReminderService : IReminderService
{
    private const int DefaultIntervalBetweenRepeatedRemindersSeconds = 30;
    private const int LongIntervalBetweenRepeatedRemindersSeconds = 60;
    private const int NonAutoModeIntervalSeconds = 60; // between end of one talk and start of next

    private readonly IDateTimeService _dateTimeService;
    private readonly ITalkScheduleService _talkScheduleService;
    private readonly IOptionsService _optionsService;
    private readonly Notifier _notifier;
    private readonly DispatcherTimer _reminderTimer;
    private DateTime _lastTalkTimeStopped = DateTime.MaxValue;
    private TalkScheduleItem? _lastTalkStopped;
    private int? _nextTalkIdToStart;
    private DateTime _lastReminderShown = DateTime.MinValue;
    private int _secondsBetweenRepeatedReminders = DefaultIntervalBetweenRepeatedRemindersSeconds;

    public ReminderService(
        IDateTimeService dateTimeService, 
        ITalkScheduleService talkScheduleService, 
        IOptionsService optionsService)
    {
        _dateTimeService = dateTimeService;
        _talkScheduleService = talkScheduleService;
        _optionsService = optionsService;

        // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/Configuration.md

        _notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 20, 20);
            
            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(10),
                maximumNotificationCount: MaximumNotificationCount.FromCount(2));

            cfg.DisplayOptions.Width = 300;
            
            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        _reminderTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromSeconds(5)
        };

        _reminderTimer.Tick += ReminderTimerFire;

        WeakReferenceMessenger.Default.Register<TimerStartMessage>(this, OnTalkTimerStart);
        WeakReferenceMessenger.Default.Register<TimerStopMessage>(this, OnTalkTimerStop);
        WeakReferenceMessenger.Default.Register<CountdownWindowStatusChangedMessage>(this, OnCountdownStatusChanged);
    }

    public void Send(string msg)
    {
        _lastReminderShown = _dateTimeService.Now();

        if (_optionsService.Options.TimerReminder)
        {
            _notifier.ShowWarning(msg, new MessageOptions
            {
                ShowCloseButton = true,
                FreezeOnMouseEnter = false,
                FontSize = 14,
                CloseClickAction = OnReminderClosed,
            });
        }
    }

    public void Shutdown()
    {
        _reminderTimer.Stop();
        _reminderTimer.Tick -= ReminderTimerFire;

        WeakReferenceMessenger.Default.Unregister<TimerStartMessage>(this);
        WeakReferenceMessenger.Default.Unregister<TimerStopMessage>(this);
        WeakReferenceMessenger.Default.Unregister<StopCountDownMessage>(this);

        _notifier.Dispose();
    }

    private void SendTalkTimerReminder()
    {
        Send(Properties.Resources.TIMER_REMINDER_MSG);
    }

    private void OnTalkTimerStop(object recipient, TimerStopMessage message)
    {
        _lastTalkTimeStopped = _dateTimeService.Now();
        _lastTalkStopped = _talkScheduleService.GetTalkScheduleItem(message.TalkId);
        _nextTalkIdToStart = _talkScheduleService.GetNext(message.TalkId);

        _reminderTimer.Stop();
        _reminderTimer.Start();
        _secondsBetweenRepeatedReminders = DefaultIntervalBetweenRepeatedRemindersSeconds;
    }

    private void OnTalkTimerStart(object recipient, TimerStartMessage message)
    {
        _notifier.ClearMessages(new ClearAll());

        _reminderTimer.Stop();

        _lastTalkTimeStopped = DateTime.MaxValue;
        _lastTalkStopped = null;
        _nextTalkIdToStart = null;
        _lastReminderShown = DateTime.MinValue;
        _secondsBetweenRepeatedReminders = DefaultIntervalBetweenRepeatedRemindersSeconds;
    }

    private void ReminderTimerFire(object? sender, System.EventArgs e)
    {
        if (ShouldSendReminder())
        {
            SendTalkTimerReminder();
        }
    }

    private bool ShouldSendReminder()
    {
        return _reminderTimer.IsEnabled &&
               IsIntervalTooLong() &&
               !AlreadyRemindedRecently();
    }

    private bool AlreadyRemindedRecently()
    {
        return (_dateTimeService.Now() - _lastReminderShown).TotalSeconds < _secondsBetweenRepeatedReminders;
    }

    private bool IsIntervalTooLongInAutoMode()
    {
        if (_nextTalkIdToStart == null)
        {
            // there is no next talk!
            return false;
        }

        if (!Enum.IsDefined(typeof(TalkTypesAutoMode), _nextTalkIdToStart.Value))
        {
            // undefined talk
            return false;
        }

        if (_lastTalkStopped == null)
        {
            Debug.Assert(_nextTalkIdToStart != null);

            // this must be after the countdown ends and before the start of the
            // first talk timer . Allow time for intro, song and prayer
            return (_dateTimeService.Now() - _lastTalkTimeStopped).TotalSeconds > 360;
        }

        if (!Enum.IsDefined(typeof(TalkTypesAutoMode), _lastTalkStopped.Id) || 
            (TalkTypesAutoMode)_lastTalkStopped.Id == TalkTypesAutoMode.Unknown)
        {
            // undefined talk
            return false;
        }

        var triggerSeconds = 60;
        const int itemWithCounselSeconds = 110;
        const int itemWithoutCounselSeconds = 45;

        switch ((TalkTypesAutoMode)_lastTalkStopped.Id)
        {
            case TalkTypesAutoMode.ConcludingComments:
            case TalkTypesAutoMode.CircuitServiceTalk:
            case TalkTypesAutoMode.Watchtower:
                return false;

            case TalkTypesAutoMode.OpeningComments:
            case TalkTypesAutoMode.TreasuresTalk:
            case TalkTypesAutoMode.DiggingTalk:
                triggerSeconds = itemWithoutCounselSeconds;
                break;

            case TalkTypesAutoMode.Reading:
                triggerSeconds = itemWithCounselSeconds; // allow for counsel
                break;

            case TalkTypesAutoMode.MinistryItem1:
            case TalkTypesAutoMode.MinistryItem2:
            case TalkTypesAutoMode.MinistryItem3:
            case TalkTypesAutoMode.MinistryItem4:
                if ((TalkTypesAutoMode)_nextTalkIdToStart == TalkTypesAutoMode.LivingPart1)
                {
                    triggerSeconds = 300; // to include middle song
                }
                else
                {
                    triggerSeconds = _lastTalkStopped.IsStudentTalk
                        ? itemWithCounselSeconds
                        : itemWithoutCounselSeconds;
                }
                break;

            case TalkTypesAutoMode.LivingPart1:
            case TalkTypesAutoMode.LivingPart2:
            case TalkTypesAutoMode.CongBibleStudy:
                triggerSeconds = itemWithoutCounselSeconds;
                break;

            case TalkTypesAutoMode.PublicTalk:
                triggerSeconds = 300; // to include middle song
                break;

            default:
                // a talk we don't know about!
                break;
        }

        return (_dateTimeService.Now() - _lastTalkTimeStopped).TotalSeconds > triggerSeconds;
    }

    private bool IsIntervalTooLong()
    {
        return _optionsService.Options.OperatingMode == OperatingMode.Automatic
            ? IsIntervalTooLongInAutoMode()
            : (_dateTimeService.Now() - _lastTalkTimeStopped).TotalSeconds > NonAutoModeIntervalSeconds;
    }

    private void OnReminderClosed(NotificationBase obj)
    {
        // reminder manually closed so delay the display of the next reminder
        _secondsBetweenRepeatedReminders = LongIntervalBetweenRepeatedRemindersSeconds;
    }

    private void OnCountdownStatusChanged(object recipient, CountdownWindowStatusChangedMessage message)
    {
        if (!message.Showing)
        {
            _lastTalkTimeStopped = _dateTimeService.Now();
            _lastTalkStopped = null;
            _nextTalkIdToStart = _talkScheduleService.GetTalkScheduleItems().First().Id;

            _reminderTimer.Stop();
            _reminderTimer.Start();
            _secondsBetweenRepeatedReminders = DefaultIntervalBetweenRepeatedRemindersSeconds;
        }
    }
}