using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ToggleDesktop.Core
{
    /// <summary>
    /// 全局热键管理器
    /// </summary>
    public class HotKeyManager : IDisposable
    {
        #region 私有字段

        private static HotKeyManager? _instance;
        private readonly Dictionary<int, HotKeyInfo> _registeredHotKeys = new Dictionary<int, HotKeyInfo>();
        private readonly Dictionary<string, int> _hotKeyNameToId = new Dictionary<string, int>();
        private int _nextHotKeyId = 1000;
        private readonly MessageWindow _messageWindow;

        #endregion

        #region 公共属性

        /// <summary>
        /// 单例实例
        /// </summary>
        public static HotKeyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HotKeyManager();
                }
                return _instance;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，实现单例模式
        /// </summary>
        private HotKeyManager()
        {
            _messageWindow = new MessageWindow(this);
        }

        #endregion

        #region Windows API

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 修饰键常量
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        // 消息常量
        private const int WM_HOTKEY = 0x0312;

        #endregion

        #region 公共方法

        /// <summary>
        /// 注册全局热键
        /// </summary>
        /// <param name="name">热键名称</param>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">主键</param>
        /// <param name="callback">回调函数</param>
        /// <returns>注册是否成功</returns>
        public bool RegisterHotKey(string name, uint modifiers, Keys key, Action callback)
        {
            try
            {
                // 如果已经注册过同名热键，先注销
                if (_hotKeyNameToId.ContainsKey(name))
                {
                    UnregisterHotKey(name);
                }

                int hotKeyId = _nextHotKeyId++;
                uint vkCode = (uint)key;

                System.Diagnostics.Debug.WriteLine($"尝试注册热键: {name} = {GetHotKeyString(modifiers, key)}, 窗口句柄: 0x{_messageWindow.Handle:X8}, ID: {hotKeyId}");
                
                bool success = RegisterHotKey(_messageWindow.Handle, hotKeyId, modifiers, vkCode);
                if (success)
                {
                    var hotKeyInfo = new HotKeyInfo
                    {
                        Id = hotKeyId,
                        Name = name,
                        Modifiers = modifiers,
                        Key = key,
                        Callback = callback
                    };

                    _registeredHotKeys[hotKeyId] = hotKeyInfo;
                    _hotKeyNameToId[name] = hotKeyId;

                    System.Diagnostics.Debug.WriteLine($"✓ 热键注册成功: {name} = {GetHotKeyString(modifiers, key)}, ID: {hotKeyId}");
                    return true;
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"✗ 热键注册失败: {name} = {GetHotKeyString(modifiers, key)}, 错误代码: {error}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"注册热键时发生异常: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="name">热键名称</param>
        /// <returns>注销是否成功</returns>
        public bool UnregisterHotKey(string name)
        {
            try
            {
                if (_hotKeyNameToId.TryGetValue(name, out int hotKeyId))
                {
                    bool success = UnregisterHotKey(_messageWindow.Handle, hotKeyId);
                    if (success)
                    {
                        _registeredHotKeys.Remove(hotKeyId);
                        _hotKeyNameToId.Remove(name);

                        System.Diagnostics.Debug.WriteLine($"热键注销成功: {name}");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"热键注销失败: {name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"注销热键时发生异常: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 检查热键是否已注册
        /// </summary>
        /// <param name="name">热键名称</param>
        /// <returns>是否已注册</returns>
        public bool IsHotKeyRegistered(string name)
        {
            return _hotKeyNameToId.ContainsKey(name);
        }

        /// <summary>
        /// 获取已注册的热键信息
        /// </summary>
        /// <param name="name">热键名称</param>
        /// <returns>热键信息，如果未找到返回null</returns>
        public HotKeyInfo? GetHotKeyInfo(string name)
        {
            if (_hotKeyNameToId.TryGetValue(name, out int hotKeyId))
            {
                return _registeredHotKeys.TryGetValue(hotKeyId, out HotKeyInfo? info) ? info : null;
            }
            return null;
        }

        /// <summary>
        /// 注销所有热键
        /// </summary>
        public void UnregisterAllHotKeys()
        {
            var names = new List<string>(_hotKeyNameToId.Keys);
            foreach (string name in names)
            {
                UnregisterHotKey(name);
            }
        }

        /// <summary>
        /// 测试热键是否可用
        /// </summary>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">主键</param>
        /// <returns>热键是否可用</returns>
        public bool TestHotKey(uint modifiers, Keys key)
        {
            int testId = 9999;
            bool success = RegisterHotKey(_messageWindow.Handle, testId, modifiers, (uint)key);
            if (success)
            {
                UnregisterHotKey(_messageWindow.Handle, testId);
            }
            return success;
        }

        /// <summary>
        /// 检查热键是否已被当前程序注册
        /// </summary>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">主键</param>
        /// <returns>是否已被当前程序注册</returns>
        public bool IsHotKeyRegisteredByThis(uint modifiers, Keys key)
        {
            foreach (var hotKeyInfo in _registeredHotKeys.Values)
            {
                if (hotKeyInfo.Modifiers == modifiers && hotKeyInfo.Key == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 智能测试热键可用性（排除当前程序已注册的热键）
        /// </summary>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">主键</param>
        /// <returns>热键是否可用</returns>
        public bool TestHotKeyAvailability(uint modifiers, Keys key)
        {
            // 如果是当前程序已注册的热键，认为是可用的
            if (IsHotKeyRegisteredByThis(modifiers, key))
            {
                return true;
            }
            
            // 否则进行真实的可用性测试
            return TestHotKey(modifiers, key);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 解析热键字符串
        /// </summary>
        /// <param name="hotKeyString">热键字符串，如"Ctrl+Alt+D"</param>
        /// <param name="modifiers">输出修饰键</param>
        /// <param name="key">输出主键</param>
        /// <returns>解析是否成功</returns>
        public static bool ParseHotKeyString(string hotKeyString, out uint modifiers, out Keys key)
        {
            modifiers = 0;
            key = Keys.None;

            try
            {
                if (string.IsNullOrWhiteSpace(hotKeyString))
                    return false;

                string[] parts = hotKeyString.Split('+');
                if (parts.Length == 0)
                    return false;

                // 解析修饰键
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i].Trim();
                    switch (part.ToLowerInvariant())
                    {
                        case "ctrl":
                        case "control":
                            modifiers |= MOD_CONTROL;
                            break;
                        case "alt":
                            modifiers |= MOD_ALT;
                            break;
                        case "shift":
                            modifiers |= MOD_SHIFT;
                            break;
                        case "win":
                        case "windows":
                            modifiers |= MOD_WIN;
                            break;
                        default:
                            return false;
                    }
                }

                // 解析主键
                string keyString = parts[parts.Length - 1].Trim();
                if (Enum.TryParse<Keys>(keyString, true, out key))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"解析热键字符串时发生异常: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 生成热键字符串
        /// </summary>
        /// <param name="modifiers">修饰键</param>
        /// <param name="key">主键</param>
        /// <returns>热键字符串</returns>
        public static string GetHotKeyString(uint modifiers, Keys key)
        {
            var parts = new List<string>();

            if ((modifiers & MOD_CONTROL) != 0)
                parts.Add("Ctrl");
            if ((modifiers & MOD_ALT) != 0)
                parts.Add("Alt");
            if ((modifiers & MOD_SHIFT) != 0)
                parts.Add("Shift");
            if ((modifiers & MOD_WIN) != 0)
                parts.Add("Win");

            parts.Add(key.ToString());

            return string.Join("+", parts);
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 处理热键消息
        /// </summary>
        /// <param name="hotKeyId">热键ID</param>
        internal void OnHotKeyPressed(int hotKeyId)
        {
            try
            {
                if (_registeredHotKeys.TryGetValue(hotKeyId, out HotKeyInfo? hotKeyInfo))
                {
                    System.Diagnostics.Debug.WriteLine($"热键触发: {hotKeyInfo.Name}");
                    hotKeyInfo.Callback?.Invoke();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理热键回调时发生异常: {ex.Message}");
            }
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
                UnregisterAllHotKeys();
                _messageWindow?.Dispose();
            }
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 热键信息
        /// </summary>
        public class HotKeyInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public uint Modifiers { get; set; }
            public Keys Key { get; set; }
            public Action? Callback { get; set; }
        }

        /// <summary>
        /// 消息窗口，用于接收热键消息
        /// </summary>
        private class MessageWindow : Form
        {
            private readonly HotKeyManager _manager;

            public MessageWindow(HotKeyManager manager)
            {
                _manager = manager;
                
                // 设置窗口属性 - 确保窗口句柄被创建
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Size = new System.Drawing.Size(1, 1); // 最小可见尺寸
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(-32000, -32000); // 移到屏幕外
                
                // 强制创建窗口句柄
                this.CreateHandle();
                
                System.Diagnostics.Debug.WriteLine($"MessageWindow创建，句柄: 0x{this.Handle:X8}");
            }

            protected override void SetVisibleCore(bool value)
            {
                // 阻止窗口显示，但保持句柄有效
                base.SetVisibleCore(false);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    int hotKeyId = m.WParam.ToInt32();
                    System.Diagnostics.Debug.WriteLine($"收到热键消息，ID: {hotKeyId}");
                    _manager.OnHotKeyPressed(hotKeyId);
                }

                base.WndProc(ref m);
            }
        }

        #endregion
    }
}
