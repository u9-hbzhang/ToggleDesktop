# ToggleDesktop 部署指南

## 📦 发布版本

### 系统要求
- **操作系统：** Windows 10 版本 1809 或更高版本
- **架构：** x64 (64位)
- **内存：** 最少 50MB 可用内存
- **磁盘空间：** 约 1MB

### 部署文件
- **发布路径：** `bin/Release/net8.0-windows7.0/win-x64/publish/`
- **主要文件：** `ToggleDesktop.exe` 
- **配置文件：** 运行时自动在 `%APPDATA%/ToggleDesktop/` 创建

## 🚀 安装说明

### 方式一：直接运行（推荐）
1. 从发布目录复制 `ToggleDesktop.exe等` 到任意位置
2. 右键文件 → 属性 → 解除阻止（如果有的话）
3. 双击运行 `ToggleDesktop.exe`
4. 程序将在系统托盘中启动

### 方式二：便携版部署
1. 创建文件夹 `ToggleDesktop`
2. 将 `ToggleDesktop.exe等` 放入文件夹
3. 可选：创建桌面快捷方式
4. 设置中可配置开机自启动

## ⚙️ 首次使用配置

### 基本设置
1. **启动程序**：双击 `ToggleDesktop.exe`
2. **查看托盘图标**：程序启动后在系统托盘显示图标
3. **快速测试**：双击托盘图标测试桌面图标切换
4. **设置热键**：右键托盘图标 → 设置 → 配置全局热键

### 推荐配置
```
✅ 开机自动启动：启用
✅ 显示通知消息：启用  
⌨️ 全局热键：Ctrl+Alt+K（默认，可自定义）
```

## 🔧 命令行使用

```bash
# 基本操作
ToggleDesktop.exe --toggle        # 切换桌面图标
ToggleDesktop.exe --show          # 显示桌面图标
ToggleDesktop.exe --hide          # 隐藏桌面图标
ToggleDesktop.exe --status        # 查看状态

# 测试和诊断
ToggleDesktop.exe --test          # 运行功能测试
ToggleDesktop.exe --performance   # 性能测试
ToggleDesktop.exe --debug         # 调试模式

# 帮助
ToggleDesktop.exe --help          # 显示帮助
```

## 📁 文件结构

```
ToggleDesktop/
├── ToggleDesktop.exe              # 主程序（单文件）
└── %APPDATA%/ToggleDesktop/       # 配置目录
    ├── settings.json              # 用户设置
    └── logs/                       # 日志文件（如果有）
```

## 🛠️ 故障排除

### 常见问题

**1. 程序无法启动**
- 检查是否解除了文件阻止
- 以管理员身份运行
- 检查 .NET 8.0 运行时

**2. 桌面图标切换无效**
- 检查是否有足够权限
- 重启程序尝试
- 运行 `ToggleDesktop.exe --debug` 查看详细信息

**3. 热键无法注册**
- 热键可能被其他程序占用
- 在设置中更换热键组合
- 检查是否有安全软件阻止

**4. 开机自启动失败**
- 需要管理员权限设置注册表
- 手动添加到启动文件夹：`Win+R` → `shell:startup`

### 性能优化

**内存使用：**
- 正常运行约 25-50MB 内存
- 支持长时间运行不内存泄漏
- 自动垃圾回收优化

**CPU 使用：**
- 待机状态 CPU 使用率 < 1%
- 切换操作响应时间 < 100ms
- 支持高频率操作

## 🔄 升级说明

### 升级步骤
1. 关闭当前运行的程序
2. 备份设置文件（可选）
3. 替换 `ToggleDesktop.exe`
4. 重新启动程序

### 设置迁移
- 设置文件自动保留在 `%APPDATA%/ToggleDesktop/`
- 升级后设置自动继承
- 如需重置：删除配置目录

## 🗑️ 卸载说明

### 完全卸载
1. 关闭程序：右键托盘图标 → 退出
2. 删除程序文件：`ToggleDesktop.exe`
3. 删除配置文件：`%APPDATA%/ToggleDesktop/`
4. 清理自启动：如果设置了开机自启动，会自动清理注册表

### 保留设置卸载
1. 关闭程序
2. 仅删除 `ToggleDesktop.exe`
3. 保留配置目录以备将来使用

## 📞 技术支持

### 系统兼容性
- ✅ Windows 10 (1809+)
- ✅ Windows 11
- ✅ 多显示器环境
- ✅ 高DPI屏幕

### 已知限制
- 需要桌面窗口可访问权限
- 部分安全软件可能误报
- 极少数系统配置可能不兼容

### 性能指标
- **启动时间：** < 3秒
- **内存占用：** 25-50MB
- **响应时间：** < 100ms
- **热键延迟：** < 50ms

---

**版本：** v1.0.0  
**更新时间：** 2024年12月  
**技术支持：** 通过项目Issues页面
