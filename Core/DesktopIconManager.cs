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
        private const int STATE_SYNC_TIMEOUT_MS = 1000;
        private const int STATE_SYNC_INTERVAL_MS = 50;

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
                RefreshDesktopWindows();
                return SetDesktopIconsHiddenInternal(!_isHidden);
            }
        }

        /// <summary>
        /// 显示桌面图标
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool ShowDesktopIcons()
        {
            lock (_lock)
            {
                RefreshDesktopWindows();
                return SetDesktopIconsHiddenInternal(false);
            }
        }

        /// <summary>
        /// 隐藏桌面图标
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool HideDesktopIcons()
        {
            lock (_lock)
            {
                RefreshDesktopWindows();
                return SetDesktopIconsHiddenInternal(true);
            }
        }

        /// <summary>
        /// 刷新桌面窗口句柄
        /// </summary>
        public void RefreshDesktopWindows()
        {
            RefreshDesktopWindows(false);
        }

        /// <summary>
        /// 刷新桌面窗口句柄，并可选在状态变化时触发事件。
        /// </summary>
        /// <param name="notifyIfStateChanged">状态变化时是否触发事件</param>
        /// <returns>状态是否发生变化</returns>
        public bool RefreshDesktopWindows(bool notifyIfStateChanged)
        {
            lock (_lock)
            {
                bool previousHiddenState = _isHidden;
                RefreshDesktopWindowsCore();

                bool changed = previousHiddenState != _isHidden;
                if (notifyIfStateChanged && changed)
                {
                    OnDesktopIconsStateChanged?.Invoke(_isHidden);
                }

                return changed;
            }
        }

        /// <summary>
        /// 仅通过注册表同步“显示桌面图标”状态（轻量探测）。
        /// </summary>
        /// <param name="notifyIfStateChanged">状态变化时是否触发事件</param>
        /// <returns>状态是否发生变化</returns>
        public bool RefreshDesktopStateFromRegistry(bool notifyIfStateChanged)
        {
            lock (_lock)
            {
                bool previousHiddenState = _isHidden;
                bool? registryHiddenState = WindowsApiHelper.TryGetDesktopIconsHiddenFromRegistry();
                if (registryHiddenState.HasValue)
                {
                    _isHidden = registryHiddenState.Value;
                }
                else
                {
                    // 注册表读取失败时回退到窗口探测，保证状态可恢复
                    RefreshDesktopWindowsCore();
                }

                bool changed = previousHiddenState != _isHidden;
                if (notifyIfStateChanged && changed)
                {
                    OnDesktopIconsStateChanged?.Invoke(_isHidden);
                }

                return changed;
            }
        }

        /// <summary>
        /// 刷新桌面窗口句柄（内部实现）。
        /// </summary>
        private void RefreshDesktopWindowsCore()
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
        /// 使用系统命令设置桌面图标显示状态。
        /// </summary>
        private bool SetDesktopIconsHiddenInternal(bool hidden)
        {
            try
            {
                if (!EnsureDesktopWindowsReady())
                {
                    System.Diagnostics.Debug.WriteLine("未找到桌面图标窗口，将尝试使用 Progman 兜底命令");
                }

                bool previousHiddenState = _isHidden;
                if (previousHiddenState == hidden)
                {
                    return true;
                }

                var commandTargets = GetCommandTargetWindows().ToList();
                if (commandTargets.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("未找到可发送切换命令的桌面窗口");
                    return false;
                }

                bool success = false;
                foreach (var window in commandTargets)
                {
                    if (!WindowsApiHelper.IsValidHandle(window))
                    {
                        continue;
                    }

                    string className = WindowsApiHelper.GetWindowClassName(window);
                    System.Diagnostics.Debug.WriteLine(
                        $"向窗口发送图标切换命令: {className} (0x{window:X8}), 目标隐藏: {hidden}");

                    WindowsApiHelper.SendMessage(
                        window,
                        WindowsApiHelper.WM_COMMAND,
                        new IntPtr(WindowsApiHelper.CMD_TOGGLE_DESKTOP_ICONS),
                        IntPtr.Zero);

                    if (WaitForHiddenState(hidden))
                    {
                        success = true;
                        break;
                    }

                    RefreshDesktopWindows();
                }

                if (success)
                {
                    if (_isHidden != previousHiddenState)
                    {
                        OnDesktopIconsStateChanged?.Invoke(_isHidden);
                    }

                    System.Diagnostics.Debug.WriteLine($"桌面图标状态已切换为: {(_isHidden ? "隐藏" : "显示")}");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"桌面图标切换后状态未达到预期。目标隐藏: {hidden}, 实际隐藏: {_isHidden}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置桌面图标状态失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 等待桌面图标状态与系统同步。
        /// </summary>
        private bool WaitForHiddenState(bool expectedHidden, int timeoutMs = STATE_SYNC_TIMEOUT_MS)
        {
            int elapsed = 0;
            while (elapsed <= timeoutMs)
            {
                RefreshDesktopWindows();
                if (_isHidden == expectedHidden)
                {
                    return true;
                }

                Thread.Sleep(STATE_SYNC_INTERVAL_MS);
                elapsed += STATE_SYNC_INTERVAL_MS;
            }

            return false;
        }

        /// <summary>
        /// 获取用于发送系统切换命令的窗口集合。
        /// </summary>
        private IEnumerable<IntPtr> GetCommandTargetWindows()
        {
            var targets = _desktopIconWindows
                .Where(WindowsApiHelper.IsValidHandle)
                .Where(h => WindowsApiHelper.GetWindowClassName(h) == WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS)
                .Distinct()
                .OrderByDescending(WindowsApiHelper.IsWindowVisible)
                .ToList();

            IntPtr progman = WindowsApiHelper.FindWindow(WindowsApiHelper.PROGMAN_CLASS, null);
            if (WindowsApiHelper.IsValidHandle(progman) && !targets.Contains(progman))
            {
                targets.Add(progman);
            }

            return targets;
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
                bool? hiddenState = WindowsApiHelper.TryGetDesktopIconsHiddenFromRegistry();
                _isHidden = hiddenState ?? false;
                return;
            }

            bool anyVisible = probeWindows.Any(WindowsApiHelper.IsWindowVisible);
            _isHidden = !anyVisible;
        }

        #endregion
    }
}
