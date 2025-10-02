using System;
using System.Diagnostics;
using ToggleDesktop.Core;
using ToggleDesktop.Utils;

namespace ToggleDesktop.Test
{
    /// <summary>
    /// 调试测试类，输出详细的调试信息
    /// </summary>
    public static class DebugTest
    {
        /// <summary>
        /// 调试桌面窗口查找过程
        /// </summary>
        public static void DebugWindowSearch()
        {
            Console.WriteLine("=== 调试桌面窗口查找 ===");
            Console.WriteLine();

            // 1. 查找Progman窗口
            Console.WriteLine("1. 查找Progman窗口:");
            IntPtr progman = WindowsApiHelper.FindWindow(WindowsApiHelper.PROGMAN_CLASS, null);
            Console.WriteLine($"   Progman句柄: 0x{progman:X8} {(progman != IntPtr.Zero ? "✓" : "✗")}");

            if (progman != IntPtr.Zero)
            {
                // 查找SHELLDLL_DefView
                Console.WriteLine("   查找SHELLDLL_DefView窗口:");
                IntPtr shellView = WindowsApiHelper.FindWindowEx(progman, IntPtr.Zero, 
                    WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS, null);
                Console.WriteLine($"   SHELLDLL_DefView句柄: 0x{shellView:X8} {(shellView != IntPtr.Zero ? "✓" : "✗")}");

                if (shellView != IntPtr.Zero)
                {
                    bool isVisible = WindowsApiHelper.IsWindowVisible(shellView);
                    Console.WriteLine($"   SHELLDLL_DefView可见性: {isVisible}");

                    // 查找SysListView32
                    Console.WriteLine("   查找SysListView32窗口:");
                    IntPtr listView = WindowsApiHelper.FindWindowEx(shellView, IntPtr.Zero,
                        WindowsApiHelper.SYSLISTVIEW32_CLASS, null);
                    Console.WriteLine($"   SysListView32句柄: 0x{listView:X8} {(listView != IntPtr.Zero ? "✓" : "✗")}");

                    if (listView != IntPtr.Zero)
                    {
                        bool listVisible = WindowsApiHelper.IsWindowVisible(listView);
                        Console.WriteLine($"   SysListView32可见性: {listVisible}");
                    }
                }
            }

            Console.WriteLine();

            // 2. 查找WorkerW窗口
            Console.WriteLine("2. 查找WorkerW窗口:");
            
            // 发送消息创建WorkerW
            if (progman != IntPtr.Zero)
            {
                WindowsApiHelper.SendMessage(progman, 0x052C, IntPtr.Zero, IntPtr.Zero);
                Console.WriteLine("   已发送创建WorkerW消息");
            }

            // 枚举WorkerW窗口
            var workerWWindows = new System.Collections.Generic.List<IntPtr>();
            WindowsApiHelper.EnumChildWindows(IntPtr.Zero, (hWnd, lParam) =>
            {
                string className = WindowsApiHelper.GetWindowClassName(hWnd);
                if (className == "WorkerW")
                {
                    workerWWindows.Add(hWnd);
                    Console.WriteLine($"   找到WorkerW: 0x{hWnd:X8}");
                }
                return true;
            }, IntPtr.Zero);

            Console.WriteLine($"   总共找到 {workerWWindows.Count} 个WorkerW窗口");

            foreach (var workerW in workerWWindows)
            {
                IntPtr shellView = WindowsApiHelper.FindWindowEx(workerW, IntPtr.Zero,
                    WindowsApiHelper.SHELLDLL_DEFVIEW_CLASS, null);
                
                if (shellView != IntPtr.Zero)
                {
                    Console.WriteLine($"   WorkerW 0x{workerW:X8} 包含 SHELLDLL_DefView: 0x{shellView:X8}");
                    bool isVisible = WindowsApiHelper.IsWindowVisible(shellView);
                    Console.WriteLine($"   可见性: {isVisible}");

                    IntPtr listView = WindowsApiHelper.FindWindowEx(shellView, IntPtr.Zero,
                        WindowsApiHelper.SYSLISTVIEW32_CLASS, null);
                    
                    if (listView != IntPtr.Zero)
                    {
                        Console.WriteLine($"   包含 SysListView32: 0x{listView:X8}");
                        bool listVisible = WindowsApiHelper.IsWindowVisible(listView);
                        Console.WriteLine($"   SysListView32可见性: {listVisible}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== 窗口查找完成 ===");
        }

        /// <summary>
        /// 详细测试切换功能
        /// </summary>
        public static void DetailedToggleTest()
        {
            Console.WriteLine("=== 详细切换功能测试 ===");
            Console.WriteLine();

            // Debug输出将在调试模式下显示在调试窗口中

            var manager = DesktopIconManager.Instance;
            
            Console.WriteLine($"初始状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            Console.WriteLine($"检测到桌面窗口数量: {manager.GetDesktopWindowCount()}");
            Console.WriteLine();

            Console.WriteLine("执行第一次切换:");
            bool result1 = manager.ToggleDesktopIcons();
            Console.WriteLine($"结果: {(result1 ? "成功" : "失败")}");
            Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            Console.WriteLine();

            Console.WriteLine("等待3秒...");
            System.Threading.Thread.Sleep(3000);

            Console.WriteLine("执行第二次切换:");
            bool result2 = manager.ToggleDesktopIcons();
            Console.WriteLine($"结果: {(result2 ? "成功" : "失败")}");
            Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            Console.WriteLine();

            Console.WriteLine("=== 测试完成 ===");
        }
    }
}
