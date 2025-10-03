# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-10-02

### Added
- **智能桌面显示**: 按下热键时，如果桌面不在前台，自动先显示桌面再切换图标状态
- **必应壁纸收藏**: 一键下载并保存必应每日壁纸到本地
  - 右键菜单新增"收藏必应壁纸"选项
  - 右键菜单新增"打开壁纸文件夹"选项
  - 自动保存到 `我的图片\BingWallpapers\` 文件夹
  - 智能文件命名：`YYYYMMDD_壁纸标题.jpg`
- 使用 Shell.Application COM 接口实现桌面显示（更可靠）

### Changed
- 优化热键响应逻辑，提升用户体验
- 改进桌面检测算法

### Technical Details
- 新增 `BingWallpaperManager.cs` - 壁纸下载和管理
- 新增 `IsDesktopForeground()` - 检测桌面是否为前台窗口
- 新增 `ShowDesktop()` - 使用 Shell COM 接口显示桌面

## [1.0.0] - 2025-08-08

### Added
- 一键切换桌面图标显示/隐藏
- 全局热键支持（默认 Ctrl+Alt+K）
- 系统托盘驻留
- 完整的设置界面
- 热键自定义录制
- 开机自启动管理
- 命令行支持
- 多线程安全设计
- 性能优化和资源管理
- 支持 Windows 10/11

### Features
- 动态托盘图标状态指示
- 智能通知系统
- 使用统计
- 命令行测试工具
- 性能测试套件

[1.1.0]: https://github.com/u9-hbzhang/ToggleDesktop/releases/tag/v1.1.0
[1.0.0]: https://github.com/u9-hbzhang/ToggleDesktop/releases/tag/v1.0.0

