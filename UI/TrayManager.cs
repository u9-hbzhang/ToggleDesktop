using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ToggleDesktop.Core;
using ToggleDesktop.Utils;

namespace ToggleDesktop.UI
{
    /// <summary>
    /// 系统托盘管理器
    /// </summary>
    public class TrayManager : IDisposable
    {
        #region 私有字段

        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;
        private DesktopIconManager _iconManager;
        private SettingsManager _settingsManager;
        private HotKeyManager _hotKeyManager;
        private BingWallpaperManager _bingWallpaperManager;
        private SettingsForm? _settingsForm;
        
        // 菜单项
        private ToolStripMenuItem? _toggleMenuItem;
        private ToolStripMenuItem? _statusMenuItem;
        private ToolStripMenuItem? _saveWallpaperMenuItem;
        private ToolStripMenuItem? _openWallpaperFolderMenuItem;
        private ToolStripMenuItem? _settingsMenuItem;
        private ToolStripMenuItem? _aboutMenuItem;
        private ToolStripMenuItem? _exitMenuItem;

        #endregion

        #region 事件

        /// <summary>
        /// 请求退出应用程序事件
        /// </summary>
        public event EventHandler? ExitRequested;

        #endregion

        #region 构造函数

        public TrayManager()
        {
            _iconManager = DesktopIconManager.Instance;
            _settingsManager = SettingsManager.Instance;
            _hotKeyManager = HotKeyManager.Instance;
            _bingWallpaperManager = BingWallpaperManager.Instance;
            
            InitializeTrayIcon();
            InitializeTrayMenu();
            InitializeHotKeys();
            
            // 订阅状态变化事件
            _iconManager.OnDesktopIconsStateChanged += OnDesktopIconStateChanged;
            
            // 初始化图标状态
            UpdateTrayIcon();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示气球提示
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="text">内容</param>
        /// <param name="icon">图标类型</param>
        /// <param name="timeout">显示时间（毫秒）</param>
        public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 2000)
        {
            if (_settingsManager.ShowNotifications && _trayIcon != null)
            {
                _trayIcon.ShowBalloonTip(timeout, title, text, icon);
            }
        }

        /// <summary>
        /// 显示设置窗口
        /// </summary>
        public void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm();
                _settingsForm.OnHotKeyChanged += OnSettingsHotKeyChanged;
            }
            
            if (!_settingsForm.Visible)
            {
                _settingsForm.Show();
                _settingsForm.BringToFront();
            }
            else
            {
                _settingsForm.BringToFront();
            }
        }

        /// <summary>
        /// 切换桌面图标状态
        /// </summary>
        public void ToggleDesktopIcons()
        {
            bool success = _iconManager.ToggleDesktopIcons();
            if (success)
            {
                _settingsManager.IncrementToggleCount();
                
                string message = _iconManager.IsHidden ? "桌面图标已隐藏" : "桌面图标已显示";
                ShowBalloonTip("ToggleDesktop", message);
            }
            else
            {
                ShowBalloonTip("ToggleDesktop", "操作失败，请检查权限或重新启动程序", ToolTipIcon.Warning);
            }
        }

        /// <summary>
        /// 热键触发的切换桌面图标（带桌面显示检查）
        /// </summary>
        public void ToggleDesktopIconsWithCheck()
        {
            // 检查桌面是否为前台窗口
            if (!WindowsApiHelper.IsDesktopForeground())
            {
                System.Diagnostics.Debug.WriteLine("桌面不在前台，先显示桌面");
                // 先显示桌面
                WindowsApiHelper.ShowDesktop();
            }
            
            // 然后切换图标
            ToggleDesktopIcons();
        }

        /// <summary>
        /// 重新注册热键
        /// </summary>
        public void RefreshHotKeys()
        {
            InitializeHotKeys();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化托盘图标
        /// </summary>
        private void InitializeTrayIcon()
        {
            _trayIcon = new NotifyIcon()
            {
                Icon = IconHelper.CreateTrayIcon(_iconManager.IsHidden),
                Text = GetTrayIconTooltip(),
                Visible = true
            };

            // 绑定事件
            _trayIcon.DoubleClick += TrayIcon_DoubleClick;
            _trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
        }

        /// <summary>
        /// 初始化托盘菜单
        /// </summary>
        private void InitializeTrayMenu()
        {
            _trayMenu = new ContextMenuStrip();

            // 切换功能菜单项
            _toggleMenuItem = new ToolStripMenuItem()
            {
                Text = GetToggleMenuText(),
                Font = new Font(_trayMenu.Font, FontStyle.Bold)
            };
            _toggleMenuItem.Click += ToggleMenuItem_Click;

            // 分隔线
            var separator1 = new ToolStripSeparator();

            // 状态信息菜单项
            _statusMenuItem = new ToolStripMenuItem()
            {
                Text = GetStatusMenuText(),
                Enabled = false
            };

            // 分隔线
            var separator2 = new ToolStripSeparator();

            // 收藏壁纸菜单项
            _saveWallpaperMenuItem = new ToolStripMenuItem()
            {
                Text = "收藏必应壁纸(&W)..."
            };
            _saveWallpaperMenuItem.Click += SaveWallpaperMenuItem_Click;

            // 打开壁纸文件夹菜单项
            _openWallpaperFolderMenuItem = new ToolStripMenuItem()
            {
                Text = "打开壁纸文件夹(&F)..."
            };
            _openWallpaperFolderMenuItem.Click += OpenWallpaperFolderMenuItem_Click;

            // 分隔线
            var separator3 = new ToolStripSeparator();

            // 设置菜单项
            _settingsMenuItem = new ToolStripMenuItem()
            {
                Text = "设置(&S)...",
                Image = Properties.Resources.Settings?.ToBitmap() // 可选：添加设置图标
            };
            _settingsMenuItem.Click += SettingsMenuItem_Click;

            // 关于菜单项
            _aboutMenuItem = new ToolStripMenuItem()
            {
                Text = "关于(&A)..."
            };
            _aboutMenuItem.Click += AboutMenuItem_Click;

            // 分隔线
            var separator4 = new ToolStripSeparator();

            // 退出菜单项
            _exitMenuItem = new ToolStripMenuItem()
            {
                Text = "退出(&X)"
            };
            _exitMenuItem.Click += ExitMenuItem_Click;

            // 添加到菜单
            _trayMenu.Items.AddRange(new ToolStripItem[]
            {
                _toggleMenuItem,
                separator1,
                _statusMenuItem,
                separator2,
                _saveWallpaperMenuItem,
                _openWallpaperFolderMenuItem,
                separator3,
                _settingsMenuItem,
                _aboutMenuItem,
                separator4,
                _exitMenuItem
            });

            _trayIcon!.ContextMenuStrip = _trayMenu;
        }

        /// <summary>
        /// 初始化热键
        /// </summary>
        private void InitializeHotKeys()
        {
            try
            {
                // 注销所有现有热键
                _hotKeyManager.UnregisterAllHotKeys();

                // 获取设置中的热键
                string hotKeyString = _settingsManager.HotKey;
                if (!string.IsNullOrWhiteSpace(hotKeyString))
                {
                    if (HotKeyManager.ParseHotKeyString(hotKeyString, out uint modifiers, out Keys key))
                    {
                        bool success = _hotKeyManager.RegisterHotKey("ToggleDesktop", modifiers, key, () =>
                        {
                            // 热键回调：先检查并显示桌面，然后切换桌面图标
                            ToggleDesktopIconsWithCheck();
                        });

                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine($"热键注册成功: {hotKeyString}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"热键注册失败: {hotKeyString}");
                            ShowBalloonTip("ToggleDesktop", 
                                $"热键 {hotKeyString} 注册失败，可能已被其他程序占用", 
                                ToolTipIcon.Warning, 3000);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"热键字符串格式错误: {hotKeyString}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化热键时发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新托盘图标
        /// </summary>
        private void UpdateTrayIcon()
        {
            if (_trayIcon != null)
            {
                // 释放旧图标
                var oldIcon = _trayIcon.Icon;
                
                // 设置新图标
                _trayIcon.Icon = IconHelper.CreateTrayIcon(_iconManager.IsHidden);
                _trayIcon.Text = GetTrayIconTooltip();
                
                // 释放旧图标资源
                oldIcon?.Dispose();
            }

            UpdateMenuItems();
        }

        /// <summary>
        /// 更新菜单项
        /// </summary>
        private void UpdateMenuItems()
        {
            if (_toggleMenuItem != null)
            {
                _toggleMenuItem.Text = GetToggleMenuText();
            }

            if (_statusMenuItem != null)
            {
                _statusMenuItem.Text = GetStatusMenuText();
            }
        }

        /// <summary>
        /// 获取托盘图标提示文本
        /// </summary>
        /// <returns>提示文本</returns>
        private string GetTrayIconTooltip()
        {
            string status = _iconManager.IsHidden ? "隐藏" : "显示";
            return $"ToggleDesktop - 桌面图标: {status}\n双击切换状态";
        }

        /// <summary>
        /// 获取切换菜单文本
        /// </summary>
        /// <returns>菜单文本</returns>
        private string GetToggleMenuText()
        {
            string action = _iconManager.IsHidden ? "显示" : "隐藏";
            string hotKeyText = _settingsManager.HotKey;
            
            // 如果热键为空，不显示热键信息
            if (string.IsNullOrWhiteSpace(hotKeyText))
            {
                return $"{action}桌面图标";
            }
            
            return $"{action}桌面图标 ({hotKeyText})";
        }

        /// <summary>
        /// 获取状态菜单文本
        /// </summary>
        /// <returns>状态文本</returns>
        private string GetStatusMenuText()
        {
            string status = _iconManager.IsHidden ? "隐藏" : "显示";
            int windowCount = _iconManager.GetDesktopWindowCount();
            return $"状态: {status} | 窗口: {windowCount}个";
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 托盘图标双击事件
        /// </summary>
        private void TrayIcon_DoubleClick(object? sender, EventArgs e)
        {
            ToggleDesktopIcons();
        }

        /// <summary>
        /// 气球提示点击事件
        /// </summary>
        private void TrayIcon_BalloonTipClicked(object? sender, EventArgs e)
        {
            // 可以在这里处理气球提示点击事件
        }

        /// <summary>
        /// 切换菜单项点击事件
        /// </summary>
        private void ToggleMenuItem_Click(object? sender, EventArgs e)
        {
            ToggleDesktopIcons();
        }

        /// <summary>
        /// 设置菜单项点击事件
        /// </summary>
        private void SettingsMenuItem_Click(object? sender, EventArgs e)
        {
            ShowSettings();
        }

        /// <summary>
        /// 收藏壁纸菜单项点击事件
        /// </summary>
        private async void SaveWallpaperMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                ShowBalloonTip("ToggleDesktop", "正在获取必应壁纸...", ToolTipIcon.Info, 2000);
                
                string? savedPath = await _bingWallpaperManager.DownloadAndSaveWallpaperAsync();
                
                if (savedPath != null)
                {
                    ShowBalloonTip("ToggleDesktop", 
                        $"壁纸已保存！\n{Path.GetFileName(savedPath)}", 
                        ToolTipIcon.Info, 3000);
                }
                else
                {
                    ShowBalloonTip("ToggleDesktop", 
                        "壁纸保存失败，请检查网络连接", 
                        ToolTipIcon.Warning, 3000);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"收藏壁纸失败: {ex.Message}");
                ShowBalloonTip("ToggleDesktop", 
                    "壁纸保存失败，请稍后重试", 
                    ToolTipIcon.Error, 3000);
            }
        }

        /// <summary>
        /// 打开壁纸文件夹菜单项点击事件
        /// </summary>
        private void OpenWallpaperFolderMenuItem_Click(object? sender, EventArgs e)
        {
            _bingWallpaperManager.OpenSaveFolder();
        }

        /// <summary>
        /// 关于菜单项点击事件
        /// </summary>
        private void AboutMenuItem_Click(object? sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        /// <summary>
        /// 退出菜单项点击事件
        /// </summary>
        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 桌面图标状态变化事件
        /// </summary>
        /// <param name="isHidden">是否隐藏</param>
        private void OnDesktopIconStateChanged(bool isHidden)
        {
            // 直接更新托盘图标（NotifyIcon是线程安全的）
            UpdateTrayIcon();
        }

        /// <summary>
        /// 设置窗口热键更改事件处理
        /// </summary>
        /// <param name="newHotKey">新的热键字符串</param>
        private void OnSettingsHotKeyChanged(string newHotKey)
        {
            RefreshHotKeys();
            UpdateMenuItems(); // 更新托盘菜单显示
        }

        /// <summary>
        /// 显示关于对话框
        /// </summary>
        private void ShowAboutDialog()
        {
            string message = "ToggleDesktop v1.0.0\n\n" +
                           "一键切换Windows桌面图标显示/隐藏的工具\n" +
                           "让您更好地欣赏精美壁纸\n\n" +
                           $"统计信息:\n" +
                           $"• 累计切换次数: {_settingsManager.ToggleCount}\n" +
                           $"• 首次使用时间: {_settingsManager.FirstRunTime:yyyy-MM-dd}\n" +
                           $"• 检测到窗口: {_iconManager.GetDesktopWindowCount()}个\n\n" +
                           "Copyright © 2024 ToggleDesktop";

            MessageBox.Show(message, "关于 ToggleDesktop", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region IDisposable实现

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 取消事件订阅
                _iconManager.OnDesktopIconsStateChanged -= OnDesktopIconStateChanged;
                
                // 注销热键
                _hotKeyManager.UnregisterAllHotKeys();
                
                // 释放资源
                _settingsForm?.Dispose();
                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
            }
        }

        #endregion
    }

    /// <summary>
    /// 占位符资源类（实际项目中应该使用真实的资源文件）
    /// </summary>
    public static class Properties
    {
        public static class Resources
        {
            public static Icon? Settings => null; // 实际项目中应该加载设置图标
        }
    }
}
