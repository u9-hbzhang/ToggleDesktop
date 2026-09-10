using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToggleDesktop.Utils;

namespace ToggleDesktop.Core
{
    /// <summary>
    /// 桌面图标管理器，负责控制桌面图标的显示和隐藏
    /// </summary>
    public class DesktopIconManager
    {
        #region 私有字段

        private static DesktopIconManager? _instance;
        private readonly object _lock = new object();
        private List<IntPtr> _desktopIconWindows = new List<IntPtr>();
        private bool _isHidden = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 单例实例
        /// </summary>
        public static DesktopIconManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DesktopIconManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 桌面图标当前是否隐藏
        /// </summary>
        public bool IsHidden
        {
            get { return _isHidden; }
            private set { _isHidden = value; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，实现单例模式
        /// </summary>
        private DesktopIconManager()
        {
            RefreshDesktopWindows();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 切换桌面图标的显示状态
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool ToggleDesktopIcons()
        {
            lock (_lock)
            {
                try
                {
                    if (!EnsureDesktopWindowsReady())
                    {
                        System.Diagnostics.Debug.WriteLine("未找到桌面图标窗口");
                        return false;
                    }

                    bool previousHiddenState = _isHidden;
                    bool targetVisible = _isHidden;
                    int targetState = targetVisible ? WindowsApiHelper.SW_SHOW : WindowsApiHelper.SW_HIDE;
                    string action = targetVisible ? "显示" : "隐藏";

                    System.Diagnostics.Debug.WriteLine($"准备{action}桌面图标，目标窗口数量: {_desktopIconWindows.Count}");

                    foreach (var window in _desktopIconWindows)
                    {
                        if (WindowsApiHelper.IsValidHandle(window))
                        {
                            string className = WindowsApiHelper.GetWindowClassName(window);
                            bool wasVisible = WindowsApiHelper.IsWindowVisible(window);
                            
                            System.Diagnostics.Debug.WriteLine($"处理窗口: {className} (0x{window:X8}), 当前可见: {wasVisible}");
                            
                            // 返回值并不代表成功与否，而是窗口以前的显示状态
                            bool result = WindowsApiHelper.ShowWindow(window, targetState);
                            bool nowVisible = WindowsApiHelper.IsWindowVisible(window);
                            
                            System.Diagnostics.Debug.WriteLine($"ShowWindow结果: {result}, 现在可见: {nowVisible}");
                        }
                    }

                    // 等待资源管理器刷新窗口状态
                    Thread.Sleep(60);
                    RefreshDesktopWindows();
                    bool success = _desktopIconWindows.Count > 0 && _isHidden == !targetVisible;

                    if (success)
                    {
                        bool synced = WindowsApiHelper.TrySetDesktopIconsHiddenInRegistry(_isHidden);
                        if (!synced)
                        {
                            System.Diagnostics.Debug.WriteLine("已切换窗口状态，但同步 HideIcons 注册表失败");
                        }

                        if (_isHidden != previousHiddenState)
                        {
                            OnDesktopIconsStateChanged?.Invoke(_isHidden);
                        }
                        System.Diagnostics.Debug.WriteLine($"桌面图标状态已切换为: {(_isHidden ? "隐藏" : "显示")}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"桌面图标切换后状态未达到预期。预期可见: {targetVisible}, 实际隐藏: {_isHidden}");
                    }

                    return success;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"切换桌面图标状态失败: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 显示桌面图标
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool ShowDesktopIcons()
        {
            RefreshDesktopWindows();
            if (!_isHidden)
            {
                // 显式“显示”请求即便无需切换，也应确保重启后状态一致
                WindowsApiHelper.TrySetDesktopIconsHiddenInRegistry(false);
                return true;
            }

            return ToggleDesktopIcons();
        }

        /// <summary>
        /// 隐藏桌面图标
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool HideDesktopIcons()
        {
            RefreshDesktopWindows();
            if (_isHidden)
            {
                // 显式“隐藏”请求即便无需切换，也应确保重启后状态一致
                WindowsApiHelper.TrySetDesktopIconsHiddenInRegistry(true);
                return true;
            }

            return ToggleDesktopIcons();
        }

        /// <summary>
        /// 刷新桌面窗口句柄
        /// </summary>
        public void RefreshDesktopWindows()
        {
            _desktopIconWindows.Clear();
            
            try
            {
                // 方法1: 查找标准的Progman窗口
                var progmanWindows = FindProgmanWindows();
                _desktopIconWindows.AddRange(progmanWindows);

                // 方法2: 查找WorkerW窗口 (Windows 10/11可能需要)
                var workerWWindows = FindWorkerWWindows();
                _desktopIconWindows.AddRange(workerWWindows);

                // 去重
                _desktopIconWindows = _desktopIconWindows.Distinct().Where(h => h != IntPtr.Zero).ToList();

                // 检查当前状态
                UpdateCurrentState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"刷新桌面窗口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取桌面图标窗口数量
        /// </summary>
        /// <returns>窗口数量</returns>
        public int GetDesktopWindowCount()
        {
            return _desktopIconWindows.Count;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 桌面图标状态改变事件
        /// </summary>
        public event Action<bool>? OnDesktopIconsStateChanged;

        #endregion

        #region 私有方法

        /// <summary>
        /// 确保桌面图标窗口已就绪（重启后首次调用时可容错）
        /// </summary>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelayMs">重试间隔（毫秒）</param>
        /// <returns>是否找到可用窗口</returns>
        private bool EnsureDesktopWindowsReady(int maxRetries = 2, int retryDelayMs = 120)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                RefreshDesktopWindows();
                if (_desktopIconWindows.Count > 0)
                {
                    return true;
                }

                if (attempt < maxRetries)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"未检测到桌面图标窗口，准备重试 {attempt + 1}/{maxRetries}");
                    Thread.Sleep(retryDelayMs);
                }
            }

            return false;
        }

        /// <summary>
        /// 获取用于状态判定的窗口集合
        /// </summary>
        private IEnumerable<IntPtr> GetStateProbeWindows()
        {
            var validWindows = _desktopIconWindows
                .Where(WindowsApiHelper.IsValidHandle)
                .ToList();

            if (validWindows.Count == 0)
            {
                return Enumerable.Empty<IntPtr>();
            }

            var listViewWindows = validWindows
                .Where(h => WindowsApiHelper.GetWindowClassName(h) == WindowsApiHelper.SYSLISTVIEW32_CLASS)
                .ToList();

            if (listViewWindows.Count > 0)
            {
                return listViewWindows;
            }

            var shellViewWindows = validWindows
                .Where(h => WindowsApiHelper.GetWindowClassName(h) == WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS)
                .ToList();

            if (shellViewWindows.Count > 0)
            {
                return shellViewWindows;
            }

            return validWindows;
        }

        /// <summary>
        /// 查找Progman窗口中的桌面图标窗口
        /// </summary>
        /// <returns>桌面图标窗口句柄列表</returns>
        private List<IntPtr> FindProgmanWindows()
        {
            var windows = new List<IntPtr>();

            // 查找Progman窗口
            IntPtr progman = WindowsApiHelper.FindWindow(WindowsApiHelper.PROGMAN_CLASS, null);
            if (progman != IntPtr.Zero)
            {
                // 查找SHELLDLL_DefView窗口
                IntPtr shellView = WindowsApiHelper.FindWindowEx(progman, IntPtr.Zero, 
                    WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS, null);
                
                if (shellView != IntPtr.Zero)
                {
                    windows.Add(shellView);
                    
                    // 查找SysListView32窗口 (实际的图标列表)
                    IntPtr listView = WindowsApiHelper.FindWindowEx(shellView, IntPtr.Zero,
                        WindowsApiHelper.SYSLISTVIEW32_CLASS, null);
                    
                    if (listView != IntPtr.Zero)
                    {
                        windows.Add(listView);
                    }
                }
            }

            return windows;
        }

        /// <summary>
        /// 查找WorkerW窗口中的桌面图标窗口 (Windows 10/11)
        /// </summary>
        /// <returns>桌面图标窗口句柄列表</returns>
        private List<IntPtr> FindWorkerWWindows()
        {
            var windows = new List<IntPtr>();

            // 发送消息创建WorkerW窗口
            IntPtr progman = WindowsApiHelper.FindWindow(WindowsApiHelper.PROGMAN_CLASS, null);
            if (progman != IntPtr.Zero)
            {
                WindowsApiHelper.SendMessage(progman, 0x052C, IntPtr.Zero, IntPtr.Zero);
            }

            // 枚举所有WorkerW窗口
            var workerWWindows = new List<IntPtr>();
            WindowsApiHelper.EnumChildWindows(IntPtr.Zero, (hWnd, lParam) =>
            {
                string className = WindowsApiHelper.GetWindowClassName(hWnd);
                if (className == "WorkerW")
                {
                    workerWWindows.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            // 在每个WorkerW窗口中查找SHELLDLL_DefView
            foreach (var workerW in workerWWindows)
            {
                IntPtr shellView = WindowsApiHelper.FindWindowEx(workerW, IntPtr.Zero,
                    WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS, null);
                
                if (shellView != IntPtr.Zero)
                {
                    windows.Add(shellView);
                    
                    IntPtr listView = WindowsApiHelper.FindWindowEx(shellView, IntPtr.Zero,
                        WindowsApiHelper.SYSLISTVIEW32_CLASS, null);
                    
                    if (listView != IntPtr.Zero)
                    {
                        windows.Add(listView);
                    }
                }
            }

            return windows;
        }

        /// <summary>
        /// 更新当前桌面图标状态
        /// </summary>
        private void UpdateCurrentState()
        {
            var probeWindows = GetStateProbeWindows().ToList();
            if (probeWindows.Count == 0)
            {
                _isHidden = false;
                return;
            }

            bool anyVisible = probeWindows.Any(WindowsApiHelper.IsWindowVisible);
            _isHidden = !anyVisible;
        }

        #endregion
    }
}
