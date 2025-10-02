using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ToggleDesktop.Core;

namespace ToggleDesktop.Test
{
    /// <summary>
    /// 性能测试类
    /// </summary>
    public static class PerformanceTest
    {
        /// <summary>
        /// 执行全面的性能测试
        /// </summary>
        public static void RunFullPerformanceTest()
        {
            Console.WriteLine("=== ToggleDesktop 性能测试 ===");
            Console.WriteLine();

            // 测试内存使用情况
            TestMemoryUsage();
            Console.WriteLine();

            // 测试桌面图标切换性能
            TestTogglePerformance();
            Console.WriteLine();

            // 测试热键响应性能
            TestHotKeyPerformance();
            Console.WriteLine();

            // 测试设置管理性能
            TestSettingsPerformance();
            Console.WriteLine();

            // 测试多线程安全性
            TestThreadSafety();
            Console.WriteLine();

            // 系统资源占用分析
            AnalyzeSystemResources();
            Console.WriteLine();

            Console.WriteLine("=== 性能测试完成 ===");
        }

        /// <summary>
        /// 测试内存使用情况
        /// </summary>
        private static void TestMemoryUsage()
        {
            Console.WriteLine("📊 内存使用测试");
            
            // 获取当前进程
            var process = Process.GetCurrentProcess();
            
            // 记录初始内存
            long initialMemory = GC.GetTotalMemory(false);
            long workingSet = process.WorkingSet64;
            
            Console.WriteLine($"初始内存使用:");
            Console.WriteLine($"  托管内存: {FormatBytes(initialMemory)}");
            Console.WriteLine($"  工作集: {FormatBytes(workingSet)}");
            
            // 创建多个实例测试内存泄漏
            var manager = DesktopIconManager.Instance;
            var hotKeyManager = HotKeyManager.Instance;
            var settingsManager = SettingsManager.Instance;
            
            // 执行一些操作
            for (int i = 0; i < 10; i++)
            {
                manager.RefreshDesktopWindows();
                settingsManager.Save();
                Thread.Sleep(100);
            }
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // 记录最终内存
            long finalMemory = GC.GetTotalMemory(false);
            long finalWorkingSet = process.WorkingSet64;
            
            Console.WriteLine($"最终内存使用:");
            Console.WriteLine($"  托管内存: {FormatBytes(finalMemory)} (变化: {FormatBytes(finalMemory - initialMemory)})");
            Console.WriteLine($"  工作集: {FormatBytes(finalWorkingSet)} (变化: {FormatBytes(finalWorkingSet - workingSet)})");
            
            // 内存泄漏检查
            if (finalMemory - initialMemory > 1024 * 1024) // 超过1MB增长
            {
                Console.WriteLine("⚠️ 可能存在内存泄漏");
            }
            else
            {
                Console.WriteLine("✅ 内存使用正常");
            }
        }

        /// <summary>
        /// 测试桌面图标切换性能
        /// </summary>
        private static void TestTogglePerformance()
        {
            Console.WriteLine("⚡ 桌面图标切换性能测试");
            
            var manager = DesktopIconManager.Instance;
            var stopwatch = new Stopwatch();
            
            // 测试窗口检测性能
            stopwatch.Start();
            manager.RefreshDesktopWindows();
            stopwatch.Stop();
            
            long refreshTime = stopwatch.ElapsedMilliseconds;
            int windowCount = manager.GetDesktopWindowCount();
            
            Console.WriteLine($"窗口检测: {refreshTime}ms (检测到 {windowCount} 个窗口)");
            
            if (windowCount > 0)
            {
                // 测试切换性能
                stopwatch.Restart();
                bool result = manager.ToggleDesktopIcons();
                stopwatch.Stop();
                
                long toggleTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"图标切换: {toggleTime}ms (结果: {(result ? "成功" : "失败")})");
                
                // 再次切换回来
                Thread.Sleep(500);
                stopwatch.Restart();
                manager.ToggleDesktopIcons();
                stopwatch.Stop();
                
                long toggleBackTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"恢复切换: {toggleBackTime}ms");
                
                // 性能评估
                if (toggleTime < 100 && toggleBackTime < 100)
                {
                    Console.WriteLine("✅ 切换性能优秀");
                }
                else if (toggleTime < 500 && toggleBackTime < 500)
                {
                    Console.WriteLine("✅ 切换性能良好");
                }
                else
                {
                    Console.WriteLine("⚠️ 切换性能可能需要优化");
                }
            }
            else
            {
                Console.WriteLine("⚠️ 未检测到桌面窗口，无法测试切换性能");
            }
        }

        /// <summary>
        /// 测试热键响应性能
        /// </summary>
        private static void TestHotKeyPerformance()
        {
            Console.WriteLine("⌨️ 热键性能测试");
            
            var hotKeyManager = HotKeyManager.Instance;
            var stopwatch = new Stopwatch();
            
            // 测试热键注册性能
            stopwatch.Start();
            bool registered = hotKeyManager.RegisterHotKey("TestHotKey", 
                HotKeyManager.MOD_CONTROL | HotKeyManager.MOD_ALT, 
                System.Windows.Forms.Keys.F12, 
                () => { /* 测试回调 */ });
            stopwatch.Stop();
            
            long registerTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"热键注册: {registerTime}ms (结果: {(registered ? "成功" : "失败")})");
            
            if (registered)
            {
                // 测试热键注销性能
                stopwatch.Restart();
                bool unregistered = hotKeyManager.UnregisterHotKey("TestHotKey");
                stopwatch.Stop();
                
                long unregisterTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"热键注销: {unregisterTime}ms (结果: {(unregistered ? "成功" : "失败")})");
                
                if (registerTime < 50 && unregisterTime < 50)
                {
                    Console.WriteLine("✅ 热键性能优秀");
                }
                else
                {
                    Console.WriteLine("✅ 热键性能正常");
                }
            }
            else
            {
                Console.WriteLine("⚠️ 热键注册失败，可能已被占用");
            }
        }

        /// <summary>
        /// 测试设置管理性能
        /// </summary>
        private static void TestSettingsPerformance()
        {
            Console.WriteLine("⚙️ 设置管理性能测试");
            
            var settingsManager = SettingsManager.Instance;
            var stopwatch = new Stopwatch();
            
            // 测试设置保存性能
            stopwatch.Start();
            for (int i = 0; i < 10; i++)
            {
                settingsManager.ToggleCount = i;
                settingsManager.Save();
            }
            stopwatch.Stop();
            
            long saveTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"设置保存 (10次): {saveTime}ms (平均: {saveTime / 10.0:F1}ms/次)");
            
            // 测试设置加载性能
            stopwatch.Restart();
            for (int i = 0; i < 10; i++)
            {
                settingsManager.Reload();
            }
            stopwatch.Stop();
            
            long loadTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"设置加载 (10次): {loadTime}ms (平均: {loadTime / 10.0:F1}ms/次)");
            
            if (saveTime < 100 && loadTime < 50)
            {
                Console.WriteLine("✅ 设置管理性能优秀");
            }
            else
            {
                Console.WriteLine("✅ 设置管理性能正常");
            }
        }

        /// <summary>
        /// 测试多线程安全性
        /// </summary>
        private static void TestThreadSafety()
        {
            Console.WriteLine("🔒 多线程安全性测试");
            
            var manager = DesktopIconManager.Instance;
            var settingsManager = SettingsManager.Instance;
            var exceptions = new List<Exception>();
            
            // 创建多个并发任务
            var tasks = new Task[5];
            for (int i = 0; i < tasks.Length; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            // 并发访问各种操作
                            manager.RefreshDesktopWindows();
                            settingsManager.ToggleCount = taskId * 10 + j;
                            settingsManager.Save();
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            
            // 等待所有任务完成
            Task.WaitAll(tasks);
            
            if (exceptions.Count == 0)
            {
                Console.WriteLine("✅ 多线程安全性测试通过");
            }
            else
            {
                Console.WriteLine($"⚠️ 发现 {exceptions.Count} 个并发异常:");
                foreach (var ex in exceptions)
                {
                    Console.WriteLine($"  - {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 分析系统资源占用
        /// </summary>
        private static void AnalyzeSystemResources()
        {
            Console.WriteLine("📈 系统资源分析");
            
            var process = Process.GetCurrentProcess();
            
            // CPU 使用情况
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            
            // 执行一些操作
            var manager = DesktopIconManager.Instance;
            for (int i = 0; i < 5; i++)
            {
                manager.RefreshDesktopWindows();
                Thread.Sleep(100);
            }
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            Console.WriteLine($"CPU 使用率: {cpuUsageTotal:P2}");
            Console.WriteLine($"线程数: {process.Threads.Count}");
            Console.WriteLine($"句柄数: {process.HandleCount}");
            Console.WriteLine($"虚拟内存: {FormatBytes(process.VirtualMemorySize64)}");
            Console.WriteLine($"峰值工作集: {FormatBytes(process.PeakWorkingSet64)}");
            
            // 资源使用评估
            if (process.WorkingSet64 < 50 * 1024 * 1024) // 小于50MB
            {
                Console.WriteLine("✅ 内存使用优秀");
            }
            else if (process.WorkingSet64 < 100 * 1024 * 1024) // 小于100MB
            {
                Console.WriteLine("✅ 内存使用良好");
            }
            else
            {
                Console.WriteLine("⚠️ 内存使用较高");
            }
        }

        /// <summary>
        /// 格式化字节数
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化的字符串</returns>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        /// <summary>
        /// 压力测试
        /// </summary>
        public static void StressTest()
        {
            Console.WriteLine("=== 压力测试 ===");
            Console.WriteLine();
            
            var manager = DesktopIconManager.Instance;
            var stopwatch = new Stopwatch();
            
            Console.WriteLine("执行100次快速切换测试...");
            stopwatch.Start();
            
            int successCount = 0;
            for (int i = 0; i < 100; i++)
            {
                if (manager.ToggleDesktopIcons())
                {
                    successCount++;
                }
                Thread.Sleep(10); // 短暂延迟
            }
            
            stopwatch.Stop();
            
            Console.WriteLine($"测试完成: {successCount}/100 成功");
            Console.WriteLine($"总耗时: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均耗时: {stopwatch.ElapsedMilliseconds / 100.0:F1}ms/次");
            
            if (successCount >= 95)
            {
                Console.WriteLine("✅ 压力测试通过");
            }
            else
            {
                Console.WriteLine("⚠️ 压力测试存在问题");
            }
        }
    }
}
