using System.Windows;
using FloatWebPlayer.Services;
using FloatWebPlayer.Views;

namespace FloatWebPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        #region Fields

        private PlayerWindow? _playerWindow;
        private ControlBarWindow? _controlBarWindow;
        private HotkeyService? _hotkeyService;

        /// <summary>
        /// 默认快进/倒退秒数
        /// </summary>
        private const int DefaultSeekSeconds = 5;

        #endregion

        #region Event Handlers

        /// <summary>
        /// 应用启动事件
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 创建主窗口（播放器）
            _playerWindow = new PlayerWindow();

            // 创建控制栏窗口
            _controlBarWindow = new ControlBarWindow();

            // 设置窗口间事件关联
            SetupWindowBindings();

            // 显示窗口
            _playerWindow.Show();
            
            // 控制栏窗口启动自动显示/隐藏监听（默认隐藏，鼠标移到顶部触发显示）
            _controlBarWindow.StartAutoShowHide();

            // 启动全局快捷键服务
            StartHotkeyService();
        }

        /// <summary>
        /// 设置两窗口之间的事件绑定
        /// </summary>
        private void SetupWindowBindings()
        {
            if (_playerWindow == null || _controlBarWindow == null)
                return;

            // 控制栏导航请求 → 播放器窗口加载
            _controlBarWindow.NavigateRequested += (s, url) =>
            {
                _playerWindow.Navigate(url);
            };

            // 控制栏后退请求
            _controlBarWindow.BackRequested += (s, e) =>
            {
                _playerWindow.GoBack();
            };

            // 控制栏前进请求
            _controlBarWindow.ForwardRequested += (s, e) =>
            {
                _playerWindow.GoForward();
            };

            // 控制栏刷新请求
            _controlBarWindow.RefreshRequested += (s, e) =>
            {
                _playerWindow.Refresh();
            };

            // 播放器窗口关闭时，关闭控制栏
            _playerWindow.Closed += (s, e) =>
            {
                _controlBarWindow.Close();
            };

            // 播放器 URL 变化时，同步到控制栏
            _playerWindow.UrlChanged += (s, url) =>
            {
                _controlBarWindow.CurrentUrl = url;
            };

            // 播放器导航状态变化时，更新控制栏按钮
            _playerWindow.NavigationStateChanged += (s, e) =>
            {
                _controlBarWindow.UpdateBackButtonState(_playerWindow.CanGoBack);
                _controlBarWindow.UpdateForwardButtonState(_playerWindow.CanGoForward);
            };
        }

        /// <summary>
        /// 启动全局快捷键服务
        /// </summary>
        private void StartHotkeyService()
        {
            _hotkeyService = new HotkeyService();

            // 绑定快捷键事件
            _hotkeyService.SeekBackward += (s, e) =>
            {
                _playerWindow?.SeekAsync(-DefaultSeekSeconds);
            };

            _hotkeyService.SeekForward += (s, e) =>
            {
                _playerWindow?.SeekAsync(DefaultSeekSeconds);
            };

            _hotkeyService.TogglePlay += (s, e) =>
            {
                _playerWindow?.TogglePlayAsync();
            };

            _hotkeyService.DecreaseOpacity += (s, e) =>
            {
                // TODO: Phase 3.10 实现透明度调节
            };

            _hotkeyService.IncreaseOpacity += (s, e) =>
            {
                // TODO: Phase 3.10 实现透明度调节
            };

            _hotkeyService.ToggleClickThrough += (s, e) =>
            {
                // TODO: Phase 3.11 实现鼠标穿透模式
            };

            _hotkeyService.Start();
        }

        /// <summary>
        /// 应用退出事件
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService?.Dispose();
            base.OnExit(e);
        }

        #endregion
    }
}

