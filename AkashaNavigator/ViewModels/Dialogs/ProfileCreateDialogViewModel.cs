using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkashaNavigator.Core.Interfaces;
using AkashaNavigator.Models.Plugin;
using AkashaNavigator.Services;

namespace AkashaNavigator.ViewModels.Dialogs
{
    /// <summary>
    /// Profile 创建对话框 ViewModel
    /// 使用 CommunityToolkit.Mvvm 源生成器
    /// </summary>
    public partial class ProfileCreateDialogViewModel : ObservableObject
    {
        private readonly IPluginLibrary _pluginLibrary;
        private readonly IProfileManager _profileManager;

        /// <summary>
        /// 可用图标列表
        /// </summary>
        public ObservableCollection<string> AvailableIcons { get; } = new();

        /// <summary>
        /// 插件选择项列表
        /// </summary>
        public ObservableCollection<PluginSelectorItem> PluginItems { get; } = new();

        /// <summary>
        /// Profile 名称（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
        private string _profileName = string.Empty;

        /// <summary>
        /// 选中的图标（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private string _selectedIcon = "📦";

        /// <summary>
        /// 错误消息（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// 是否显示错误消息
        /// </summary>
        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// 是否有已安装的插件
        /// </summary>
        public bool HasPlugins => PluginItems.Count > 0;

        /// <summary>
        /// 创建成功的 Profile ID
        /// </summary>
        public string? CreatedProfileId { get; private set; }

        /// <summary>
        /// 对话框结果
        /// </summary>
        public bool? DialogResult { get; private set; }

        /// <summary>
        /// 请求关闭事件
        /// </summary>
        public event EventHandler<bool?>? RequestClose;

        public ProfileCreateDialogViewModel(IPluginLibrary pluginLibrary, IProfileManager profileManager)
        {
            _pluginLibrary = pluginLibrary ?? throw new ArgumentNullException(nameof(pluginLibrary));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));

            LoadIcons();
            LoadPlugins();
        }

        /// <summary>
        /// 加载图标列表
        /// </summary>
        private void LoadIcons()
        {
            var icons = _profileManager.ProfileIcons;
            AvailableIcons.Clear();
            foreach (var icon in icons)
            {
                AvailableIcons.Add(icon);
            }

            if (AvailableIcons.Count > 0)
            {
                SelectedIcon = AvailableIcons[0];
            }
        }

        /// <summary>
        /// 加载已安装插件列表
        /// </summary>
        private void LoadPlugins()
        {
            var installedPlugins = _pluginLibrary.GetInstalledPlugins();
            var items = installedPlugins
                .Select(p => new PluginSelectorItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Version = p.Version,
                    Description = p.Description,
                    IsSelected = false
                })
                .ToList();

            // 监听选择变化
            foreach (var item in items)
            {
                item.PropertyChanged += OnPluginItemPropertyChanged;
            }

            PluginItems.Clear();
            foreach (var item in items)
            {
                PluginItems.Add(item);
            }

            OnPropertyChanged(nameof(HasPlugins));
        }

        private void OnPluginItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 插件选择变化时可以更新 UI（如果需要）
        }

        /// <summary>
        /// 创建 Profile（自动生成 CreateCommand）
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void Create()
        {
            // 验证输入
            if (!ValidateInput())
            {
                return;
            }

            // 获取输入值
            var profileName = ProfileName.Trim();
            var selectedPluginIds = PluginItems
                .Where(i => i.IsSelected)
                .Select(i => i.Id)
                .ToList();

            // 生成 Profile ID
            var generatedId = _profileManager.GenerateProfileId(profileName);

            // 检查 ID 是否已存在
            if (_profileManager.ProfileIdExists(generatedId))
            {
                SetError("已存在同名 Profile");
                return;
            }

            // 创建 Profile
            var result = _profileManager.CreateProfile(generatedId, profileName, SelectedIcon, selectedPluginIds);

            if (result.IsSuccess)
            {
                CreatedProfileId = result.ProfileId;
                DialogResult = true;
                RequestClose?.Invoke(this, true);
            }
            else
            {
                SetError(result.ErrorMessage ?? "创建失败");
            }
        }

        /// <summary>
        /// 是否可以创建（名称不为空）
        /// </summary>
        private bool CanCreate() => !string.IsNullOrWhiteSpace(ProfileName);

        /// <summary>
        /// 取消（自动生成 CancelCommand）
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            RequestClose?.Invoke(this, false);
        }

        /// <summary>
        /// 关闭窗口（自动生成 CloseCommand）
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            DialogResult = false;
            RequestClose?.Invoke(this, null);
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        private bool ValidateInput()
        {
            var name = ProfileName?.Trim();

            // 检查名称是否为空
            if (string.IsNullOrWhiteSpace(name))
            {
                SetError("Profile 名称不能为空");
                return false;
            }

            // 清除错误
            ClearError();
            return true;
        }

        /// <summary>
        /// 设置错误消息
        /// </summary>
        private void SetError(string message)
        {
            ErrorMessage = message;
            OnPropertyChanged(nameof(ShowError));
        }

        /// <summary>
        /// 清除错误消息
        /// </summary>
        private void ClearError()
        {
            ErrorMessage = null;
            OnPropertyChanged(nameof(ShowError));
        }

        /// <summary>
        /// Profile 名称变化时调用
        /// </summary>
        partial void OnProfileNameChanged(string value)
        {
            ClearError();
        }
    }
}
