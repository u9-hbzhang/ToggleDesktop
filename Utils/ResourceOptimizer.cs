using System;
using System.Diagnostics;
using System.Runtime;

namespace ToggleDesktop.Utils
{
    /// <summary>
    /// 资源优化工具类
    /// </summary>
    public static class ResourceOptimizer
    {
        /// <summary>
        /// 优化内存使用
        /// </summary>
        public static void OptimizeMemory()
        {
            try
            {
                // 强制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // 压缩大对象堆
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                // 释放未使用的内存给操作系统
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                System.Diagnostics.Debug.WriteLine("内存优化完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"内存优化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取内存使用情况
        /// </summary>
        /// <returns>内存使用报告</returns>
        public static MemoryUsageReport GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            
            return new MemoryUsageReport
            {
                ManagedMemory = GC.GetTotalMemory(false),
                WorkingSet = process.WorkingSet64,
                VirtualMemory = process.VirtualMemorySize64,
                PrivateMemory = process.PrivateMemorySize64,
                PeakWorkingSet = process.PeakWorkingSet64,
                Generation0Collections = GC.CollectionCount(0),
                Generation1Collections = GC.CollectionCount(1),
                Generation2Collections = GC.CollectionCount(2)
            };
        }

        /// <summary>
        /// 检查内存泄漏
        /// </summary>
        /// <param name="baseline">基线内存使用</param>
        /// <param name="current">当前内存使用</param>
        /// <returns>是否可能存在内存泄漏</returns>
        public static bool CheckMemoryLeak(MemoryUsageReport baseline, MemoryUsageReport current)
        {
            // 托管内存增长超过5MB
            long managedGrowth = current.ManagedMemory - baseline.ManagedMemory;
            if (managedGrowth > 5 * 1024 * 1024)
            {
                return true;
            }

            // 工作集增长超过10MB
            long workingSetGrowth = current.WorkingSet - baseline.WorkingSet;
            if (workingSetGrowth > 10 * 1024 * 1024)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置进程优先级
        /// </summary>
        /// <param name="priority">优先级</param>
        public static void SetProcessPriority(ProcessPriorityClass priority)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                process.PriorityClass = priority;
                System.Diagnostics.Debug.WriteLine($"进程优先级设置为: {priority}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置进程优先级失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 优化垃圾回收器设置
        /// </summary>
        public static void OptimizeGarbageCollector()
        {
            try
            {
                // 设置为服务器GC模式（如果可用）
                if (GCSettings.IsServerGC)
                {
                    System.Diagnostics.Debug.WriteLine("使用服务器GC模式");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("使用工作站GC模式");
                }

                // 设置延迟模式为交互式
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
                
                System.Diagnostics.Debug.WriteLine("GC设置优化完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GC优化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理系统资源
        /// </summary>
        public static void CleanupResources()
        {
            try
            {
                // 优化内存
                OptimizeMemory();

                // 清理临时文件（如果有）
                string tempPath = System.IO.Path.GetTempPath();
                string appTempPath = System.IO.Path.Combine(tempPath, "ToggleDesktop");
                
                if (System.IO.Directory.Exists(appTempPath))
                {
                    try
                    {
                        System.IO.Directory.Delete(appTempPath, true);
                        System.Diagnostics.Debug.WriteLine("临时文件清理完成");
                    }
                    catch
                    {
                        // 忽略清理失败
                    }
                }

                System.Diagnostics.Debug.WriteLine("资源清理完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"资源清理失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 监控资源使用情况
        /// </summary>
        /// <param name="intervalMs">监控间隔（毫秒）</param>
        /// <param name="durationMs">监控持续时间（毫秒）</param>
        /// <returns>资源使用统计</returns>
        public static ResourceUsageStats MonitorResourceUsage(int intervalMs = 1000, int durationMs = 10000)
        {
            var stats = new ResourceUsageStats();
            var stopwatch = Stopwatch.StartNew();
            
            var initialReport = GetMemoryUsage();
            stats.InitialMemory = initialReport.ManagedMemory;
            stats.InitialWorkingSet = initialReport.WorkingSet;

            long maxMemory = initialReport.ManagedMemory;
            long maxWorkingSet = initialReport.WorkingSet;
            int sampleCount = 0;

            while (stopwatch.ElapsedMilliseconds < durationMs)
            {
                System.Threading.Thread.Sleep(intervalMs);
                
                var currentReport = GetMemoryUsage();
                sampleCount++;
                
                if (currentReport.ManagedMemory > maxMemory)
                    maxMemory = currentReport.ManagedMemory;
                    
                if (currentReport.WorkingSet > maxWorkingSet)
                    maxWorkingSet = currentReport.WorkingSet;
            }

            var finalReport = GetMemoryUsage();
            
            stats.FinalMemory = finalReport.ManagedMemory;
            stats.FinalWorkingSet = finalReport.WorkingSet;
            stats.PeakMemory = maxMemory;
            stats.PeakWorkingSet = maxWorkingSet;
            stats.SampleCount = sampleCount;
            stats.MonitoringDuration = stopwatch.ElapsedMilliseconds;

            return stats;
        }
    }

    /// <summary>
    /// 内存使用报告
    /// </summary>
    public class MemoryUsageReport
    {
        public long ManagedMemory { get; set; }
        public long WorkingSet { get; set; }
        public long VirtualMemory { get; set; }
        public long PrivateMemory { get; set; }
        public long PeakWorkingSet { get; set; }
        public int Generation0Collections { get; set; }
        public int Generation1Collections { get; set; }
        public int Generation2Collections { get; set; }
    }

    /// <summary>
    /// 资源使用统计
    /// </summary>
    public class ResourceUsageStats
    {
        public long InitialMemory { get; set; }
        public long FinalMemory { get; set; }
        public long PeakMemory { get; set; }
        public long InitialWorkingSet { get; set; }
        public long FinalWorkingSet { get; set; }
        public long PeakWorkingSet { get; set; }
        public int SampleCount { get; set; }
        public long MonitoringDuration { get; set; }
        
        public long MemoryGrowth => FinalMemory - InitialMemory;
        public long WorkingSetGrowth => FinalWorkingSet - InitialWorkingSet;
        public double AverageMemoryGrowthRate => SampleCount > 0 ? (double)MemoryGrowth / SampleCount : 0;
    }
}
