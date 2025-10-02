using System;
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

            // 检查前台窗口是否是桌面相关窗口
            return foregroundWindow == shellWindow || 
                   foregroundWindow == progman ||
                   foregroundWindow == IntPtr.Zero;
        }

        /// <summary>
        /// 显示桌面 - 使用 Shell COM 接口（最可靠的方法）
        /// </summary>
        public static void ShowDesktop()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("开始显示桌面 - 使用 Shell.Application COM");
                
                // 方法1: 使用 Shell.Application COM 对象
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
