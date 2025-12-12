using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FloatWebPlayer.Helpers
{
    /// <summary>
    /// Win32 API 封装，用于窗口操作
    /// </summary>
    public static class Win32Helper
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region Constants

        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;

        // 调整大小方向常量
        private const int SC_SIZE_HTLEFT = 0xF001;
        private const int SC_SIZE_HTRIGHT = 0xF002;
        private const int SC_SIZE_HTTOP = 0xF003;
        private const int SC_SIZE_HTTOPLEFT = 0xF004;
        private const int SC_SIZE_HTTOPRIGHT = 0xF005;
        private const int SC_SIZE_HTBOTTOM = 0xF006;
        private const int SC_SIZE_HTBOTTOMLEFT = 0xF007;
        private const int SC_SIZE_HTBOTTOMRIGHT = 0xF008;

        // 扩展窗口样式常量
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        // SetLayeredWindowAttributes 标志
        private const uint LWA_ALPHA = 0x02;

        #endregion

        #region Resize Direction Enum

        /// <summary>
        /// 调整大小方向
        /// </summary>
        public enum ResizeDirection
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 检测鼠标位置对应的调整大小方向
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="mousePosition">鼠标相对窗口的位置</param>
        /// <param name="borderThickness">边框厚度</param>
        /// <returns>调整方向</returns>
        public static ResizeDirection GetResizeDirection(Window window, System.Windows.Point mousePosition, int borderThickness = 6)
        {
            double width = window.ActualWidth;
            double height = window.ActualHeight;

            bool left = mousePosition.X < borderThickness;
            bool right = mousePosition.X > width - borderThickness;
            bool top = mousePosition.Y < borderThickness;
            bool bottom = mousePosition.Y > height - borderThickness;

            if (top && left) return ResizeDirection.TopLeft;
            if (top && right) return ResizeDirection.TopRight;
            if (bottom && left) return ResizeDirection.BottomLeft;
            if (bottom && right) return ResizeDirection.BottomRight;
            if (left) return ResizeDirection.Left;
            if (right) return ResizeDirection.Right;
            if (top) return ResizeDirection.Top;
            if (bottom) return ResizeDirection.Bottom;

            return ResizeDirection.None;
        }

        /// <summary>
        /// 开始调整窗口大小
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="direction">调整方向</param>
        public static void StartResize(Window window, ResizeDirection direction)
        {
            if (direction == ResizeDirection.None) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            int scSize = direction switch
            {
                ResizeDirection.Left => SC_SIZE_HTLEFT,
                ResizeDirection.Right => SC_SIZE_HTRIGHT,
                ResizeDirection.Top => SC_SIZE_HTTOP,
                ResizeDirection.TopLeft => SC_SIZE_HTTOPLEFT,
                ResizeDirection.TopRight => SC_SIZE_HTTOPRIGHT,
                ResizeDirection.Bottom => SC_SIZE_HTBOTTOM,
                ResizeDirection.BottomLeft => SC_SIZE_HTBOTTOMLEFT,
                ResizeDirection.BottomRight => SC_SIZE_HTBOTTOMRIGHT,
                _ => 0
            };

            if (scSize != 0)
            {
                ReleaseCapture();
                SendMessage(hwnd, WM_SYSCOMMAND, (IntPtr)scSize, IntPtr.Zero);
            }
        }

        /// <summary>
        /// 开始移动窗口（模拟标题栏拖动）
        /// </summary>
        /// <param name="window">目标窗口</param>
        public static void StartMove(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            ReleaseCapture();
            SendMessage(hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }

        /// <summary>
        /// 设置窗口鼠标穿透模式
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="enable">是否启用穿透</param>
        public static void SetClickThrough(Window window, bool enable)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (enable)
            {
                // 添加 WS_EX_TRANSPARENT 和 WS_EX_LAYERED
                exStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
            }
            else
            {
                // 移除 WS_EX_TRANSPARENT（保留 WS_EX_LAYERED 用于透明度）
                exStyle &= ~WS_EX_TRANSPARENT;
            }

            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        /// <summary>
        /// 设置窗口透明度（0.0 - 1.0）
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="opacity">透明度值（0.0 完全透明，1.0 完全不透明）</param>
        public static void SetWindowOpacity(Window window, double opacity)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            // 确保窗口有 WS_EX_LAYERED 样式
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            if ((exStyle & WS_EX_LAYERED) == 0)
            {
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
            }

            // 设置透明度（0-255）
            byte alpha = (byte)(Math.Clamp(opacity, 0.0, 1.0) * 255);
            SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
        }

        /// <summary>
        /// 获取窗口是否处于鼠标穿透模式
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <returns>是否穿透</returns>
        public static bool IsClickThrough(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TRANSPARENT) != 0;
        }

        /// <summary>
        /// 检查鼠标是否在窗口区域内
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <returns>鼠标是否在窗口内</returns>
        public static bool IsCursorInWindow(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return false;

            if (!GetCursorPos(out POINT pt)) return false;
            if (!GetWindowRect(hwnd, out RECT rect)) return false;

            // 使用 Win32 API 获取的窗口坐标（物理像素）进行比较
            return pt.X >= rect.Left && pt.X <= rect.Right && 
                   pt.Y >= rect.Top && pt.Y <= rect.Bottom;
        }

        #endregion
    }
}
