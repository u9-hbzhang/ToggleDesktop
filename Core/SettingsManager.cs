using System;
using System.IO;
using System.Text.Json;

namespace ToggleDesktop.Core
{
    /// <summary>
    /// 设置管理器，负责程序配置的保存和加载
    /// </summary>
    public class SettingsManager
    {
        #region 私有字段
        
        private static SettingsManager? _instance;
        private readonly string _settingsPath;
        private SettingsData _settings = new SettingsData();

        #endregion

        #region 公共属性

        /// <summary>
        /// 单例实例
        /// </summary>
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 是否开机自启动
        /// </summary>
        public bool AutoStart
        {
            get => _settings.AutoStart;
            set => _settings.AutoStart = value;
        }

        /// <summary>
        /// 是否显示通知
        /// </summary>
        public bool ShowNotifications
        {
            get => _settings.ShowNotifications;
            set => _settings.ShowNotifications = value;
        }

        /// <summary>
        /// 全局热键
        /// </summary>
        public string HotKey
        {
            get => _settings.HotKey;
            set => _settings.HotKey = value;
        }

        /// <summary>
        /// 窗口位置X坐标
        /// </summary>
        public int WindowX
        {
            get => _settings.WindowX;
            set => _settings.WindowX = value;
        }

        /// <summary>
        /// 窗口位置Y坐标
        /// </summary>
        public int WindowY
        {
            get => _settings.WindowY;
            set => _settings.WindowY = value;
        }

        /// <summary>
        /// 记住最后的图标状态
        /// </summary>
        public bool RememberIconState
        {
            get => _settings.RememberIconState;
            set => _settings.RememberIconState = value;
        }

        /// <summary>
        /// 最后的图标隐藏状态
        /// </summary>
        public bool LastIconHiddenState
        {
            get => _settings.LastIconHiddenState;
            set => _settings.LastIconHiddenState = value;
        }

        /// <summary>
        /// 使用统计 - 切换次数
        /// </summary>
        public int ToggleCount
        {
            get => _settings.ToggleCount;
            set => _settings.ToggleCount = value;
        }

        /// <summary>
        /// 首次运行时间
        /// </summary>
        public DateTime FirstRunTime
        {
            get => _settings.FirstRunTime;
            set => _settings.FirstRunTime = value;
        }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastRunTime
        {
            get => _settings.LastRunTime;
            set => _settings.LastRunTime = value;
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，实现单例模式
        /// </summary>
        private SettingsManager()
        {
            // 设置文件路径
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "ToggleDesktop");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsPath = Path.Combine(appFolder, "settings.json");
            
            // 加载设置
            LoadSettings();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 保存设置到文件
        /// </summary>
        public void Save()
        {
            try
            {
                _settings.LastRunTime = DateTime.Now;
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重新加载设置
        /// </summary>
        public void Reload()
        {
            LoadSettings();
        }

        /// <summary>
        /// 重置为默认设置
        /// </summary>
        public void ResetToDefault()
        {
            _settings = new SettingsData();
            Save();
        }

        /// <summary>
        /// 获取设置文件路径
        /// </summary>
        /// <returns>设置文件路径</returns>
        public string GetSettingsPath()
        {
            return _settingsPath;
        }

        /// <summary>
        /// 增加切换计数
        /// </summary>
        public void IncrementToggleCount()
        {
            _settings.ToggleCount++;
            Save();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从文件加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    // 首次运行，创建默认设置
                    _settings = new SettingsData();
                    _settings.FirstRunTime = DateTime.Now;
                    Save();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                _settings = new SettingsData();
            }
        }

        #endregion
    }

    /// <summary>
    /// 设置数据结构
    /// </summary>
    public class SettingsData
    {
        /// <summary>
        /// 是否开机自启动
        /// </summary>
        public bool AutoStart { get; set; } = false;

        /// <summary>
        /// 是否显示通知
        /// </summary>
        public bool ShowNotifications { get; set; } = true;

        /// <summary>
        /// 全局热键
        /// </summary>
        public string HotKey { get; set; } = "Ctrl+Alt+D";

        /// <summary>
        /// 窗口位置X坐标
        /// </summary>
        public int WindowX { get; set; } = -1;

        /// <summary>
        /// 窗口位置Y坐标
        /// </summary>
        public int WindowY { get; set; } = -1;

        /// <summary>
        /// 记住最后的图标状态
        /// </summary>
        public bool RememberIconState { get; set; } = true;

        /// <summary>
        /// 最后的图标隐藏状态
        /// </summary>
        public bool LastIconHiddenState { get; set; } = false;

        /// <summary>
        /// 使用统计 - 切换次数
        /// </summary>
        public int ToggleCount { get; set; } = 0;

        /// <summary>
        /// 首次运行时间
        /// </summary>
        public DateTime FirstRunTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastRunTime { get; set; } = DateTime.Now;
    }
}
