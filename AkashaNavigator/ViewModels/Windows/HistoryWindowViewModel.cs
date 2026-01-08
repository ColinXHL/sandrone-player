using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkashaNavigator.Core.Interfaces;
using AkashaNavigator.Models.Data;

namespace AkashaNavigator.ViewModels.Windows
{
    /// <summary>
    /// 历史记录窗口 ViewModel
    /// 使用 CommunityToolkit.Mvvm 源生成器
    /// </summary>
    public partial class HistoryWindowViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        /// <summary>
        /// 历史记录列表
        /// </summary>
        public ObservableCollection<HistoryItem> HistoryItems { get; } = new();

        /// <summary>
        /// 搜索文本（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private string _searchText = string.Empty;

        /// <summary>
        /// 是否为空（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClearAllCommand))]
        private bool _isEmpty;

        /// <summary>
        /// 选择历史项事件（由 Code-behind 订阅以关闭窗口）
        /// </summary>
        public event EventHandler<HistoryItem?>? ItemSelected;

        public HistoryWindowViewModel(IDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            LoadHistory();
        }

        /// <summary>
        /// 搜索文本变化时重新加载（自动生成的方法）
        /// </summary>
        partial void OnSearchTextChanged(string value)
        {
            LoadHistory();
        }

        /// <summary>
        /// 加载历史记录
        /// </summary>
        public void LoadHistory()
        {
            var history = string.IsNullOrWhiteSpace(SearchText)
                ? _dataService.GetHistory()
                : _dataService.SearchHistory(SearchText);

            HistoryItems.Clear();
            foreach (var item in history)
            {
                HistoryItems.Add(item);
            }

            IsEmpty = HistoryItems.Count == 0;
        }

        /// <summary>
        /// 删除指定历史项（自动生成 DeleteCommand）
        /// </summary>
        [RelayCommand]
        private void Delete(int id)
        {
            _dataService.DeleteHistory(id);
            LoadHistory();
        }

        /// <summary>
        /// 清空全部历史（自动生成 ClearAllCommand）
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanClearAll))]
        private void ClearAll()
        {
            _dataService.ClearHistory();
            LoadHistory();
        }

        /// <summary>
        /// 是否可以清空（当列表不为空时）
        /// </summary>
        private bool CanClearAll() => !IsEmpty;

        /// <summary>
        /// 选择历史项（自动生成 SelectItemCommand）
        /// </summary>
        [RelayCommand]
        private void SelectItem(HistoryItem? item)
        {
            ItemSelected?.Invoke(this, item);
        }
    }
}
