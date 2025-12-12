# 🎬 FloatWebPlayer

**A floating web player for Windows, designed for watching tutorial videos while gaming.**

**Windows 悬浮网页播放器，专为游戏时观看攻略视频设计。**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6)](https://www.microsoft.com/windows)

---

## ✨ Features | 功能特性

- **🪟 Always on Top** - Floating window stays above other applications | 悬浮窗口始终置顶
- **⌨️ Global Hotkeys** - Control playback without leaving your game | 全局快捷键，无需切换窗口
- **👻 Mouse Click-Through** - Interact with apps behind the player | 鼠标穿透模式
- **🎚️ Adjustable Opacity** - Set transparency from 20% to 100% | 透明度可调 (20%-100%)
- **🎯 Edge Snapping** - Window snaps to screen edges | 窗口边缘吸附
- **🍪 Cookie Persistence** - Stay logged in to websites | Cookie 持久化，保持登录状态
- **🎨 Minimal UI** - Clean borderless design with custom controls | 简洁无边框设计

---

## 📸 Screenshots | 截图

> *Coming soon... | 即将添加...*

---

## 🖥️ System Requirements | 系统要求

- **OS**: Windows 10/11
- **Runtime**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Browser Engine**: [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (usually pre-installed on Windows 10/11)

---

## 📥 Installation | 安装

### Option 1: Download Release | 下载发布版

> *Release builds coming soon... | 发布版即将推出...*

### Option 2: Build from Source | 从源码构建

```powershell
# Clone the repository | 克隆仓库
git clone https://github.com/ColinXHL/float-web-player.git
cd float-web-player

# Build | 构建
dotnet build -c Release

# Run | 运行
dotnet run --project FloatWebPlayer
```

---

## ⌨️ Default Hotkeys | 默认快捷键

| Key | Function | 功能 |
|-----|----------|------|
| `` ` `` | Play / Pause | 播放 / 暂停 |
| `5` | Seek Backward (5s) | 后退 5 秒 |
| `6` | Seek Forward (5s) | 前进 5 秒 |
| `7` | Decrease Opacity | 降低透明度 |
| `8` | Increase Opacity | 增加透明度 |
| `0` | Toggle Click-Through | 切换鼠标穿透 |

> ⚠️ Hotkeys are disabled when typing in text fields. | 在输入框中输入时快捷键自动禁用。

---

## 🛠️ Tech Stack | 技术栈

| Component | Technology |
|-----------|------------|
| Framework | .NET 8.0 + WPF |
| Browser Engine | Microsoft WebView2 |
| Global Hotkeys | Win32 API (Low-level Keyboard Hook) |
| Click-Through | Win32 API (WS_EX_TRANSPARENT) |
| Window Control | Win32 API (SendMessage) |

---

## 📁 Project Structure | 项目结构

```
FloatWebPlayer/
├── Views/              # WPF Windows (Player, ControlBar, OSD)
├── Services/           # HotkeyService, etc.
├── Helpers/            # Win32Helper, ScriptInjector
├── Models/             # Data models
├── Scripts/            # Injected JS/CSS for WebView2
└── docs/               # Design documents
```

---

## 🚧 Development Status | 开发状态

- [x] Basic player window with WebView2
- [x] Floating control bar (top of screen)
- [x] Global hotkey support
- [x] Opacity adjustment
- [x] Mouse click-through mode
- [x] OSD notifications
- [ ] Edge snapping
- [ ] History & Bookmarks
- [ ] Settings window

---

## 🤝 Contributing | 贡献

Issues and Pull Requests are welcome!

欢迎提交 Issue 和 Pull Request！

---

## 📄 License | 许可证

This project is licensed under the [MIT License](LICENSE).

本项目采用 [MIT 许可证](LICENSE) 开源。

---

## 🙏 Acknowledgments | 致谢

- [Microsoft WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- [Bilibili](https://www.bilibili.com/) - Primary use case inspiration
