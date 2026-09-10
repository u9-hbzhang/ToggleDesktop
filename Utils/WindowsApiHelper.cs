using System;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ToggleDesktop.Utils
{
    /// <summary>
    /// Windows API帮助类，封装桌面图标控制相关的API调用
    /// </summary>
    public static class WindowsApiHelper
    {
        #region Windows API常量

        // ShowWindow命令
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;

        // 窗口类名
        public const string PROGMAN_CLASS = "Progman";
        public const string SHELLDLL_DEFVIEW_CLASS = "SHELLDLL_DefView";
        public const string SYSLISTVIEW32_CLASS = "SysListView32";
        private const string EXPLORER_ADVANCED_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string HIDE_ICONS_VALUE_NAME = "HideIcons";
        private const string DESKTOP_REGISTRY_KEY = @"Control Panel\Desktop";
        private const string WALLPAPER_VALUE_NAME = "WallPaper";
        private const int SPI_GETDESKWALLPAPER = 0x0073;
        private const int MAX_WALLPAPER_PATH = 260;

        #endregion

        #region Windows API声明

        /// <summary>
        /// 查找窗口
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        /// <summary>
        /// 查找子窗口
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string? lpszClass, string? lpszWindow);

        /// <summary>
        /// 显示/隐藏窗口
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 检查窗口是否可见
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// 获取窗口类名
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// 枚举子窗口
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// 枚举窗口回调函数
        /// </summary>
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// 发送消息到窗口
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 获取系统参数（支持读取当前桌面壁纸路径）
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, System.Text.StringBuilder pvParam, int fWinIni);

        /// <summary>
        /// 获取前台窗口
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 获取桌面窗口
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// 获取Shell窗口（桌面窗口）
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        /// <summary>
        /// 模拟键盘输入（旧API，已弃用）
        /// </summary>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        /// <summary>
        /// 发送输入事件（推荐使用）
        /// </summary>
        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // 虚拟键码常量
        public const byte VK_LWIN = 0x5B;  // 左Windows键
        public const byte VK_D = 0x44;      // D键

        // keybd_event标志
        public const uint KEYEVENTF_KEYUP = 0x0002;
        
        // SendInput相关常量
        public const uint INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP_SENDINPUT = 0x0002;

        /// <summary>
        /// INPUT结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        /// <summary>
        /// INPUT联合体
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        /// <summary>
        /// 鼠标输入结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// 键盘输入结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// 硬件输入结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取窗口类名
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>窗口类名</returns>
        public static string GetWindowClassName(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return string.Empty;

            var className = new System.Text.StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }

        /// <summary>
        /// 检查句柄是否有效
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>是否有效</returns>
        public static bool IsValidHandle(IntPtr hWnd)
        {
            return hWnd != IntPtr.Zero;
        }

        /// <summary>
        /// 检查桌面是否为前台窗口
        /// </summary>
        /// <returns>桌面是否为前台</returns>
        public static bool IsDesktopForeground()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            IntPtr shellWindow = GetShellWindow();
            IntPtr progman = FindWindow(PROGMAN_CLASS, null);

            if (foregroundWindow == IntPtr.Zero)
            {
                return true;
            }

            string foregroundClass = GetWindowClassName(foregroundWindow);
            if (foregroundClass == PROGMAN_CLASS ||
                foregroundClass == "WorkerW" ||
                foregroundClass == SHELLDLL_DEFVIEW_CLASS ||
                foregroundClass == SYSLISTVIEW32_CLASS)
            {
                return true;
            }

            // 检查前台窗口是否是桌面相关窗口
            return foregroundWindow == shellWindow || 
                   foregroundWindow == progman;
        }

        /// <summary>
        /// 显示桌面 - 使用 Shell COM 接口（最可靠的方法）
        /// </summary>
        public static void ShowDesktop()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("开始显示桌面 - 使用 Shell.Application COM");
                
                // 优选方法: 使用 Shell.Application COM 对象
                Type shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType != null)
                {
                    dynamic shell = Activator.CreateInstance(shellType);
                    shell.ToggleDesktop();
                    System.Diagnostics.Debug.WriteLine("Shell.Application.ToggleDesktop() 调用成功");
                    
                    // 释放COM对象
                    Marshal.ReleaseComObject(shell);
                    
                    System.Threading.Thread.Sleep(200);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("Shell.Application 方法失败，尝试备用方法");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shell.Application 方法异常: {ex.Message}");
            }
            
            // 备用方法：使用 IShellDispatch 接口
            try
            {
                System.Diagnostics.Debug.WriteLine("尝试 IShellDispatch 接口");
                var shellObj = new Shell32.Shell();
                var shell = (Shell32.IShellDispatch)shellObj;
                shell.ToggleDesktop();
                System.Diagnostics.Debug.WriteLine("IShellDispatch.ToggleDesktop() 调用成功");
                Marshal.ReleaseComObject(shellObj);
                System.Threading.Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IShellDispatch 方法也失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("所有显示桌面的方法都失败了");
            }
        }

        /// <summary>
        /// 从注册表读取“显示桌面图标”状态。
        /// true 表示隐藏，false 表示显示，null 表示无法读取。
        /// </summary>
        public static bool? TryGetDesktopIconsHiddenFromRegistry()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(EXPLORER_ADVANCED_REGISTRY_KEY, false);
                object? value = key?.GetValue(HIDE_ICONS_VALUE_NAME);
                if (value == null)
                {
                    return null;
                }

                int hideIcons = Convert.ToInt32(value);
                return hideIcons != 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取 HideIcons 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 同步“显示桌面图标”的注册表状态。
        /// </summary>
        /// <param name="hidden">true=隐藏图标，false=显示图标</param>
        /// <returns>是否写入成功</returns>
        public static bool TrySetDesktopIconsHiddenInRegistry(bool hidden)
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(EXPLORER_ADVANCED_REGISTRY_KEY, true);
                if (key == null)
                {
                    return false;
                }

                key.SetValue(HIDE_ICONS_VALUE_NAME, hidden ? 1 : 0, RegistryValueKind.DWord);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入 HideIcons 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取当前桌面正在显示的壁纸文件路径。
        /// 优先使用 SystemParametersInfo，失败时回退注册表和系统缓存文件。
        /// </summary>
        /// <returns>壁纸文件路径，失败返回 null</returns>
        public static string? TryGetCurrentDesktopWallpaperPath()
        {
            try
            {
                var buffer = new System.Text.StringBuilder(MAX_WALLPAPER_PATH);
                if (SystemParametersInfo(SPI_GETDESKWALLPAPER, buffer.Capacity, buffer, 0))
                {
                    string wallpaperPath = buffer.ToString().TrimEnd('\0').Trim();
                    if (!string.IsNullOrWhiteSpace(wallpaperPath))
                    {
                        wallpaperPath = Environment.ExpandEnvironmentVariables(wallpaperPath);
                        if (File.Exists(wallpaperPath))
                        {
                            return wallpaperPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SystemParametersInfo 读取壁纸路径失败: {ex.Message}");
            }

            try
            {
                using RegistryKey? desktopKey = Registry.CurrentUser.OpenSubKey(DESKTOP_REGISTRY_KEY, false);
                string? registryPath = desktopKey?.GetValue(WALLPAPER_VALUE_NAME) as string;
                if (!string.IsNullOrWhiteSpace(registryPath))
                {
                    registryPath = Environment.ExpandEnvironmentVariables(registryPath);
                    if (File.Exists(registryPath))
                    {
                        return registryPath;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"注册表读取壁纸路径失败: {ex.Message}");
            }

            try
            {
                string transcodedWallpaperPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Themes\TranscodedWallpaper");

                if (File.Exists(transcodedWallpaperPath))
                {
                    return transcodedWallpaperPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取 TranscodedWallpaper 失败: {ex.Message}");
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// Shell32 COM 互操作
    /// </summary>
    public static class Shell32
    {
        [ComImport]
        [Guid("13709620-C279-11CE-A49E-444553540000")]
        [ClassInterface(ClassInterfaceType.None)]
        public class Shell
        {
        }

        [ComImport]
        [Guid("D8F015C0-C278-11CE-A49E-444553540000")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface IShellDispatch
        {
            void ToggleDesktop();
        }
    }
}
