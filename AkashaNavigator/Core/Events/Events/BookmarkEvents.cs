namespace AkashaNavigator.Core.Events.Events
{
    /// <summary>
    /// 收藏请求事件
    /// </summary>
    public class BookmarkRequestedEvent
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// 收藏状态变化事件
    /// </summary>
    public class BookmarkStateChangedEvent
    {
        public bool IsBookmarked { get; set; }
    }

    /// <summary>
    /// 历史记录菜单请求事件
    /// </summary>
    public class HistoryRequestedEvent
    {
    }

    /// <summary>
    /// 历史记录项选中事件
    /// </summary>
    public class HistoryItemSelectedEvent
    {
        public string Url { get; set; } = string.Empty;
    }
}
