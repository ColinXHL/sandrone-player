namespace AkashaNavigator.Core.Events.Events
{
    /// <summary>
    /// 记录笔记请求事件
    /// </summary>
    public class RecordNoteRequestedEvent
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}

namespace AkashaNavigator.Core.Events.Events
{
    /// <summary>
    /// 应用退出事件
    /// </summary>
    public class ApplicationExitEvent
    {
    }
}
