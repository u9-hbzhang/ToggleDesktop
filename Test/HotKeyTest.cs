using System;
using System.Threading;
using System.Windows.Forms;
using ToggleDesktop.Core;

namespace ToggleDesktop.Test
{
    /// <summary>
    /// 热键功能测试类
    /// </summary>
    public static class HotKeyTest
    {
        /// <summary>
        /// 基本热键测试
        /// </summary>
        public static void BasicTest()
        {
            Console.WriteLine("=== 热键基本功能测试 ===");
            
            var hotKeyManager = HotKeyManager.Instance;
            bool testSuccess = false;
            
            // 测试热键注册
            Console.WriteLine("1. 测试热键注册...");
            bool registerResult = hotKeyManager.RegisterHotKey("TestHotKey", 
                HotKeyManager.MOD_CONTROL | HotKeyManager.MOD_ALT, 
                Keys.T, 
                () => {
                    Console.WriteLine("✓ 热键触发成功！Ctrl+Alt+T 被按下");
                    testSuccess = true;
                });
            
            Console.WriteLine($"   注册结果: {(registerResult ? "成功" : "失败")}");
            
            if (registerResult)
            {
                Console.WriteLine("2. 开始消息循环测试热键...");
                Console.WriteLine("   请按 Ctrl+Alt+T 测试热键（10秒内）");
                
                // 在GUI线程中运行消息循环
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                var form = new Form()
                {
                    WindowState = FormWindowState.Minimized,
                    ShowInTaskbar = false,
                    Visible = false
                };
                
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 10000; // 10秒
                timer.Tick += (s, e) => {
                    timer.Stop();
                    form.Close();
                };
                timer.Start();
                
                // 运行消息循环
                Application.Run(form);
                
                // 注销热键
                bool unregisterResult = hotKeyManager.UnregisterHotKey("TestHotKey");
                Console.WriteLine($"3. 注销热键: {(unregisterResult ? "成功" : "失败")}");
                
                if (testSuccess)
                {
                    Console.WriteLine("✓ 热键测试成功！");
                }
                else
                {
                    Console.WriteLine("✗ 热键没有响应，可能存在问题");
                }
            }
            else
            {
                Console.WriteLine("✗ 热键注册失败，无法进行测试");
            }
        }
        
        /// <summary>
        /// 测试默认热键
        /// </summary>
        public static void TestDefaultHotKey()
        {
            Console.WriteLine("=== 默认热键测试 ===");
            
            var settingsManager = SettingsManager.Instance;
            var hotKeyManager = HotKeyManager.Instance;
            
            string defaultHotKey = settingsManager.HotKey;
            Console.WriteLine($"当前默认热键: {defaultHotKey}");
            
            // 解析热键
            if (HotKeyManager.ParseHotKeyString(defaultHotKey, out uint modifiers, out Keys key))
            {
                Console.WriteLine($"解析结果: 修饰键={modifiers}, 主键={key}");
                
                // 测试热键是否可用
                bool available = hotKeyManager.TestHotKey(modifiers, key);
                Console.WriteLine($"热键可用性: {(available ? "可用" : "不可用/已被占用")}");
                
                if (available)
                {
                    Console.WriteLine($"请尝试按 {defaultHotKey} 来切换桌面图标...");
                    Console.WriteLine("(按任意键退出测试)");
                    
                    // 等待用户测试
                    var startTime = DateTime.Now;
                    while (!Console.KeyAvailable && (DateTime.Now - startTime).TotalSeconds < 30)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }
                    
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                }
            }
            else
            {
                Console.WriteLine("✗ 默认热键解析失败");
            }
        }
        
        /// <summary>
        /// 详细诊断测试
        /// </summary>
        public static void DiagnosticTest()
        {
            Console.WriteLine("=== 热键详细诊断 ===");
            
            var hotKeyManager = HotKeyManager.Instance;
            
            Console.WriteLine("1. 检查常用热键组合可用性:");
            var testCombinations = new[]
            {
                new { Name = "Ctrl+Alt+D", Modifiers = HotKeyManager.MOD_CONTROL | HotKeyManager.MOD_ALT, Key = Keys.D },
                new { Name = "Ctrl+Alt+T", Modifiers = HotKeyManager.MOD_CONTROL | HotKeyManager.MOD_ALT, Key = Keys.T },
                new { Name = "Ctrl+Shift+D", Modifiers = HotKeyManager.MOD_CONTROL | HotKeyManager.MOD_SHIFT, Key = Keys.D },
                new { Name = "Win+D", Modifiers = HotKeyManager.MOD_WIN, Key = Keys.D },
            };
            
            foreach (var combo in testCombinations)
            {
                bool available = hotKeyManager.TestHotKey(combo.Modifiers, combo.Key);
                Console.WriteLine($"   {combo.Name}: {(available ? "可用" : "不可用")}");
            }
            
            Console.WriteLine("\n2. 测试当前应用的热键注册:");
            var settingsManager = SettingsManager.Instance;
            string currentHotKey = settingsManager.HotKey;
            Console.WriteLine($"   当前设置的热键: {currentHotKey}");
            
            if (HotKeyManager.ParseHotKeyString(currentHotKey, out uint modifiers, out Keys key))
            {
                bool available = hotKeyManager.TestHotKey(modifiers, key);
                Console.WriteLine($"   热键可用性: {(available ? "可用" : "不可用/已被占用")}");
                
                // 简单注册测试
                bool registered = hotKeyManager.RegisterHotKey("QuickTest", modifiers, key, 
                    () => Console.WriteLine("✓ 当前热键触发测试成功！"));
                Console.WriteLine($"   注册测试: {(registered ? "成功" : "失败")}");
                
                if (registered)
                {
                    hotKeyManager.UnregisterHotKey("QuickTest");
                    Console.WriteLine("   测试热键已注销");
                }
            }
            else
            {
                Console.WriteLine("   ✗ 热键字符串解析失败");
            }
        }
    }
}
