using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FloatWebPlayer.Services
{
    /// <summary>
    /// 全局快捷键服务，使用低级键盘钩子实现
    /// 按键不会被拦截，既能触发快捷键功能，又能正常输入
    /// </summary>
    public class HotkeyService : IDisposable
    {
        #region Win32 API

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // 虚拟键码
        private const uint VK_0 = 0x30;
        private const uint VK_5 = 0x35;
        private const uint VK_6 = 0x36;
        private const uint VK_7 = 0x37;
        private const uint VK_8 = 0x38;
        private const uint VK_OEM_3 = 0xC0; // ` 波浪键

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

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc? _hookProc;
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

            _hookProc = HookCallback;
            _hookId = SetHook(_hookProc);
            _isStarted = true;
        }

        /// <summary>
        /// 停止快捷键服务
        /// </summary>
        public void Stop()
        {
            if (!_isStarted) return;

            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            _hookProc = null;
            _isStarted = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 设置键盘钩子
        /// </summary>
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, 
                GetModuleHandle(curModule?.ModuleName), 0);
        }

        /// <summary>
        /// 键盘钩子回调
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                
                // 在 UI 线程上触发事件
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    switch (hookStruct.vkCode)
                    {
                        case VK_5:
                            SeekBackward?.Invoke(this, EventArgs.Empty);
                            break;
                        case VK_6:
                            SeekForward?.Invoke(this, EventArgs.Empty);
                            break;
                        case VK_OEM_3:
                            TogglePlay?.Invoke(this, EventArgs.Empty);
                            break;
                        case VK_7:
                            DecreaseOpacity?.Invoke(this, EventArgs.Empty);
                            break;
                        case VK_8:
                            IncreaseOpacity?.Invoke(this, EventArgs.Empty);
                            break;
                        case VK_0:
                            ToggleClickThrough?.Invoke(this, EventArgs.Empty);
                            break;
                    }
                });
            }

            // 关键：调用 CallNextHookEx 让按键继续传递，不拦截
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
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
