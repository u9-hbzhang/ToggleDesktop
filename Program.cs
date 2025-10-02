using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ToggleDesktop.Core;
using ToggleDesktop.Utils;

namespace ToggleDesktop
{
    /// <summary>
    /// 程序入口点
    /// </summary>
    internal static class Program
    {
        #region Windows API for Console

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        #endregion

        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 设置应用程序为启用视觉样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // 检查是否有命令行参数
                if (args.Length > 0)
                {
                    // 为命令行操作分配控制台
                    AllocConsole();
                    HandleCommandLineArgs(args);
                    FreeConsole();
                    return;
                }

                // GUI模式：不显示控制台窗口
                var consoleWindow = GetConsoleWindow();
                if (consoleWindow != IntPtr.Zero)
                {
                    ShowWindow(consoleWindow, SW_HIDE);
                }

                // 创建并运行主应用程序
                var app = new ToggleDesktopApp();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动失败: {ex.Message}", "ToggleDesktop", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 处理命令行参数
        /// </summary>
        /// <param name="args">命令行参数</param>
        private static void HandleCommandLineArgs(string[] args)
        {
            var manager = DesktopIconManager.Instance;

            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "--toggle":
                    case "-t":
                        bool success = manager.ToggleDesktopIcons();
                        Console.WriteLine($"切换桌面图标: {(success ? "成功" : "失败")}");
                        Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");
                        break;

                    case "--show":
                    case "-s":
                        success = manager.ShowDesktopIcons();
                        Console.WriteLine($"显示桌面图标: {(success ? "成功" : "失败")}");
                        break;

                    case "--hide":
                    case "-h":
                        success = manager.HideDesktopIcons();
                        Console.WriteLine($"隐藏桌面图标: {(success ? "成功" : "失败")}");
                        break;

                    case "--status":
                        Console.WriteLine($"桌面图标状态: {(manager.IsHidden ? "隐藏" : "显示")}");
                        Console.WriteLine($"检测到桌面窗口数量: {manager.GetDesktopWindowCount()}");
                        break;

                    case "--test":
                        ToggleDesktop.Test.TestCore.TestDesktopIconManager();
                        break;

                    case "--interactive":
                    case "-i":
                        ToggleDesktop.Test.TestCore.InteractiveTest();
                        break;

                    case "--debug":
                        ToggleDesktop.Test.DebugTest.DebugWindowSearch();
                        break;

                    case "--debug-toggle":
                        ToggleDesktop.Test.DebugTest.DetailedToggleTest();
                        break;

                    case "--performance":
                        ToggleDesktop.Test.PerformanceTest.RunFullPerformanceTest();
                        break;

                    case "--stress":
                        ToggleDesktop.Test.PerformanceTest.StressTest();
                        break;

                    case "--hotkey-test":
                        ToggleDesktop.Test.HotKeyTest.BasicTest();
                        break;

                    case "--hotkey-default":
                        ToggleDesktop.Test.HotKeyTest.TestDefaultHotKey();
                        break;

                    case "--hotkey-diag":
                        ToggleDesktop.Test.HotKeyTest.DiagnosticTest();
                        break;

                    case "--help":
                    case "/?":
                        ShowHelp();
                        break;

                    default:
                        Console.WriteLine($"未知参数: {arg}");
                        ShowHelp();
                        break;
                }
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("ToggleDesktop - Windows桌面图标切换工具");
            Console.WriteLine();
            Console.WriteLine("用法: ToggleDesktop.exe [选项]");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  --toggle, -t      切换桌面图标显示/隐藏状态");
            Console.WriteLine("  --show, -s        显示桌面图标");
            Console.WriteLine("  --hide, -h        隐藏桌面图标");
            Console.WriteLine("  --status          显示当前状态");
            Console.WriteLine("  --test            运行核心功能测试");
            Console.WriteLine("  --interactive, -i 交互式测试模式");
            Console.WriteLine("  --debug           调试桌面窗口查找");
            Console.WriteLine("  --debug-toggle    详细切换功能测试");
            Console.WriteLine("  --performance     运行性能测试");
            Console.WriteLine("  --stress          运行压力测试");
            Console.WriteLine("  --hotkey-test     基本热键功能测试");
            Console.WriteLine("  --hotkey-default  测试默认热键");
            Console.WriteLine("  --hotkey-diag     热键详细诊断");
            Console.WriteLine("  --help, /?        显示此帮助信息");
            Console.WriteLine();
            Console.WriteLine("不带参数运行将启动GUI界面。");
        }
    }

    /// <summary>
    /// 主应用程序类
    /// </summary>
    public class ToggleDesktopApp
    {
        private ToggleDesktop.UI.TrayManager? _trayManager;
        private DesktopIconManager _iconManager;
        private SettingsManager _settingsManager;

        public ToggleDesktopApp()
        {
            _iconManager = DesktopIconManager.Instance;
            _settingsManager = SettingsManager.Instance;
        }

        /// <summary>
        /// 运行应用程序
        /// </summary>
        public void Run()
        {
            try
            {
                // 程序启动优化
                ResourceOptimizer.OptimizeGarbageCollector();
                ResourceOptimizer.SetProcessPriority(System.Diagnostics.ProcessPriorityClass.Normal);
                
                // 创建托盘管理器
                _trayManager = new ToggleDesktop.UI.TrayManager();
                _trayManager.ExitRequested += OnExitRequested;
                
                // 显示启动消息
                _trayManager.ShowBalloonTip("ToggleDesktop", 
                    "程序已启动，双击托盘图标切换桌面图标状态");

                // 运行消息循环
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序运行出错: {ex.Message}", "ToggleDesktop", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 清理资源
                _trayManager?.Dispose();
                _settingsManager.Save();
                
                // 程序退出优化
                ResourceOptimizer.CleanupResources();
            }
        }

        /// <summary>
        /// 退出请求事件处理
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void OnExitRequested(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
