# 键盘捕获设计文档

## 概述

本项目使用纯 Win32 API 实现全局键盘钩子，无需第三方库依赖。当前实现已支持基础热键功能，本文档记录扩展至游戏辅助场景的设计思路。

## 当前实现

### 架构

```
HotkeyConfig (配置根)
  └── HotkeyProfile (配置档案，支持进程自动切换)
        └── HotkeyBinding (按键绑定: Key + Modifiers + Action)

HotkeyService (钩子服务)
  └── ActionDispatcher (动作分发器，支持脚本扩展)
```

### 核心组件

| 文件 | 职责 |
|------|------|
| `Win32Helper.cs` | Win32 API 封装 (`SetWindowsHookEx`, `GetAsyncKeyState`) |
| `HotkeyService.cs` | 低级键盘钩子 (`WH_KEYBOARD_LL`)，按键分发 |
| `ActionDispatcher.cs` | 字符串 Action → 处理器映射 |
| `HotkeyBinding.cs` | 按键绑定模型，支持修饰键和进程过滤 |

### 特性

- **不拦截按键**：`CallNextHookEx` 确保按键正常传递
- **输入保护**：焦点在 TextBox 等控件时跳过热键
- **进程过滤**：可限定特定进程生效

---

## 游戏辅助场景扩展

### 功能矩阵

| 功能 | 当前 | 扩展难度 | 说明 |
|------|------|----------|------|
| KeyDown 监听 | ✅ | - | 已实现 |
| KeyUp 监听 | ❌ | 低 | 新增 `WM_KEYUP` 处理 |
| 鼠标钩子 | ❌ | 中 | 新增 `WH_MOUSE_LL` |
| 按键状态追踪 | ❌ | 低 | 字典记录按下状态 |
| 长按变连发 | ❌ | 中 | 定时器实现 |
| 模拟按键输入 | ❌ | 低 | `SendInput` API |
| 录制回放 | ❌ | 中高 | 时间戳 + 事件序列化 |
| 暂停时释放按键 | ❌ | 低 | 遍历 VK 枚举释放 |

---

## 扩展实现指南

### 1. KeyUp 监听

在 `Win32Helper.cs` 添加常量：

```csharp
public const int WM_KEYUP = 0x0101;
```

修改 `HotkeyService.HookCallback`：

```csharp
private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
{
    if (nCode >= 0)
    {
        var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
        var vkCode = hookStruct.vkCode;

        if (wParam == (IntPtr)Win32Helper.WM_KEYDOWN)
        {
            HandleKeyDown(vkCode);
        }
        else if (wParam == (IntPtr)Win32Helper.WM_KEYUP)
        {
            HandleKeyUp(vkCode);
        }
    }
    return Win32Helper.CallNextHook(_hookId, nCode, wParam, lParam);
}
```

### 2. 按键状态追踪

新增字段：

```csharp
private readonly Dictionary<uint, bool> _keyDownState = new();
```

处理逻辑：

```csharp
private void HandleKeyDown(uint vkCode)
{
    if (_keyDownState.TryGetValue(vkCode, out var isDown) && isDown)
        return; // 已按下，跳过重复事件

    _keyDownState[vkCode] = true;
    // 触发按下事件...
}

private void HandleKeyUp(uint vkCode)
{
    _keyDownState[vkCode] = false;
    // 触发释放事件...
}
```

### 3. 长按变连发

新增字段：

```csharp
private readonly Dictionary<uint, DateTime> _keyFirstDownTime = new();
private readonly Dictionary<uint, System.Timers.Timer> _repeatTimers = new();
```

实现：

```csharp
private void HandleKeyDown(uint vkCode)
{
    if (!_keyFirstDownTime.ContainsKey(vkCode))
    {
        _keyFirstDownTime[vkCode] = DateTime.Now;
        TriggerKeyAction(vkCode); // 首次按下立即触发
    }
    else
    {
        var elapsed = DateTime.Now - _keyFirstDownTime[vkCode];
        // 长按阈值 200ms 后启动连发
        if (elapsed.TotalMilliseconds > 200 && !_repeatTimers.ContainsKey(vkCode))
        {
            var timer = new System.Timers.Timer(50); // 50ms 间隔连发
            timer.Elapsed += (s, e) => TriggerKeyAction(vkCode);
            timer.Start();
            _repeatTimers[vkCode] = timer;
        }
    }
}

private void HandleKeyUp(uint vkCode)
{
    _keyFirstDownTime.Remove(vkCode);
    if (_repeatTimers.TryGetValue(vkCode, out var timer))
    {
        timer.Stop();
        timer.Dispose();
        _repeatTimers.Remove(vkCode);
    }
}
```

### 4. 鼠标钩子

在 `Win32Helper.cs` 添加：

```csharp
// 鼠标钩子常量
public const int WH_MOUSE_LL = 14;
public const int WM_LBUTTONDOWN = 0x0201;
public const int WM_LBUTTONUP = 0x0202;
public const int WM_RBUTTONDOWN = 0x0204;
public const int WM_RBUTTONUP = 0x0205;
public const int WM_MOUSEMOVE = 0x0200;
public const int WM_MOUSEWHEEL = 0x020A;

// 鼠标钩子结构体
[StructLayout(LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

// 设置鼠标钩子
public static IntPtr SetMouseHook(LowLevelMouseProc proc)
{
    using var curProcess = Process.GetCurrentProcess();
    using var curModule = curProcess.MainModule;
    return SetWindowsHookEx(WH_MOUSE_LL, proc,
        GetModuleHandle(curModule?.ModuleName), 0);
}

public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
```

### 5. 模拟按键输入

在 `Win32Helper.cs` 添加：

```csharp
[DllImport("user32.dll")]
private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public uint type;
    public InputUnion U;
}

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)] public MOUSEINPUT mi;
    [FieldOffset(0)] public KEYBDINPUT ki;
}

[StructLayout(LayoutKind.Sequential)]
public struct KEYBDINPUT
{
    public ushort wVk;
    public ushort wScan;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

public const uint INPUT_KEYBOARD = 1;
public const uint KEYEVENTF_KEYUP = 0x0002;

public static void SimulateKeyDown(ushort vkCode)
{
    var input = new INPUT
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT { wVk = vkCode }
        }
    };
    SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
}

public static void SimulateKeyUp(ushort vkCode)
{
    var input = new INPUT
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT { wVk = vkCode, dwFlags = KEYEVENTF_KEYUP }
        }
    };
    SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
}
```

### 6. 录制回放

数据结构：

```csharp
public enum InputEventType { KeyDown, KeyUp, MouseDown, MouseUp, MouseMove }

public record InputEvent(
    uint Timestamp,      // 相对时间戳 (ms)
    InputEventType Type,
    uint Code,           // VK 或鼠标按钮
    int? X = null,       // 鼠标位置
    int? Y = null
);
```

录制器：

```csharp
public class InputRecorder
{
    private readonly List<InputEvent> _events = new();
    private uint _startTime;
    private bool _isRecording;

    public void Start()
    {
        _events.Clear();
        _startTime = GetTickCount();
        _isRecording = true;
    }

    public void Stop() => _isRecording = false;

    public void RecordKeyDown(uint vkCode)
    {
        if (!_isRecording) return;
        _events.Add(new InputEvent(GetTickCount() - _startTime, InputEventType.KeyDown, vkCode));
    }

    public IReadOnlyList<InputEvent> GetEvents() => _events;
}
```

回放器：

```csharp
public class InputPlayer
{
    public async Task PlayAsync(IReadOnlyList<InputEvent> events, CancellationToken ct)
    {
        uint lastTime = 0;
        foreach (var evt in events)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay((int)(evt.Timestamp - lastTime), ct);
            lastTime = evt.Timestamp;

            switch (evt.Type)
            {
                case InputEventType.KeyDown:
                    Win32Helper.SimulateKeyDown((ushort)evt.Code);
                    break;
                case InputEventType.KeyUp:
                    Win32Helper.SimulateKeyUp((ushort)evt.Code);
                    break;
                // ... 鼠标事件处理
            }
        }
    }
}
```

### 7. 暂停时释放所有按键

```csharp
public static void ReleaseAllKeys()
{
    foreach (int vk in Enum.GetValues(typeof(VirtualKey)))
    {
        if ((GetAsyncKeyState(vk) & 0x8000) != 0)
        {
            SimulateKeyUp((ushort)vk);
        }
    }
}
```

---

## 与 BetterGenshinImpact 对比

| 方面 | BetterGI | 本项目 |
|------|----------|--------|
| 依赖 | `MouseKeyHook` 库 | 纯 Win32 API |
| 代码量 | 少（库封装） | 多 200-400 行 |
| 控制力 | 受限于库 API | 完全控制 |
| 架构 | 直接事件 | 配置驱动 + 分发器 |
| 扩展性 | 改库或 fork | 直接修改 |

**推荐场景**：
- 轻量工具 / 零依赖要求：使用本项目方案
- 快速开发 / 功能丰富：使用 `MouseKeyHook`

---

## 注意事项

1. **游戏反作弊**：部分游戏会检测全局钩子，可能导致封号
2. **管理员权限**：某些游戏以管理员运行，钩子需同样权限
3. **性能**：钩子回调应快速返回，耗时操作需异步处理
4. **线程安全**：定时器回调在非 UI 线程，修改 UI 需 `Dispatcher.Invoke`
