# ToggleDesktop

一键切换Windows桌面图标显示/隐藏的工具，让您更好地欣赏精美壁纸。

## 功能特性

### 核心功能
- ✅ 一键切换桌面图标显示/隐藏
- ✅ 全局热键支持（默认 Ctrl+Alt+D）
- ✅ **智能桌面显示** - 热键按下时自动显示桌面（如果未在前台）
- ✅ **必应壁纸收藏** - 一键下载并保存必应每日壁纸
- ✅ 系统托盘驻留，右键菜单操作
- ✅ 支持命令行操作
- ✅ 自动检测桌面窗口状态
- ✅ 支持Windows 10/11桌面机制

### 用户界面
- 🎨 动态托盘图标状态指示
- ⚙️ 完整的设置界面
- 🔧 热键录制和自定义
- 💬 智能通知系统
- 📊 使用统计和状态监控

### 系统集成
- 🚀 开机自启动管理
- 🔐 注册表安全访问
- 💾 配置文件自动保存
- 🛡️ 多线程安全设计
- ⚡ 性能优化和资源管理

## 使用方法

### GUI模式
直接运行 `ToggleDesktop.exe`，程序将在系统托盘中运行：
- **双击托盘图标**：切换桌面图标显示状态
- **右键托盘图标**：查看菜单选项
  - 显示/隐藏桌面图标
  - **收藏必应壁纸** - 下载当前必应每日壁纸到"我的图片\BingWallpapers"
  - **打开壁纸文件夹** - 快速访问已保存的壁纸
  - 设置和关于
- **热键操作**：按下设置的热键（默认Ctrl+Alt+D）
  - 如果桌面不在前台，自动先显示桌面（类似Win+D）
  - 然后切换桌面图标显示状态

### 命令行模式
```bash
# 基本操作
ToggleDesktop.exe --toggle        # 切换桌面图标显示/隐藏状态
ToggleDesktop.exe --show          # 显示桌面图标
ToggleDesktop.exe --hide          # 隐藏桌面图标
ToggleDesktop.exe --status        # 查看当前状态

# 测试功能
ToggleDesktop.exe --test          # 运行核心功能测试
ToggleDesktop.exe --performance   # 运行性能测试
ToggleDesktop.exe --stress        # 运行压力测试
ToggleDesktop.exe --debug         # 调试桌面窗口查找

# 帮助信息
ToggleDesktop.exe --help          # 显示帮助信息
```

## 系统要求

- Windows 10 或更高版本
- .NET 6.0 Runtime（程序已包含）

## 技术实现

本程序通过以下Windows API实现桌面图标控制：
- `FindWindow` - 查找桌面窗口
- `FindWindowEx` - 查找桌面图标容器窗口
- `ShowWindow` - 控制窗口显示/隐藏
- `IsWindowVisible` - 检查窗口可见状态
- `GetForegroundWindow` - 获取前台窗口
- `keybd_event` - 模拟键盘输入（显示桌面）

支持两种桌面机制：
1. 传统Progman窗口机制
2. Windows 10/11的WorkerW窗口机制

必应壁纸功能：
- 使用必应官方API获取每日壁纸信息
- 异步下载高质量壁纸图片
- 自动文件命名和去重

## 编译说明

```bash
# 开发模式编译
dotnet build

# 发布单文件版本
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## 版本历史

### v1.1.0 (当前版本)
- ✨ 新增智能桌面显示功能：热键触发时自动显示桌面
- ✨ 新增必应壁纸收藏功能：一键下载保存每日壁纸
- ✨ 新增壁纸文件夹快速访问
- 🔧 优化热键响应逻辑

### v1.0.0
- 实现基础的桌面图标切换功能
- 支持系统托盘操作
- 支持命令行参数
- 兼容Windows 10/11

## 许可证

此项目采用 MIT 许可证。

## 问题反馈

如果您遇到问题或有建议，请提交Issue。
