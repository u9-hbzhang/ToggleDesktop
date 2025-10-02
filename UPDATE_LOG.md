# ToggleDesktop 更新日志

## 版本 1.1.0 (2025-10-01)

### 新增功能

#### 1. 智能桌面显示
- **功能描述**: 按下切换热键时，如果Windows桌面不在前台，程序会自动先显示桌面（模拟Win+D），然后再执行图标切换操作
- **实现位置**: 
  - `Utils/WindowsApiHelper.cs`: 新增了桌面检测和显示方法
  - `UI/TrayManager.cs`: 修改了热键回调逻辑
- **使用场景**: 当您打开其他窗口时按下热键，程序会先最小化所有窗口显示桌面，然后切换图标状态，让您更方便地欣赏壁纸

#### 2. 必应壁纸收藏功能
- **功能描述**: 可以通过托盘菜单一键下载并保存当前的必应每日壁纸
- **实现位置**:
  - `Core/BingWallpaperManager.cs`: 新增壁纸管理器类
  - `UI/TrayManager.cs`: 新增两个菜单项
    - "收藏必应壁纸": 下载当前必应壁纸
    - "打开壁纸文件夹": 打开壁纸保存目录
- **保存位置**: `我的图片\BingWallpapers\`
- **文件命名**: `YYYYMMDD_壁纸标题.jpg`

### 技术细节

#### 桌面检测
```csharp
// 检查桌面是否为前台窗口
public static bool IsDesktopForeground()
{
    IntPtr foregroundWindow = GetForegroundWindow();
    IntPtr shellWindow = GetShellWindow();
    IntPtr progman = FindWindow(PROGMAN_CLASS, null);

    return foregroundWindow == shellWindow || 
           foregroundWindow == progman ||
           foregroundWindow == IntPtr.Zero;
}
```

#### 显示桌面
```csharp
// 模拟Win+D快捷键
public static void ShowDesktop()
{
    keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
    keybd_event(VK_D, 0, 0, UIntPtr.Zero);
    keybd_event(VK_D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    System.Threading.Thread.Sleep(100);
}
```

#### 必应壁纸API
- **API地址**: `https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN`
- **返回格式**: JSON，包含壁纸URL、标题和版权信息
- **下载逻辑**: 异步下载，自动去重文件名

### 使用方法

1. **智能桌面显示**: 无需额外操作，按下设置的热键即可（默认Ctrl+Alt+D）
2. **收藏壁纸**: 
   - 右键点击系统托盘图标
   - 选择"收藏必应壁纸"
   - 等待下载完成的提示
   - 可通过"打开壁纸文件夹"查看已保存的壁纸

### 已知问题
- 无

### 下一步计划
- 添加壁纸自动下载功能（每日定时）
- 添加壁纸历史记录查看
- 支持更多壁纸源（如Unsplash等）

