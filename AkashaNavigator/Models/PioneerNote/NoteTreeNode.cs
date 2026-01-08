using System;
using System.Collections.ObjectModel;

namespace AkashaNavigator.Models.PioneerNote
{
    /// <summary>
    /// 笔记树节点模型
    /// </summary>
    public class NoteTreeNode
    {
        /// <summary>
        /// 节点 ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 标题/名称
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL（仅笔记项有）
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = "🔗";

        /// <summary>
        /// 是否为目录
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// 记录/创建时间
        /// </summary>
        public DateTime RecordedTime { get; set; }

        /// <summary>
        /// 所属目录 ID
        /// </summary>
        public string? FolderId { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public ObservableCollection<NoteTreeNode>? Children { get; set; }

        /// <summary>
        /// 格式化的时间显示
        /// </summary>
        public string FormattedTime => RecordedTime.ToString("MM/dd HH:mm");
    }
}
