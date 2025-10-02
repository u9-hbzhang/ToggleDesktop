using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Reflection;

namespace ToggleDesktop.Utils
{
    /// <summary>
    /// 开机自启动管理器
    /// </summary>
    public static class AutoStartManager
    {
        #region 常量

        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "ToggleDesktop";

        #endregion

        #region 公共方法

        /// <summary>
        /// 检查是否已设置开机自启动
        /// </summary>
        /// <returns>是否已设置自启动</returns>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue(APP_NAME);
                        return value != null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查自启动状态失败: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        /// <param name="enable">是否启用自启动</param>
        /// <returns>操作是否成功</returns>
        public static bool SetAutoStart(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            string exePath = GetExecutablePath();
                            if (!string.IsNullOrEmpty(exePath))
                            {
                                key.SetValue(APP_NAME, exePath);
                                return true;
                            }
                        }
                        else
                        {
                            key.DeleteValue(APP_NAME, false);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置自启动失败: {ex.Message}");
                
                // 显示用户友好的错误信息
                string operation = enable ? "启用" : "禁用";
                MessageBox.Show($"无法{operation}开机自启动功能。\n" +
                              $"可能需要管理员权限或系统限制了注册表访问。\n\n" +
                              $"错误信息: {ex.Message}",
                              "ToggleDesktop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        /// <summary>
        /// 获取当前可执行文件路径
        /// </summary>
        /// <returns>可执行文件路径</returns>
        public static string GetExecutablePath()
        {
            try
            {
                // 获取当前执行的程序集
                Assembly? assembly = Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    string? location = assembly.Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        // 如果是.dll文件，尝试获取对应的.exe文件
                        if (location.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            string exePath = location.Replace(".dll", ".exe");
                            if (System.IO.File.Exists(exePath))
                            {
                                return exePath;
                            }
                        }
                        return location;
                    }
                }

                // 备用方法：使用进程路径
                return System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取可执行文件路径失败: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// 获取注册表中的自启动命令
        /// </summary>
        /// <returns>自启动命令，如果未设置则返回null</returns>
        public static string? GetAutoStartCommand()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue(APP_NAME);
                        return value?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取自启动命令失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 验证自启动设置是否正确
        /// </summary>
        /// <returns>自启动设置是否正确</returns>
        public static bool ValidateAutoStartSetting()
        {
            if (!IsAutoStartEnabled())
            {
                return false;
            }

            string? registryCommand = GetAutoStartCommand();
            string currentPath = GetExecutablePath();

            if (string.IsNullOrEmpty(registryCommand) || string.IsNullOrEmpty(currentPath))
            {
                return false;
            }

            // 检查路径是否匹配
            return string.Equals(registryCommand.Trim('"'), currentPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 修复自启动设置（如果路径已改变）
        /// </summary>
        /// <returns>修复是否成功</returns>
        public static bool RepairAutoStartSetting()
        {
            if (IsAutoStartEnabled() && !ValidateAutoStartSetting())
            {
                // 重新设置自启动
                return SetAutoStart(true);
            }

            return true;
        }

        #endregion
    }
}
