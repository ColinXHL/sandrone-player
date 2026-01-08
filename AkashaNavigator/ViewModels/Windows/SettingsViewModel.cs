using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkashaNavigator.Core.Events;
using AkashaNavigator.Core.Events.Events;
using AkashaNavigator.Core.Interfaces;
using AkashaNavigator.Models.Config;
using AkashaNavigator.Models.Profile;
using HotkeyModifierKeys = AkashaNavigator.Models.Config.ModifierKeys;

namespace AkashaNavigator.ViewModels.Windows
{
    /// <summary>
    /// 设置窗口 ViewModel - 混合架构
    /// 负责配置状态和业务逻辑，快捷键输入等 UI 交互保留在 Code-behind
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IConfigService _configService;
        private readonly IProfileManager _profileManager;
        private readonly IEventBus _eventBus;
        private AppConfig _config;

        /// <summary>
        /// 可用 Profile 列表
        /// </summary>
        public ObservableCollection<GameProfile> Profiles { get; } = new();

        /// <summary>
        /// 快进秒数（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private int _seekSeconds;

        /// <summary>
        /// 透明度百分比（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private int _opacityPercent;

        /// <summary>
        /// 是否启用边缘吸附（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private bool _enableEdgeSnap;

        /// <summary>
        /// 吸附阈值（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private int _snapThreshold;

        /// <summary>
        /// 是否在退出时提示记录笔记（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private bool _promptRecordOnExit;

        /// <summary>
        /// 是否启用插件更新通知（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private bool _enablePluginUpdateNotification;

        /// <summary>
        /// 是否启用调试日志（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private bool _enableDebugLog;

        /// <summary>
        /// 当前选中的 Profile（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UnsubscribeProfileCommand))]
        private GameProfile? _selectedProfile;

        /// <summary>
        /// 快捷键值存储（供 Code-behind 访问）
        /// </summary>
        public Dictionary<string, uint> HotkeyValues { get; } = new();

        /// <summary>
        /// 快捷键修饰键存储（供 Code-behind 访问）
        /// </summary>
        public Dictionary<string, HotkeyModifierKeys> HotkeyModifiers { get; } = new();

        public SettingsViewModel(
            IConfigService configService,
            IProfileManager profileManager,
            IEventBus eventBus)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _config = _configService.Config;
            LoadSettings();
            LoadHotkeys();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            SeekSeconds = _config.SeekSeconds;
            OpacityPercent = (int)(_config.DefaultOpacity * 100);
            EnableEdgeSnap = _config.EnableEdgeSnap;
            SnapThreshold = _config.SnapThreshold;
            PromptRecordOnExit = _config.PromptRecordOnExit;
            EnablePluginUpdateNotification = _config.EnablePluginUpdateNotification;
            EnableDebugLog = _config.EnableDebugLog;
        }

        /// <summary>
        /// 加载快捷键
        /// </summary>
        private void LoadHotkeys()
        {
            HotkeyValues["SeekBackward"] = _config.HotkeySeekBackward;
            HotkeyValues["SeekForward"] = _config.HotkeySeekForward;
            HotkeyValues["TogglePlay"] = _config.HotkeyTogglePlay;
            HotkeyValues["DecreaseOpacity"] = _config.HotkeyDecreaseOpacity;
            HotkeyValues["IncreaseOpacity"] = _config.HotkeyIncreaseOpacity;
            HotkeyValues["ToggleClickThrough"] = _config.HotkeyToggleClickThrough;
            HotkeyValues["ToggleMaximize"] = _config.HotkeyToggleMaximize;

            HotkeyModifiers["SeekBackward"] = _config.HotkeySeekBackwardMod;
            HotkeyModifiers["SeekForward"] = _config.HotkeySeekForwardMod;
            HotkeyModifiers["TogglePlay"] = _config.HotkeyTogglePlayMod;
            HotkeyModifiers["DecreaseOpacity"] = _config.HotkeyDecreaseOpacityMod;
            HotkeyModifiers["IncreaseOpacity"] = _config.HotkeyIncreaseOpacityMod;
            HotkeyModifiers["ToggleClickThrough"] = _config.HotkeyToggleClickThroughMod;
            HotkeyModifiers["ToggleMaximize"] = _config.HotkeyToggleMaximizeMod;
        }

        /// <summary>
        /// 保存设置（自动生成 SaveCommand）
        /// </summary>
        [RelayCommand]
        private void Save()
        {
            _config.SeekSeconds = SeekSeconds;
            _config.DefaultOpacity = OpacityPercent / 100.0;
            _config.EnableEdgeSnap = EnableEdgeSnap;
            _config.SnapThreshold = SnapThreshold;
            _config.PromptRecordOnExit = PromptRecordOnExit;
            _config.EnablePluginUpdateNotification = EnablePluginUpdateNotification;
            _config.EnableDebugLog = EnableDebugLog;

            // 保存快捷键
            _config.HotkeySeekBackward = HotkeyValues["SeekBackward"];
            _config.HotkeySeekBackwardMod = HotkeyModifiers["SeekBackward"];
            _config.HotkeySeekForward = HotkeyValues["SeekForward"];
            _config.HotkeySeekForwardMod = HotkeyModifiers["SeekForward"];
            _config.HotkeyTogglePlay = HotkeyValues["TogglePlay"];
            _config.HotkeyTogglePlayMod = HotkeyModifiers["TogglePlay"];
            _config.HotkeyDecreaseOpacity = HotkeyValues["DecreaseOpacity"];
            _config.HotkeyDecreaseOpacityMod = HotkeyModifiers["DecreaseOpacity"];
            _config.HotkeyIncreaseOpacity = HotkeyValues["IncreaseOpacity"];
            _config.HotkeyIncreaseOpacityMod = HotkeyModifiers["IncreaseOpacity"];
            _config.HotkeyToggleClickThrough = HotkeyValues["ToggleClickThrough"];
            _config.HotkeyToggleClickThroughMod = HotkeyModifiers["ToggleClickThrough"];
            _config.HotkeyToggleMaximize = HotkeyValues["ToggleMaximize"];
            _config.HotkeyToggleMaximizeMod = HotkeyModifiers["ToggleMaximize"];

            _configService.UpdateConfig(_config);
        }

        /// <summary>
        /// 重置为默认设置（自动生成 ResetCommand）
        /// </summary>
        [RelayCommand]
        private void Reset()
        {
            var defaultConfig = new AppConfig();
            LoadFromConfig(defaultConfig);
        }

        /// <summary>
        /// 从配置加载设置
        /// </summary>
        private void LoadFromConfig(AppConfig config)
        {
            SeekSeconds = AppConstants.DefaultSeekSeconds;
            OpacityPercent = (int)(AppConstants.MaxOpacity * 100);
            EnableEdgeSnap = true;
            SnapThreshold = AppConstants.SnapThreshold;
            PromptRecordOnExit = false;
            EnablePluginUpdateNotification = true;
            EnableDebugLog = false;

            // 重置快捷键
            HotkeyValues["SeekBackward"] = config.HotkeySeekBackward;
            HotkeyModifiers["SeekBackward"] = config.HotkeySeekBackwardMod;
            HotkeyValues["SeekForward"] = config.HotkeySeekForward;
            HotkeyModifiers["SeekForward"] = config.HotkeySeekForwardMod;
            HotkeyValues["TogglePlay"] = config.HotkeyTogglePlay;
            HotkeyModifiers["TogglePlay"] = config.HotkeyTogglePlayMod;
            HotkeyValues["DecreaseOpacity"] = config.HotkeyDecreaseOpacity;
            HotkeyModifiers["DecreaseOpacity"] = config.HotkeyDecreaseOpacityMod;
            HotkeyValues["IncreaseOpacity"] = config.HotkeyIncreaseOpacity;
            HotkeyModifiers["IncreaseOpacity"] = config.HotkeyIncreaseOpacityMod;
            HotkeyValues["ToggleClickThrough"] = config.HotkeyToggleClickThrough;
            HotkeyModifiers["ToggleClickThrough"] = config.HotkeyToggleClickThroughMod;
            HotkeyValues["ToggleMaximize"] = config.HotkeyToggleMaximize;
            HotkeyModifiers["ToggleMaximize"] = config.HotkeyToggleMaximizeMod;
        }

        /// <summary>
        /// 打开插件中心（自动生成 OpenPluginCenterCommand）
        /// </summary>
        [RelayCommand]
        private void OpenPluginCenter()
        {
            // 通过 EventBus 发布请求（解耦 PluginCenterWindow）
            _eventBus.Publish(new PluginCenterRequestedEvent());
        }

        /// <summary>
        /// 加载 Profile 列表
        /// </summary>
        public void LoadProfileList()
        {
            var profiles = _profileManager.InstalledProfiles;
            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            // 选中当前 Profile
            var currentProfile = _profileManager.CurrentProfile;
            for (int i = 0; i < Profiles.Count; i++)
            {
                if (Profiles[i].Id.Equals(currentProfile.Id, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedProfile = Profiles[i];
                    break;
                }
            }
        }

        /// <summary>
        /// Profile 选择变化（自动生成的方法）
        /// </summary>
        partial void OnSelectedProfileChanged(GameProfile? value)
        {
            if (value != null)
            {
                var currentProfile = _profileManager.CurrentProfile;
                if (!value.Id.Equals(currentProfile.Id, StringComparison.OrdinalIgnoreCase))
                {
                    _profileManager.SwitchProfile(value.Id);
                }
            }
        }

        /// <summary>
        /// 取消订阅 Profile（自动生成 UnsubscribeProfileCommand）
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanUnsubscribeProfile))]
        private void UnsubscribeProfile()
        {
            if (SelectedProfile == null)
                return;

            if (SelectedProfile.Id.Equals("default", StringComparison.OrdinalIgnoreCase))
                return;  // 不能删除默认 Profile

            var result = _profileManager.UnsubscribeProfile(SelectedProfile.Id);
            if (result.IsSuccess)
            {
                LoadProfileList();
            }
        }

        /// <summary>
        /// 是否可以取消订阅
        /// </summary>
        private bool CanUnsubscribeProfile() =>
            SelectedProfile != null &&
            !SelectedProfile.Id.Equals("default", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 打开配置文件夹（自动生成 OpenConfigFolderCommand）
        /// </summary>
        [RelayCommand]
        private void OpenConfigFolder()
        {
            // 通过事件通知 Code-behind，传递路径参数
            var path = _profileManager.DataDirectory;
            OpenConfigFolderRequested?.Invoke(this, path);
        }

        /// <summary>
        /// 请求打开配置文件夹事件（参数：配置目录路径）
        /// </summary>
        public event EventHandler<string>? OpenConfigFolderRequested;

        /// <summary>
        /// 插件中心关闭后请求刷新 Profile 列表
        /// </summary>
        public void RefreshProfileList()
        {
            _profileManager.ReloadProfiles();
            LoadProfileList();
        }
    }
}
