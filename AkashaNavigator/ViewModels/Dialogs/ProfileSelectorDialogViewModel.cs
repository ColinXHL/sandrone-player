using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkashaNavigator.Core.Interfaces;

namespace AkashaNavigator.ViewModels.Dialogs
{
    /// <summary>
    /// Profile选择项视图模型
    /// </summary>
    public partial class ProfileSelectionItem : ObservableObject
    {
        /// <summary>
        /// Profile ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Profile 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Profile 图标
        /// </summary>
        public string Icon { get; set; } = "📋";

        /// <summary>
        /// 是否已选中
        /// </summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// 是否已添加插件
        /// </summary>
        public bool AlreadyAdded { get; set; }

        /// <summary>
        /// 是否可以选择
        /// </summary>
        public bool CanSelect { get; set; } = true;

        /// <summary>
        /// 已添加提示的可见性
        /// </summary>
        public Visibility AlreadyAddedVisibility => AlreadyAdded ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Profile选择器对话框 ViewModel
    /// 使用 CommunityToolkit.Mvvm 源生成器
    /// </summary>
    public partial class ProfileSelectorDialogViewModel : ObservableObject
    {
        private readonly IProfileManager _profileManager;
        private readonly IPluginAssociationManager _pluginAssociationManager;
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService;
        private string _pluginId = string.Empty;

        /// <summary>
        /// Profile 列表
        /// </summary>
        public ObservableCollection<ProfileSelectionItem> Profiles { get; } = new();

        /// <summary>
        /// 选择计数文本
        /// </summary>
        [ObservableProperty]
        private string _selectionCountText = "已选择 0 个 Profile";

        /// <summary>
        /// 是否可以确认（至少选择了一个 Profile）
        /// </summary>
        [ObservableProperty]
        private bool _canConfirm;

        /// <summary>
        /// 请求关闭对话框事件
        /// </summary>
        public event EventHandler<bool?>? RequestClose;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProfileSelectorDialogViewModel(
            IProfileManager profileManager,
            IPluginAssociationManager pluginAssociationManager,
            INotificationService notificationService,
            ILogService logService)
        {
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            _pluginAssociationManager = pluginAssociationManager ?? throw new ArgumentNullException(nameof(pluginAssociationManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// 初始化 ViewModel（设置插件ID并加载Profile列表）
        /// </summary>
        public void Initialize(string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId))
                throw new ArgumentException("Plugin ID cannot be empty", nameof(pluginId));

            _pluginId = pluginId;
            LoadProfiles();
        }

        /// <summary>
        /// 加载 Profile 列表
        /// </summary>
        private void LoadProfiles()
        {
            var allProfiles = _profileManager.InstalledProfiles;
            var profilesWithPlugin = _pluginAssociationManager.GetProfilesUsingPlugin(_pluginId);
            var profilesWithPluginSet = profilesWithPlugin.ToHashSet();

            var profileItems = allProfiles
                .Select(p => new ProfileSelectionItem
                {
                    Id = p.Id,
                    Name = p.Name ?? p.Id,
                    Icon = p.Icon ?? "📋",
                    IsSelected = false,
                    AlreadyAdded = profilesWithPluginSet.Contains(p.Id),
                    CanSelect = !profilesWithPluginSet.Contains(p.Id)
                })
                .ToList();

            Profiles.Clear();
            foreach (var item in profileItems)
            {
                Profiles.Add(item);
            }

            // 订阅每个 ProfileSelectionItem 的属性变化
            foreach (var item in Profiles)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ProfileSelectionItem.IsSelected))
                    {
                        UpdateSelectionCount();
                    }
                };
            }

            UpdateSelectionCount();
        }

        /// <summary>
        /// 更新选择计数
        /// </summary>
        private void UpdateSelectionCount()
        {
            var selectedCount = Profiles.Count(p => p.IsSelected);
            SelectionCountText = $"已选择 {selectedCount} 个 Profile";
            CanConfirm = selectedCount > 0;
        }

        /// <summary>
        /// 确认命令
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            var selectedProfiles = Profiles.Where(p => p.IsSelected).Select(p => p.Id).ToList();

            if (selectedProfiles.Count == 0)
            {
                _notificationService.Warning("请至少选择一个 Profile", "提示");
                return;
            }

            // 添加插件到选定的Profile
            int successCount = 0;
            foreach (var profileId in selectedProfiles)
            {
                try
                {
                    _pluginAssociationManager.AddPluginToProfile(_pluginId, profileId);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logService.Error(nameof(ProfileSelectorDialogViewModel),
                        "添加插件到 Profile {ProfileId} 失败: {ErrorMessage}", profileId, ex.Message);
                }
            }

            if (successCount > 0)
            {
                _notificationService.Success($"已成功将插件添加到 {successCount} 个 Profile", "添加成功");
                RequestClose?.Invoke(this, true);
            }
            else
            {
                _notificationService.Error("添加失败，请查看日志了解详情", "添加失败");
                RequestClose?.Invoke(this, false);
            }
        }

        /// <summary>
        /// 取消命令
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        /// <summary>
        /// 关闭命令
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, false);
        }
    }
}
