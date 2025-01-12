using System.ComponentModel;
using System.Runtime.CompilerServices;
using ToastNotifications.Core;

namespace OnlyT.Services.OverrunNotificationService;

// https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/CustomNotificatios.md

public sealed class CustomOverrunNotification : NotificationBase, INotifyPropertyChanged
{
    private string? _body;
    private bool _isOverrun;
    private int _mins;

    private CustomOverrunDisplayPart? _displayPart;

    public override NotificationDisplayPart DisplayPart => _displayPart ??= new CustomOverrunDisplayPart(this);

    public event PropertyChangedEventHandler? PropertyChanged;

    public CustomOverrunNotification(string body, bool isOverrun, int mins, MessageOptions? messageOptions) 
        : base(body, messageOptions)
    {
        IsOverrun = isOverrun;
        Body = body;
        Mins = mins;
    }

    public int Mins
    {
        get => _mins;
        set
        {
            _mins = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsOverrun
    {
        get => _isOverrun;
        set
        {
            _isOverrun = value;
            OnPropertyChanged();
        }
    }
    
    public new string Body
    {
        get => _body ?? string.Empty;
        set
        {
            _body = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}