using System.IO;
using System.Windows;
using System.Windows.Controls;
using FloatWebPlayer.Models;
using FloatWebPlayer.Services;

namespace FloatWebPlayer.Views
{
    /// <summary>
    /// 已安装插件页面 - 显示全局插件库中的所有插件
    /// </summary>
    public partial class InstalledPluginsPage : UserControl
    {
        public InstalledPluginsPage()
        {
            InitializeComponent();
            Loaded += InstalledPluginsPage_Loaded;
        }

        private void InstalledPluginsPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPluginList();
        }

        /// <summary>
        /// 刷新插件列表
        /// </summary>
        public void RefreshPluginList()
        {
            var plugins = PluginLibrary.Instance.GetInstalledPlugins();
            var searchText = SearchBox?.Text?.ToLower() ?? "";

            // 过滤搜索
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                plugins = plugins.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    (p.Description?.ToLower().Contains(searchText) ?? false)
                ).ToList();
            }

            // 转换为视图模型
            var viewModels = plugins.Select(p => new InstalledPluginViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                Description = p.Description,
                Author = p.Author,
                ReferenceCount = PluginAssociationManager.Instance.GetPluginReferenceCount(p.Id),
                ProfilesText = GetProfilesText(p.Id),
                HasDescription = !string.IsNullOrWhiteSpace(p.Description),
                HasSettingsUi = HasSettingsUi(p.Id)
            }).ToList();

            PluginList.ItemsSource = viewModels;
            PluginCountText.Text = $"共 {viewModels.Count} 个插件";
            NoPluginsText.Visibility = viewModels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 获取插件关联的Profile文本
        /// </summary>
        private string GetProfilesText(string pluginId)
        {
            var profiles = PluginAssociationManager.Instance.GetProfilesUsingPlugin(pluginId);
            if (profiles.Count == 0)
                return "无";
            if (profiles.Count <= 3)
                return string.Join(", ", profiles);
            return $"{string.Join(", ", profiles.Take(3))} 等 {profiles.Count} 个";
        }

        /// <summary>
        /// 检查插件是否有设置UI
        /// </summary>
        private bool HasSettingsUi(string pluginId)
        {
            var pluginDir = PluginLibrary.Instance.GetPluginDirectory(pluginId);
            var settingsUiPath = Path.Combine(pluginDir, "settings_ui.json");
            return File.Exists(settingsUiPath);
        }

        /// <summary>
        /// 搜索框文本变化
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshPluginList();
        }

        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string pluginId)
            {
                // TODO: 打开插件设置对话框
                MessageBox.Show($"插件 {pluginId} 的设置功能正在开发中", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 添加到Profile按钮点击
        /// </summary>
        private void BtnAddToProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string pluginId)
            {
                var dialog = new ProfileSelectorDialog(pluginId);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    RefreshPluginList();
                }
            }
        }

        /// <summary>
        /// 卸载按钮点击
        /// </summary>
        private void BtnUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string pluginId)
            {
                var profiles = PluginAssociationManager.Instance.GetProfilesUsingPlugin(pluginId);
                
                string message;
                if (profiles.Count > 0)
                {
                    message = $"该插件被以下 Profile 引用：\n\n• {string.Join("\n• ", profiles)}\n\n卸载后，这些 Profile 中将移除该插件的引用。\n\n确定要卸载吗？";
                }
                else
                {
                    message = $"确定要卸载插件 \"{pluginId}\" 吗？";
                }

                var result = MessageBox.Show(message, "确认卸载", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    var uninstallResult = PluginLibrary.Instance.UninstallPlugin(pluginId, force: true);
                    if (uninstallResult.IsSuccess)
                    {
                        RefreshPluginList();
                    }
                    else
                    {
                        MessageBox.Show($"卸载失败: {uninstallResult.ErrorMessage}", "错误",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 已安装插件视图模型
    /// </summary>
    public class InstalledPluginViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Author { get; set; }
        public int ReferenceCount { get; set; }
        public string ProfilesText { get; set; } = "无";
        public bool HasDescription { get; set; }
        public bool HasSettingsUi { get; set; }
        public Visibility HasDescriptionVisibility => HasDescription ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HasSettingsUiVisibility => HasSettingsUi ? Visibility.Visible : Visibility.Collapsed;
    }
}
