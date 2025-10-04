# ToggleDesktop

<div align="center">

![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-lightgrey.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

一键切换 Windows 桌面图标显示/隐藏的工具，让您更好地欣赏精美壁纸。

[功能特性](#-功能特性) • [快速开始](#-快速开始) • [使用指南](#-使用指南) • [开发](#-开发) • [许可证](#-许可证)

<img src="https://via.placeholder.com/800x400/f0f0f0/333333?text=ToggleDesktop+Demo" alt="ToggleDesktop Demo" />

</div>

---

## ✨ 功能特性

### 核心功能

- 🎯 **一键切换** - 快速显示/隐藏桌面图标，瞬间拥有纯净桌面
- ⌨️ **全局热键** - 默认 `Ctrl+Alt+K`，可自定义任意快捷键
- 🖥️ **智能桌面显示** - 热键触发时自动显示桌面（如果未在前台）
- 🖼️ **必应壁纸收藏** - 一键下载保存必应每日壁纸
- 🔔 **托盘驻留** - 常驻系统托盘，右键菜单快速操作
- ⚙️ **丰富设置** - 热键自定义、开机自启、通知开关等

### 亮点功能

| 功能 | 说明 |
|------|------|
| **智能桌面显示** | 按热键时如果有窗口打开，自动先显示桌面再隐藏图标 |
| **壁纸收藏** | 自动下载必应每日壁纸，保存到 `我的图片\BingWallpapers` |
| **命令行支持** | 支持命令行操作，可集成到脚本中 |
| **状态指示** | 动态托盘图标实时显示当前状态 |
| **开机自启** | 可设置开机自动启动，无需手动运行 |

---

## 📦 快速开始

### 系统要求

- Windows 10 或 Windows 11
- .NET 8.0 Runtime（或更高版本）
  - 下载地址：https://dotnet.microsoft.com/download/dotnet/8.0

### 下载安装

1. 前往 [Releases](https://github.com/u9-hbzhang/ToggleDesktop/releases) 页面
2. 下载最新版本的 `ToggleDesktop.exe`
3. 双击运行即可

**首次运行后，程序会在系统托盘显示图标。**

### 快速使用

1. **双击托盘图标** - 切换桌面图标显示/隐藏
2. **按下热键** `Ctrl+Alt+K` - 快速切换
3. **右键托盘图标** - 更多选项

---

## 📖 使用指南

### 基本操作

#### 切换桌面图标

**方法 1：快捷键**
- 按下 `Ctrl+Alt+K`（可在设置中自定义）
- 如果桌面不在前台，会自动先显示桌面

**方法 2：托盘图标**
- 双击托盘图标即可切换

**方法 3：右键菜单**
- 右键托盘图标 → 选择"显示/隐藏桌面图标"

#### 收藏必应壁纸

1. 右键点击托盘图标
2. 选择"收藏必应壁纸"
3. 等待下载完成提示
4. 壁纸保存在：`C:\Users\你的用户名\Pictures\BingWallpapers\`

**文件命名格式**：`20241002_挪威北部特罗姆瑟的极光.jpg`

#### 查看已保存的壁纸

右键托盘图标 → "打开壁纸文件夹"

### 高级设置

右键托盘图标 → "设置"，可以配置：

- **热键设置** - 自定义快捷键组合
- **开机自启** - 系统启动时自动运行
- **通知设置** - 开启/关闭操作提示
- **状态查看** - 查看使用统计

### 命令行模式

```bash
# 切换桌面图标状态
ToggleDesktop.exe --toggle

# 显示桌面图标
ToggleDesktop.exe --show

# 隐藏桌面图标
ToggleDesktop.exe --hide

# 查看当前状态
ToggleDesktop.exe --status

# 显示帮助
ToggleDesktop.exe --help
```

---

## 🎨 使用场景

### 场景 1：欣赏壁纸
```
工作中 → 按 Ctrl+Alt+K → 桌面自动显示 → 图标隐藏 → 纯净壁纸
```

### 场景 2：截图分享
```
准备截图 → 快速按热键 → 获得无图标的干净桌面 → 完美截图
```

### 场景 3：收藏壁纸
```
发现喜欢的壁纸 → 右键菜单收藏 → 自动保存到本地 → 建立壁纸库
```

---

## 🛠️ 开发

### 技术栈

- **框架**: .NET 8.0 / Windows Forms
- **语言**: C# 12
- **架构**: 单例模式 + 事件驱动
- **核心技术**:
  - Windows API (FindWindow, ShowWindow)
  - Shell.Application COM 接口
  - 全局热键注册 (RegisterHotKey)
  - HTTP 异步请求 (HttpClient)

### 项目结构

```
ToggleDesktop/
├── Core/                      # 核心业务逻辑
│   ├── DesktopIconManager.cs  # 桌面图标管理
│   ├── HotKeyManager.cs       # 全局热键管理
│   ├── SettingsManager.cs     # 设置管理
│   └── BingWallpaperManager.cs # 壁纸下载管理
├── UI/                        # 用户界面
│   ├── TrayManager.cs         # 托盘管理
│   ├── SettingsForm.cs        # 设置窗口
│   ├── HotKeySelector.cs      # 热键选择器
│   └── IconHelper.cs          # 图标辅助
├── Utils/                     # 工具类
│   ├── WindowsApiHelper.cs    # Windows API 封装
│   ├── AutoStartManager.cs    # 开机自启管理
│   └── ResourceOptimizer.cs   # 资源优化
├── Test/                      # 测试代码
└── Program.cs                 # 程序入口
```

### 本地开发

```bash
# 克隆项目
git clone https://github.com/u9-hbzhang/ToggleDesktop.git
cd ToggleDesktop

# 编译项目
dotnet build

# 运行项目
dotnet run

# 发布单文件版本
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### 运行测试

```bash
# 基础功能测试
ToggleDesktop.exe --test

# 性能测试
ToggleDesktop.exe --performance

# 热键测试
ToggleDesktop.exe --hotkey-test
```

---

## 🤝 贡献

欢迎贡献代码、报告问题或提出建议！

### 如何贡献

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 报告问题

请通过 [Issues](https://github.com/u9-hbzhang/ToggleDesktop/issues) 报告问题，包含：
- 操作系统版本
- 重现步骤
- 预期行为
- 实际行为
- 截图（如果有）

---

## 📝 更新日志

查看 [CHANGELOG.md](CHANGELOG.md) 了解版本历史和更新内容。

### 最新版本 v1.1.0 (2024-10-02)

- ✨ 新增智能桌面显示功能
- ✨ 新增必应壁纸收藏功能
- 🔧 优化热键响应逻辑
- 🐛 修复桌面检测问题

---

## ❓ 常见问题

### Q: 热键不起作用？
**A:** 检查是否被其他程序占用。可以在设置中更换其他快捷键。

### Q: 壁纸下载失败？
**A:** 检查网络连接。确认防火墙未阻止程序访问网络。

### Q: 如何卸载？
**A:** 
1. 右键托盘图标 → 退出
2. （可选）取消开机自启动
3. 删除程序文件

### Q: 支持 Windows 7/8 吗？
**A:** 理论上支持，但未经测试。建议使用 Windows 10/11。

---

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

---

## 🙏 致谢

- 必应每日壁纸 API
- Windows Shell API
- .NET 社区

---

## 📧 联系方式

- 作者：[HBZhang]
- 邮箱：u9_hbzhang@outlook.com
- 主页：[https://github.com/u9-hbzhang](https://github.com/u9-hbzhang)

---

<div align="center">

**如果这个项目对你有帮助，请给个 ⭐️ Star 支持一下！**

Made with ❤️ by [HBZhang]

</div>
