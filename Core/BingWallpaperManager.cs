using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ToggleDesktop.Core
{
    /// <summary>
    /// 必应壁纸管理器
    /// </summary>
    public class BingWallpaperManager
    {
        #region 私有字段

        private static BingWallpaperManager? _instance;
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BING_API_URL = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN";
        private readonly string _savePath;

        #endregion

        #region 公共属性

        /// <summary>
        /// 单例实例
        /// </summary>
        public static BingWallpaperManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BingWallpaperManager();
                }
                return _instance;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，实现单例模式
        /// </summary>
        private BingWallpaperManager()
        {
            // 设置保存路径为"我的图片\BingWallpapers"
            string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            _savePath = Path.Combine(picturesPath, "BingWallpapers");

            // 确保文件夹存在
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            // 设置HttpClient超时
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取当前必应壁纸信息
        /// </summary>
        /// <returns>壁纸信息（URL和标题）</returns>
        public async Task<(string url, string title)?> GetCurrentWallpaperInfoAsync()
        {
            try
            {
                Debug.WriteLine("正在获取必应壁纸信息...");
                
                // 请求必应API
                var response = await _httpClient.GetStringAsync(BING_API_URL);
                
                Debug.WriteLine($"API响应: {response}");

                // 解析JSON
                using (JsonDocument doc = JsonDocument.Parse(response))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("images", out JsonElement images) && images.GetArrayLength() > 0)
                    {
                        var image = images[0];
                        
                        // 获取URL（相对路径）
                        if (!image.TryGetProperty("url", out JsonElement urlElement))
                        {
                            Debug.WriteLine("未找到url属性");
                            return null;
                        }

                        string relativeUrl = urlElement.GetString() ?? "";
                        string fullUrl = $"https://www.bing.com{relativeUrl}";

                        // 获取标题和版权信息
                        string title = "";
                        if (image.TryGetProperty("title", out JsonElement titleElement))
                        {
                            title = titleElement.GetString() ?? "";
                        }
                        
                        if (string.IsNullOrWhiteSpace(title) && 
                            image.TryGetProperty("copyright", out JsonElement copyrightElement))
                        {
                            title = copyrightElement.GetString() ?? "";
                        }

                        // 如果标题太长，截取主要部分
                        if (title.Contains("(©"))
                        {
                            title = title.Substring(0, title.IndexOf("(©")).Trim();
                        }

                        Debug.WriteLine($"获取到壁纸: {title}, URL: {fullUrl}");
                        return (fullUrl, title);
                    }
                }

                Debug.WriteLine("JSON中未找到images数组");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取必应壁纸信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 下载并保存当前必应壁纸
        /// </summary>
        /// <returns>保存的文件路径，失败返回null</returns>
        public async Task<string?> DownloadAndSaveWallpaperAsync()
        {
            try
            {
                // 获取壁纸信息
                var wallpaperInfo = await GetCurrentWallpaperInfoAsync();
                if (wallpaperInfo == null)
                {
                    Debug.WriteLine("无法获取壁纸信息");
                    return null;
                }

                string url = wallpaperInfo.Value.url;
                string title = wallpaperInfo.Value.title;

                Debug.WriteLine($"开始下载壁纸: {url}");

                // 下载图片
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(url);
                
                Debug.WriteLine($"下载完成，大小: {imageBytes.Length} 字节");

                // 生成文件名（使用日期+标题）
                string safeTitle = MakeSafeFileName(title);
                string fileName = $"{DateTime.Now:yyyyMMdd}_{safeTitle}.jpg";
                string filePath = Path.Combine(_savePath, fileName);

                // 如果文件已存在，添加序号
                int counter = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"{DateTime.Now:yyyyMMdd}_{safeTitle}_{counter}.jpg";
                    filePath = Path.Combine(_savePath, fileName);
                    counter++;
                }

                // 保存文件
                await File.WriteAllBytesAsync(filePath, imageBytes);
                
                Debug.WriteLine($"壁纸保存成功: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"下载保存壁纸失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 打开壁纸保存文件夹
        /// </summary>
        public void OpenSaveFolder()
        {
            try
            {
                if (Directory.Exists(_savePath))
                {
                    Process.Start("explorer.exe", _savePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开文件夹失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取保存路径
        /// </summary>
        public string GetSavePath()
        {
            return _savePath;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 将字符串转换为安全的文件名
        /// </summary>
        /// <param name="fileName">原始文件名</param>
        /// <returns>安全的文件名</returns>
        private string MakeSafeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "BingWallpaper";
            }

            // 移除非法字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safeName = fileName;
            
            foreach (char c in invalidChars)
            {
                safeName = safeName.Replace(c, '_');
            }

            // 限制长度
            if (safeName.Length > 50)
            {
                safeName = safeName.Substring(0, 50);
            }

            return safeName.Trim();
        }

        #endregion
    }
}

