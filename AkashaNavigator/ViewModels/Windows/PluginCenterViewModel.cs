using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AkashaNavigator.Views.Pages;

namespace AkashaNavigator.ViewModels.Windows
{
    /// <summary>
    /// 插件中心窗口 ViewModel - 混合架构
    /// </summary>
    public partial class PluginCenterViewModel : ObservableObject
    {
        private readonly MyProfilesPage _myProfilesPage;
        private readonly ProfileMarketPage _profileMarketPage;
        private readonly InstalledPluginsPage _installedPluginsPage;
        private readonly AvailablePluginsPage _availablePluginsPage;

        /// <summary>
        /// 当前显示的页面类型（自动生成属性和通知）
        /// </summary>
        [ObservableProperty]
        private PluginCenterPageType _currentPage = PluginCenterPageType.MyProfiles;

        public PluginCenterViewModel(
            MyProfilesPage myProfilesPage,
            ProfileMarketPage profileMarketPage,
            InstalledPluginsPage installedPluginsPage,
            AvailablePluginsPage availablePluginsPage)
        {
            _myProfilesPage = myProfilesPage ?? throw new ArgumentNullException(nameof(myProfilesPage));
            _profileMarketPage = profileMarketPage ?? throw new ArgumentNullException(nameof(profileMarketPage));
            _installedPluginsPage = installedPluginsPage ?? throw new ArgumentNullException(nameof(installedPluginsPage));
            _availablePluginsPage = availablePluginsPage ?? throw new ArgumentNullException(nameof(availablePluginsPage));
        }

        /// <summary>
        /// 导航到我的配置页面（自动生成 NavigateToMyProfilesCommand）
        /// </summary>
        [RelayCommand]
        private void NavigateToMyProfiles()
        {
            CurrentPage = PluginCenterPageType.MyProfiles;
        }

        /// <summary>
        /// 导航到配置市场页面（自动生成 NavigateToProfileMarketCommand）
        /// </summary>
        [RelayCommand]
        private void NavigateToProfileMarket()
        {
            CurrentPage = PluginCenterPageType.ProfileMarket;
        }

        /// <summary>
        /// 导航到已安装插件页面（自动生成 NavigateToInstalledPluginsCommand）
        /// </summary>
        [RelayCommand]
        private void NavigateToInstalledPlugins()
        {
            CurrentPage = PluginCenterPageType.InstalledPlugins;
        }

        /// <summary>
        /// 导航到可用插件页面（自动生成 NavigateToAvailablePluginsCommand）
        /// </summary>
        [RelayCommand]
        private void NavigateToAvailablePlugins()
        {
            CurrentPage = PluginCenterPageType.AvailablePlugins;
        }

        /// <summary>
        /// CurrentPage 属性变化时的处理（由 CommunityToolkit.Mvvm 自动调用）
        /// </summary>
        partial void OnCurrentPageChanged(PluginCenterPageType value)
        {
            // 刷新页面数据（ViewModel 职责：数据管理）
            switch (value)
            {
                case PluginCenterPageType.MyProfiles:
                    _myProfilesPage.RefreshProfileList();
                    break;
                case PluginCenterPageType.ProfileMarket:
                    var vm = _profileMarketPage.DataContext as ViewModels.Pages.ProfileMarketPageViewModel;
                    vm?.LoadProfilesAsync();
                    break;
                case PluginCenterPageType.InstalledPlugins:
                    _installedPluginsPage.CheckAndRefreshPluginList();
                    break;
                case PluginCenterPageType.AvailablePlugins:
                    _availablePluginsPage.RefreshPluginList();
                    break;
            }
        }

        /// <summary>
        /// 刷新当前页面
        /// </summary>
        public void RefreshCurrentPage()
        {
            switch (CurrentPage)
            {
                case PluginCenterPageType.MyProfiles:
                    _myProfilesPage.RefreshProfileList();
                    break;
                case PluginCenterPageType.ProfileMarket:
                    var vm = _profileMarketPage.DataContext as ViewModels.Pages.ProfileMarketPageViewModel;
                    vm?.LoadProfilesAsync();
                    break;
                case PluginCenterPageType.InstalledPlugins:
                    _installedPluginsPage.CheckAndRefreshPluginList();
                    break;
                case PluginCenterPageType.AvailablePlugins:
                    _availablePluginsPage.RefreshPluginList();
                    break;
            }
        }

        // 暴露 Pages 供 Code-behind 访问
        public MyProfilesPage MyProfilesPage => _myProfilesPage;
        public ProfileMarketPage ProfileMarketPage => _profileMarketPage;
        public InstalledPluginsPage InstalledPluginsPage => _installedPluginsPage;
        public AvailablePluginsPage AvailablePluginsPage => _availablePluginsPage;
    }

    /// <summary>
    /// 插件中心页面类型
    /// </summary>
    public enum PluginCenterPageType
    {
        MyProfiles,
        ProfileMarket,
        InstalledPlugins,
        AvailablePlugins
    }
}
