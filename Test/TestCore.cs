using System;
using ToggleDesktop.Core;

namespace ToggleDesktop.Test
{
    /// <summary>
    /// 核心功能测试类
    /// </summary>
    public static class TestCore
    {
        /// <summary>
        /// 测试桌面图标管理器基本功能
        /// </summary>
        public static void TestDesktopIconManager()
        {
            Console.WriteLine("=== 测试桌面图标管理器 ===");
            Console.WriteLine();

            var manager = DesktopIconManager.Instance;
            
            // 监听状态变化事件
            manager.OnDesktopIconsStateChanged += (isHidden) =>
            {
                Console.WriteLine($"[事件] 桌面图标状态改变: {(isHidden ? "隐藏" : "显示")}");
            };

            // 显示初始状态
            Console.WriteLine($"初始状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            Console.WriteLine($"检测到桌面窗口数量: {manager.GetDesktopWindowCount()}");
            Console.WriteLine();

            if (manager.GetDesktopWindowCount() == 0)
            {
                Console.WriteLine("⚠️ 未检测到桌面图标窗口，可能需要管理员权限或系统不兼容");
                return;
            }

            // 测试功能
            Console.WriteLine("正在测试切换功能...");
            Console.WriteLine();

            // 第一次切换
            Console.Write("执行第一次切换... ");
            bool result1 = manager.ToggleDesktopIcons();
            Console.WriteLine($"{(result1 ? "成功" : "失败")}");
            Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            
            System.Threading.Thread.Sleep(2000); // 等待2秒

            // 第二次切换
            Console.Write("执行第二次切换... ");
            bool result2 = manager.ToggleDesktopIcons();
            Console.WriteLine($"{(result2 ? "成功" : "失败")}");
            Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");

            System.Threading.Thread.Sleep(1000);

            // 测试具体的显示/隐藏方法
            Console.WriteLine();
            Console.WriteLine("测试具体方法:");
            
            Console.Write("隐藏桌面图标... ");
            bool hideResult = manager.HideDesktopIcons();
            Console.WriteLine($"{(hideResult ? "成功" : "失败")}");
            
            System.Threading.Thread.Sleep(1000);
            
            Console.Write("显示桌面图标... ");
            bool showResult = manager.ShowDesktopIcons();
            Console.WriteLine($"{(showResult ? "成功" : "失败")}");

            Console.WriteLine();
            Console.WriteLine("=== 测试完成 ===");
        }

        /// <summary>
        /// 交互式测试模式
        /// </summary>
        public static void InteractiveTest()
        {
            Console.WriteLine("=== 交互式测试模式 ===");
            Console.WriteLine("按任意键进行切换，输入 'q' 退出");
            Console.WriteLine();

            var manager = DesktopIconManager.Instance;
            
            manager.OnDesktopIconsStateChanged += (isHidden) =>
            {
                Console.WriteLine($"✓ 桌面图标状态: {(isHidden ? "隐藏" : "显示")}");
            };

            Console.WriteLine($"当前状态: {(manager.IsHidden ? "隐藏" : "显示")}");
            Console.WriteLine($"检测到桌面窗口: {manager.GetDesktopWindowCount()} 个");
            Console.WriteLine();

            while (true)
            {
                Console.Write("按键操作 (任意键=切换, q=退出): ");
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    break;
                }

                bool result = manager.ToggleDesktopIcons();
                if (!result)
                {
                    Console.WriteLine("❌ 切换失败");
                }
                Console.WriteLine();
            }

            Console.WriteLine("退出测试模式");
        }
    }
}
