using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AkashaNavigator.Helpers;
using AkashaNavigator.Models.Config;
using AkashaNavigator.Models.Profile;
using AkashaNavigator.ViewModels.Windows;
using AkashaNavigator.Core.Interfaces;
using HotkeyModifierKeys = AkashaNavigator.Models.Config.ModifierKeys;

namespace AkashaNavigator.Views.Windows
{
/// <summary>
/// SettingsWindow - 设置窗口（混合架构）
/// </summary>
public partial class SettingsWindow : AnimatedWindow
{
#region Fields

    private readonly SettingsViewModel _viewModel;
    private readonly INotificationService _notificationService;
    private bool _isInitializing = true;
    private TextBox? _currentHotkeyTextBox;
    private readonly Dictionary<TextBox, string> _hotkeyTextBoxToKeyMap = new();
    private ImeHelper.ImeState _savedImeState;

#endregion

#region Constructor

    public SettingsWindow(
        SettingsViewModel viewModel,
        INotificationService notificationService)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        InitializeComponent();
        DataContext = _viewModel;

        // 订阅 ViewModel 事件
        _viewModel.OpenConfigFolderRequested += OnOpenConfigFolder;

        // 初始化快捷键映射
        InitHotkeyMapping();

        // 加载 Profile 列表
        _viewModel.LoadProfileList();

        _isInitializing = false;
    }

#endregion

#region Private Methods

    /// <summary>
    /// 初始化快捷键输入框映射
    /// </summary>
    private void InitHotkeyMapping()
    {
        _hotkeyTextBoxToKeyMap[HotkeySeekBackward] = "SeekBackward";
        _hotkeyTextBoxToKeyMap[HotkeySeekForward] = "SeekForward";
        _hotkeyTextBoxToKeyMap[HotkeyTogglePlay] = "TogglePlay";
        _hotkeyTextBoxToKeyMap[HotkeyDecreaseOpacity] = "DecreaseOpacity";
        _hotkeyTextBoxToKeyMap[HotkeyIncreaseOpacity] = "IncreaseOpacity";
        _hotkeyTextBoxToKeyMap[HotkeyToggleClickThrough] = "ToggleClickThrough";
        _hotkeyTextBoxToKeyMap[HotkeyToggleMaximize] = "ToggleMaximize";

        // 加载快捷键显示
        foreach (var (textBox, key) in _hotkeyTextBoxToKeyMap)
        {
            var vkCode = _viewModel.HotkeyValues[key];
            var modifiers = _viewModel.HotkeyModifiers[key];
            textBox.Text = Win32Helper.GetHotkeyDisplayName(vkCode, modifiers);
        }
    }

    /// <summary>
    /// 获取快捷键显示名称
    /// </summary>
    private string GetHotkeyDisplayName(TextBox textBox)
    {
        if (_hotkeyTextBoxToKeyMap.TryGetValue(textBox, out var key))
        {
            var vkCode = _viewModel.HotkeyValues[key];
            var modifiers = _viewModel.HotkeyModifiers[key];
            return Win32Helper.GetHotkeyDisplayName(vkCode, modifiers);
        }
        return string.Empty;
    }

#endregion

#region Event Handlers

    /// <summary>
    /// 标题栏拖动
    /// </summary>
    private new void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        base.TitleBar_MouseLeftButtonDown(sender, e);
    }

    /// <summary>
    /// 快进秒数滑块值变化（保留在 Code-behind 用于显示更新）
    /// </summary>
    private void SeekSecondsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing)
            return;
        SeekSecondsValue.Text = $"{(int)e.NewValue}s";
    }

    /// <summary>
    /// 透明度滑块值变化（保留在 Code-behind 用于显示更新）
    /// </summary>
    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing)
            return;
        OpacityValue.Text = $"{(int)e.NewValue}%";
    }

    /// <summary>
    /// 吸附阈值滑块值变化（保留在 Code-behind 用于显示更新）
    /// </summary>
    private void SnapThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing)
            return;
        SnapThresholdValue.Text = $"{(int)e.NewValue}px";
    }

    /// <summary>
    /// 打开配置文件夹（通过 ViewModel 事件处理）
    /// </summary>
    private void OnOpenConfigFolder(object? sender, string path)
    {
        if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
    }

    /// <summary>
    /// 打开配置文件夹按钮点击（备用方案）
    /// </summary>
    private void BtnOpenConfigFolder_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenConfigFolderCommand.Execute(null);
    }

    /// <summary>
    /// 快捷键输入框获得焦点
    /// </summary>
    private void Hotkey_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            _currentHotkeyTextBox = textBox;
            textBox.Text = "按下新快捷键...";

            // 切换到英文输入模式
            _savedImeState = ImeHelper.SwitchToEnglish(this);
        }
    }

    /// <summary>
    /// 快捷键输入框失去焦点
    /// </summary>
    private void Hotkey_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // 恢复显示
            textBox.Text = GetHotkeyDisplayName(textBox);
        }
        _currentHotkeyTextBox = null;

        // 恢复之前的输入法状态
        ImeHelper.RestoreImeState(_savedImeState);
    }

    /// <summary>
    /// 快捷键按键捕获
    /// </summary>
    private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;

        Key targetKey = e.Key;
        bool isSystemKey = e.Key == Key.System;

        if (isSystemKey)
        {
            // Alt+任意键时，取实际按键（e.SystemKey）
            targetKey = e.SystemKey;
            // 排除系统级快捷键（Alt+Tab/Alt+Esc 等，避免干扰系统）
            if (targetKey == Key.Tab || targetKey == Key.Escape)
            {
                e.Handled = false;
                return;
            }
        }

        // 忽略修饰键本身
        if (targetKey == Key.LeftCtrl || targetKey == Key.RightCtrl || targetKey == Key.LeftAlt ||
            targetKey == Key.RightAlt || targetKey == Key.LeftShift || targetKey == Key.RightShift ||
            targetKey == Key.LWin || targetKey == Key.RWin)
        {
            e.Handled = true;
            return;
        }

        // 获取虚拟键码
        var vkCode = (uint)KeyInterop.VirtualKeyFromKey(targetKey);

        // 获取当前修饰键状态
        var modifiers = HotkeyModifierKeys.None;
        if (Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
            modifiers |= HotkeyModifierKeys.Ctrl;
        if (isSystemKey || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= HotkeyModifierKeys.Alt;
        if (Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            modifiers |= HotkeyModifierKeys.Shift;

        e.Handled = true;

        // 更新显示和存储
        textBox.Text = Win32Helper.GetHotkeyDisplayName(vkCode, modifiers);

        if (_hotkeyTextBoxToKeyMap.TryGetValue(textBox, out var key))
        {
            _viewModel.HotkeyValues[key] = vkCode;
            _viewModel.HotkeyModifiers[key] = modifiers;
        }

        // 移动焦点到下一个控件
        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    /// <summary>
    /// 取消按钮
    /// </summary>
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
    }

    /// <summary>
    /// 保存按钮（调用 ViewModel 命令后关闭窗口）
    /// </summary>
    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCommand.Execute(null);
        CloseWithAnimation();
    }

    /// <summary>
    /// Profile 选择变化（ViewModel 已通过绑定处理，此方法保留用于扩展）
    /// </summary>
    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        // 绑定已处理 Profile 切换，此方法保留用于扩展
    }

#endregion
}
}
