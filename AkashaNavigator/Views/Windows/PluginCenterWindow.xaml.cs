using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AkashaNavigator.Helpers;
using AkashaNavigator.ViewModels.Windows;

namespace AkashaNavigator.Views.Windows
{
    /// <summary>
    /// 插件中心窗口 - 统一管理所有插件的安装、卸载和配置
    /// </summary>
    public partial class PluginCenterWindow : AnimatedWindow
    {
        private readonly PluginCenterViewModel _viewModel;

        public PluginCenterWindow(PluginCenterViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = _viewModel;

            LoadPages();
            UpdatePageVisibility(_viewModel.CurrentPage);

            // 订阅 ViewModel 的 PropertyChanged 事件，处理页面显示切换
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.CurrentPage))
                {
                    UpdatePageVisibility(_viewModel.CurrentPage);
                }
            };
        }

        /// <summary>
        /// 加载所有 Pages
        /// </summary>
        private void LoadPages()
        {
            ContentArea.Children.Add(_viewModel.MyProfilesPage);
            ContentArea.Children.Add(_viewModel.ProfileMarketPage);
            ContentArea.Children.Add(_viewModel.InstalledPluginsPage);
            ContentArea.Children.Add(_viewModel.AvailablePluginsPage);
        }

        /// <summary>
        /// UI 逻辑：页面显示切换（保留在 Code-behind，因为涉及 Visibility 操作）
        /// </summary>
        private void UpdatePageVisibility(PluginCenterPageType currentPage)
        {
            _viewModel.MyProfilesPage.Visibility = currentPage == PluginCenterPageType.MyProfiles
                ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.ProfileMarketPage.Visibility = currentPage == PluginCenterPageType.ProfileMarket
                ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.InstalledPluginsPage.Visibility = currentPage == PluginCenterPageType.InstalledPlugins
                ? Visibility.Visible : Visibility.Collapsed;
            _viewModel.AvailablePluginsPage.Visibility = currentPage == PluginCenterPageType.AvailablePlugins
                ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// UI 逻辑：标题栏拖动（保持不变）
        /// </summary>
        private new void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        /// <summary>
        /// UI 逻辑：关闭按钮（保持不变）
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 公开方法供外部调用
        /// </summary>
        public void NavigateToInstalledPlugins()
        {
            _viewModel.NavigateToInstalledPluginsCommand.Execute(null);
        }

        /// <summary>
        /// 刷新当前页面
        /// </summary>
        public void RefreshCurrentPage()
        {
            _viewModel.RefreshCurrentPage();
        }
    }
}
