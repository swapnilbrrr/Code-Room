namespace CodeRoom.Web.ViewModels.Notifications;

public class NotificationItemViewModel
{
    public int? NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsAnnouncement { get; set; }
}

public class NotificationBellViewModel
{
    public int UnreadCount { get; set; }
    public IReadOnlyList<NotificationItemViewModel> Items { get; set; } = [];
}

public class NotificationsPageViewModel
{
    public int UnreadCount { get; set; }
    public IReadOnlyList<NotificationItemViewModel> Notifications { get; set; } = [];
}
