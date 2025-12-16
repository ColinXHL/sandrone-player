using System.IO;
using System.Windows;
using System.Windows.Controls;
using FloatWebPlayer.Helpers;
using FloatWebPlayer.Models;
using FloatWebPlayer.Services;

namespace FloatWebPlayer.Views
{
    /// <summary>
    /// 可用插件页面 - 显示内置但未安装的插件
    /// </summary>
    public partial class AvailablePluginsPage : UserControl
    {
        public AvailablePluginsPage()
        {
            InitializeComponent();
            Loaded += AvailablePluginsPage_Loaded;
        }

        private void AvailablePluginsPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPluginList();
        }

        /// <summary>
        /// 刷新插件列表
        /// </summary>
        public void RefreshPluginList()
        {
            var availablePlugins = GetAvailablePlugins();
            var searchText = SearchBox?.Text?.ToLower() ?? "";

            // 过滤搜索
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                availablePlugins = availablePlugins.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    (p.Description?.ToLower().Contains(searchText) ?? false)
                ).ToList();
            }

            PluginList.ItemsSource = availablePlugins;
            PluginCountText.Text = $"共 {availablePlugins.Count} 个可用插件";
            NoPluginsText.Visibility = availablePlugins.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 获取可用（未安装）的插件列表
        /// </summary>
        private List<AvailablePluginViewModel> GetAvailablePlugins()
        {
            var result = new List<AvailablePluginViewModel>();
            var installedIds = PluginLibrary.Instance.GetInstalledPlugins()
                .Select(p => p.Id)
                .ToHashSet();

            // 扫描内置插件目录
            var builtinPluginsDir = AppPaths.BuiltInPluginsDirectory;
            if (!Directory.Exists(builtinPluginsDir))
                return result;

            foreach (var pluginDir in Directory.GetDirectories(builtinPluginsDir))
            {
                var manifestPath = Path.Combine(pluginDir, "plugin.json");
                if (!File.Exists(manifestPath))
                    continue;

                try
                {
                    var manifest = JsonHelper.LoadFromFile<PluginManifest>(manifestPath);
                    if (manifest == null || string.IsNullOrEmpty(manifest.Id))
                        continue;

                    // 跳过已安装的插件
                    if (installedIds.Contains(manifest.Id))
                        continue;

                    result.Add(new AvailablePluginViewModel
                    {
                        Id = manifest.Id,
                        Name = manifest.Name ?? manifest.Id,
                        Version = manifest.Version ?? "1.0.0",
                        Description = manifest.Description,
                        Author = manifest.Author,
                        SourceDirectory = pluginDir,
                        HasDescription = !string.IsNullOrWhiteSpace(manifest.Description),
                        HasAuthor = !string.IsNullOrWhiteSpace(manifest.Author)
                    });
                }
                catch
                {
                    // 忽略无效的插件清单
                }
            }

            return result;
        }

        /// <summary>
        /// 搜索框文本变化
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshPluginList();
        }

        /// <summary>
        /// 安装按钮点击
        /// </summary>
        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string pluginId)
            {
                // 获取插件源目录
                var viewModel = (PluginList.ItemsSource as List<AvailablePluginViewModel>)?
                    .FirstOrDefault(p => p.Id == pluginId);
                
                if (viewModel == null)
                    return;

                var result = PluginLibrary.Instance.InstallPlugin(pluginId, viewModel.SourceDirectory);
                if (result.IsSuccess)
                {
                    MessageBox.Show($"插件 \"{viewModel.Name}\" 安装成功！", "安装成功",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshPluginList();
                    
                    // 通知父窗口刷新
                    if (Window.GetWindow(this) is PluginCenterWindow centerWindow)
                    {
                        centerWindow.RefreshCurrentPage();
                    }
                }
                else
                {
                    MessageBox.Show($"安装失败: {result.ErrorMessage}", "安装失败",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// 可用插件视图模型
    /// </summary>
    public class AvailablePluginViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string SourceDirectory { get; set; } = string.Empty;
        public bool HasDescription { get; set; }
        public bool HasAuthor { get; set; }
        public Visibility HasDescriptionVisibility => HasDescription ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HasAuthorVisibility => HasAuthor ? Visibility.Visible : Visibility.Collapsed;
    }
}
