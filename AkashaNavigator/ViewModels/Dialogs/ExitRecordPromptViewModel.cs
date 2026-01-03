using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AkashaNavigator.ViewModels.Dialogs
{
    /// <summary>
    /// 用户操作结果
    /// </summary>
    public enum PromptResult
    {
        /// <summary>
        /// 取消操作，不做任何事情
        /// </summary>
        Cancel,

        /// <summary>
        /// 继续退出应用
        /// </summary>
        Exit,

        /// <summary>
        /// 打开开荒笔记窗口
        /// </summary>
        OpenPioneerNotes,

        /// <summary>
        /// 打开快速记录对话框
        /// </summary>
        QuickRecord
    }

    /// <summary>
    /// 退出记录提示对话框 ViewModel
    /// 使用 CommunityToolkit.Mvvm 源生成器
    /// </summary>
    public partial class ExitRecordPromptViewModel : ObservableObject
    {
        /// <summary>
        /// 页面标题
        /// </summary>
        [ObservableProperty]
        private string _pageTitle = string.Empty;

        /// <summary>
        /// 页面 URL
        /// </summary>
        [ObservableProperty]
        private string _pageUrl = string.Empty;

        /// <summary>
        /// 用户选择的操作结果
        /// </summary>
        public PromptResult Result { get; private set; } = PromptResult.Cancel;

        /// <summary>
        /// 请求关闭对话框事件
        /// </summary>
        public event EventHandler? RequestClose;

        /// <summary>
        /// 初始化 ViewModel（设置页面数据）
        /// </summary>
        public void Initialize(string url, string title)
        {
            PageUrl = string.IsNullOrWhiteSpace(url) ? "(无 URL)" : url;
            PageTitle = string.IsNullOrWhiteSpace(title) ? "(无标题)" : title;
        }

        /// <summary>
        /// 打开开荒笔记命令
        /// </summary>
        [RelayCommand]
        private void OpenPioneerNotes()
        {
            Result = PromptResult.OpenPioneerNotes;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 快速记录命令
        /// </summary>
        [RelayCommand]
        private void QuickRecord()
        {
            Result = PromptResult.QuickRecord;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 直接退出命令
        /// </summary>
        [RelayCommand]
        private void DirectExit()
        {
            Result = PromptResult.Exit;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 关闭命令（取消操作）
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            Result = PromptResult.Cancel;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
