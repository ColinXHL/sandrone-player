using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FloatWebPlayer.Services
{
    /// <summary>
    /// 全局快捷键服务，使用 Win32 API RegisterHotKey 实现
    /// </summary>
    public class HotkeyService : IDisposable
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        #region Constants

        // Windows 消息常量
        private const int WM_HOTKEY = 0x0312;

        // 修饰键常量
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        // 虚拟键码
        private const uint VK_0 = 0x30;
        private const uint VK_5 = 0x35;
        private const uint VK_6 = 0x36;
        private const uint VK_7 = 0x37;
        private const uint VK_8 = 0x38;
        private const uint VK_OEM_3 = 0xC0; // ` 波浪键

        // 快捷键 ID
        private const int HOTKEY_SEEK_BACKWARD = 1;  // 5
        private const int HOTKEY_SEEK_FORWARD = 2;   // 6
        private const int HOTKEY_TOGGLE_PLAY = 3;    // `
        private const int HOTKEY_DECREASE_OPACITY = 4; // 7
        private const int HOTKEY_INCREASE_OPACITY = 5; // 8
        private const int HOTKEY_TOGGLE_CLICK_THROUGH = 6; // 0

        #endregion

        #region Events

        /// <summary>
        /// 视频倒退事件
        /// </summary>
        public event EventHandler? SeekBackward;

        /// <summary>
        /// 视频前进事件
        /// </summary>
        public event EventHandler? SeekForward;

        /// <summary>
        /// 播放/暂停切换事件
        /// </summary>
        public event EventHandler? TogglePlay;

        /// <summary>
        /// 降低透明度事件
        /// </summary>
        public event EventHandler? DecreaseOpacity;

        /// <summary>
        /// 增加透明度事件
        /// </summary>
        public event EventHandler? IncreaseOpacity;

        /// <summary>
        /// 切换鼠标穿透模式事件
        /// </summary>
        public event EventHandler? ToggleClickThrough;

        #endregion

        #region Fields

        private Window? _messageWindow;
        private HwndSource? _hwndSource;
        private IntPtr _hwnd = IntPtr.Zero;
        private bool _isStarted;
        private bool _disposed;

        #endregion

        #region Public Methods

        /// <summary>
        /// 启动快捷键服务
        /// </summary>
        public void Start()
        {
            if (_isStarted) return;

            // 创建隐藏窗口用于接收消息
            _messageWindow = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Visibility = Visibility.Hidden
            };

            _messageWindow.SourceInitialized += (s, e) =>
            {
                _hwnd = new WindowInteropHelper(_messageWindow).Handle;
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                _hwndSource?.AddHook(WndProc);

                // 注册所有快捷键
                RegisterAllHotkeys();
            };

            _messageWindow.Show();
            _messageWindow.Hide();

            _isStarted = true;
        }

        /// <summary>
        /// 停止快捷键服务
        /// </summary>
        public void Stop()
        {
            if (!_isStarted) return;

            UnregisterAllHotkeys();

            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
            _hwndSource = null;

            _messageWindow?.Close();
            _messageWindow = null;

            _hwnd = IntPtr.Zero;
            _isStarted = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 注册所有快捷键
        /// </summary>
        private void RegisterAllHotkeys()
        {
            if (_hwnd == IntPtr.Zero) return;

            // 注册快捷键（无修饰键）
            RegisterHotKey(_hwnd, HOTKEY_SEEK_BACKWARD, MOD_NONE, VK_5);
            RegisterHotKey(_hwnd, HOTKEY_SEEK_FORWARD, MOD_NONE, VK_6);
            RegisterHotKey(_hwnd, HOTKEY_TOGGLE_PLAY, MOD_NONE, VK_OEM_3);
            RegisterHotKey(_hwnd, HOTKEY_DECREASE_OPACITY, MOD_NONE, VK_7);
            RegisterHotKey(_hwnd, HOTKEY_INCREASE_OPACITY, MOD_NONE, VK_8);
            RegisterHotKey(_hwnd, HOTKEY_TOGGLE_CLICK_THROUGH, MOD_NONE, VK_0);
        }

        /// <summary>
        /// 注销所有快捷键
        /// </summary>
        private void UnregisterAllHotkeys()
        {
            if (_hwnd == IntPtr.Zero) return;

            UnregisterHotKey(_hwnd, HOTKEY_SEEK_BACKWARD);
            UnregisterHotKey(_hwnd, HOTKEY_SEEK_FORWARD);
            UnregisterHotKey(_hwnd, HOTKEY_TOGGLE_PLAY);
            UnregisterHotKey(_hwnd, HOTKEY_DECREASE_OPACITY);
            UnregisterHotKey(_hwnd, HOTKEY_INCREASE_OPACITY);
            UnregisterHotKey(_hwnd, HOTKEY_TOGGLE_CLICK_THROUGH);
        }

        /// <summary>
        /// 窗口过程，处理 WM_HOTKEY 消息
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();

                switch (hotkeyId)
                {
                    case HOTKEY_SEEK_BACKWARD:
                        SeekBackward?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case HOTKEY_SEEK_FORWARD:
                        SeekForward?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case HOTKEY_TOGGLE_PLAY:
                        TogglePlay?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case HOTKEY_DECREASE_OPACITY:
                        DecreaseOpacity?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case HOTKEY_INCREASE_OPACITY:
                        IncreaseOpacity?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;

                    case HOTKEY_TOGGLE_CLICK_THROUGH:
                        ToggleClickThrough?.Invoke(this, EventArgs.Empty);
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        ~HotkeyService()
        {
            Dispose(false);
        }

        #endregion
    }
}
