using ToastNotifications.Core;

namespace OnlyT.Services.OverrunNotificationService;

/// <summary>
/// Interaction logic for CustomOverrunDisplayPart.xaml
/// </summary>
public partial class CustomOverrunDisplayPart : NotificationDisplayPart
{
    public CustomOverrunDisplayPart(CustomOverrunNotification customNotification)
    {
        InitializeComponent();
        Bind(customNotification);
    }
}