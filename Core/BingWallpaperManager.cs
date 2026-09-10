using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using ToggleDesktop.Utils;

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
        private const string BING_API_BASE_URL = "https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN";
        private const int PREFERRED_UHD_WIDTH = 7680;
        private const int PREFERRED_UHD_HEIGHT = 4320;
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
        /// <returns>壁纸信息（候选URL列表和标题）</returns>
        public async Task<(List<string> urlCandidates, string title)?> GetCurrentWallpaperInfoAsync()
        {
            string[] apiUrls =
            {
                BuildBingApiUrl(useUhd: true),
                BuildBingApiUrl(useUhd: false)
            };

            foreach (string apiUrl in apiUrls)
            {
                try
                {
                    Debug.WriteLine($"正在获取必应壁纸信息: {apiUrl}");

                    // 请求必应API
                    var response = await _httpClient.GetStringAsync(apiUrl);
                    var wallpaperInfo = ParseWallpaperInfo(response);
                    if (wallpaperInfo != null)
                    {
                        return wallpaperInfo;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取必应壁纸信息失败（{apiUrl}）: {ex.Message}");
                }
            }

            Debug.WriteLine("所有必应API请求都失败了");
            return null;
        }

        /// <summary>
        /// 下载并保存当前必应壁纸
        /// </summary>
        /// <returns>保存的文件路径，失败返回null</returns>
        public async Task<string?> DownloadAndSaveWallpaperAsync()
        {
            try
            {
                // 优先保存当前桌面正在显示的必应壁纸（与用户所见一致）
                string? savedCurrentWallpaperPath = await TrySaveCurrentDesktopBingWallpaperAsync();
                if (!string.IsNullOrWhiteSpace(savedCurrentWallpaperPath))
                {
                    return savedCurrentWallpaperPath;
                }

                // 获取壁纸信息
                var wallpaperInfo = await GetCurrentWallpaperInfoAsync();
                if (wallpaperInfo == null)
                {
                    Debug.WriteLine("无法获取壁纸信息");
                    return null;
                }

                string title = wallpaperInfo.Value.title;
                byte[]? imageBytes = null;
                string? downloadedUrl = null;

                foreach (string candidateUrl in wallpaperInfo.Value.urlCandidates)
                {
                    try
                    {
                        Debug.WriteLine($"尝试下载壁纸: {candidateUrl}");
                        imageBytes = await _httpClient.GetByteArrayAsync(candidateUrl);
                        downloadedUrl = candidateUrl;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"下载候选地址失败（{candidateUrl}）: {ex.Message}");
                    }
                }

                if (imageBytes == null)
                {
                    Debug.WriteLine("所有候选壁纸地址都下载失败");
                    return null;
                }
                
                Debug.WriteLine($"下载完成，来源: {downloadedUrl}, 大小: {imageBytes.Length} 字节");

                // 生成文件名（使用日期+标题）
                string safeTitle = MakeSafeFileName(title);
                string filePath = BuildUniqueFilePath(safeTitle, ".jpg");

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

        /// <summary>
        /// 优先收藏当前桌面正在显示的必应壁纸（例如 Microsoft Bing Wallpaper 客户端缓存）
        /// </summary>
        /// <returns>保存路径，若不可用返回 null</returns>
        private async Task<string?> TrySaveCurrentDesktopBingWallpaperAsync()
        {
            try
            {
                string? currentWallpaperPath = WindowsApiHelper.TryGetCurrentDesktopWallpaperPath();
                if (string.IsNullOrWhiteSpace(currentWallpaperPath) || !File.Exists(currentWallpaperPath))
                {
                    Debug.WriteLine("当前桌面壁纸路径不可用，回退到必应API下载");
                    return null;
                }

                if (!IsLikelyBingWallpaper(currentWallpaperPath))
                {
                    Debug.WriteLine($"当前壁纸不是必应来源: {currentWallpaperPath}");
                    return null;
                }

                string sourceExtension = Path.GetExtension(currentWallpaperPath);
                if (string.IsNullOrWhiteSpace(sourceExtension) || sourceExtension.Length > 8)
                {
                    sourceExtension = ".jpg";
                }

                string sourceName = Path.GetFileNameWithoutExtension(currentWallpaperPath);
                string safeTitle = MakeSafeFileName(sourceName);
                string savePath = BuildUniqueFilePath(safeTitle, sourceExtension);

                byte[] imageBytes = await File.ReadAllBytesAsync(currentWallpaperPath);
                await File.WriteAllBytesAsync(savePath, imageBytes);

                Debug.WriteLine($"已从当前桌面壁纸保存: {currentWallpaperPath} -> {savePath}");
                return savePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存当前桌面壁纸失败，回退到必应API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 构建不重名的壁纸保存路径
        /// </summary>
        /// <param name="safeTitle">安全文件名标题</param>
        /// <param name="extension">文件扩展名（包含.）</param>
        /// <returns>可用文件路径</returns>
        private string BuildUniqueFilePath(string safeTitle, string extension)
        {
            string normalizedExtension = extension.StartsWith(".")
                ? extension
                : $".{extension}";

            string fileName = $"{DateTime.Now:yyyyMMdd}_{safeTitle}{normalizedExtension}";
            string filePath = Path.Combine(_savePath, fileName);

            int counter = 1;
            while (File.Exists(filePath))
            {
                fileName = $"{DateTime.Now:yyyyMMdd}_{safeTitle}_{counter}{normalizedExtension}";
                filePath = Path.Combine(_savePath, fileName);
                counter++;
            }

            return filePath;
        }

        /// <summary>
        /// 判断路径是否很可能来自必应壁纸来源
        /// </summary>
        /// <param name="wallpaperPath">壁纸路径</param>
        /// <returns>是否为必应来源</returns>
        private static bool IsLikelyBingWallpaper(string wallpaperPath)
        {
            if (string.IsNullOrWhiteSpace(wallpaperPath))
            {
                return false;
            }

            string lowerPath = wallpaperPath.ToLowerInvariant();
            string fileName = Path.GetFileName(lowerPath);

            return lowerPath.Contains("microsoft.bingwallpaper") ||
                   lowerPath.Contains($"{Path.DirectorySeparatorChar}bing{Path.DirectorySeparatorChar}") ||
                   fileName.Contains("_bing");
        }

        /// <summary>
        /// 构建必应壁纸API地址
        /// </summary>
        /// <param name="useUhd">是否请求高分辨率版本</param>
        /// <returns>API地址</returns>
        private static string BuildBingApiUrl(bool useUhd)
        {
            if (!useUhd)
            {
                return BING_API_BASE_URL;
            }

            return $"{BING_API_BASE_URL}&uhd=1&uhdwidth={PREFERRED_UHD_WIDTH}&uhdheight={PREFERRED_UHD_HEIGHT}";
        }

        /// <summary>
        /// 解析API响应并提取壁纸信息
        /// </summary>
        /// <param name="response">API响应内容</param>
        /// <returns>壁纸信息（候选URL列表和标题）</returns>
        private static (List<string> urlCandidates, string title)? ParseWallpaperInfo(string response)
        {
            using JsonDocument doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("images", out JsonElement images) || images.GetArrayLength() == 0)
            {
                Debug.WriteLine("JSON中未找到images数组");
                return null;
            }

            var image = images[0];
            List<string> urlCandidates = BuildWallpaperUrlCandidates(image);
            if (urlCandidates.Count == 0)
            {
                Debug.WriteLine("未构建出有效的壁纸URL");
                return null;
            }

            string title = ExtractWallpaperTitle(image);
            Debug.WriteLine($"获取到壁纸: {title}, 候选URL数量: {urlCandidates.Count}");
            Debug.WriteLine($"首选下载地址: {urlCandidates[0]}");
            return (urlCandidates, title);
        }

        /// <summary>
        /// 构建壁纸下载候选地址（高分辨率优先）
        /// </summary>
        /// <param name="image">必应图片信息节点</param>
        /// <returns>候选地址列表</returns>
        private static List<string> BuildWallpaperUrlCandidates(JsonElement image)
        {
            var candidates = new List<string>();
            var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddCandidate(string? rawUrl)
            {
                if (string.IsNullOrWhiteSpace(rawUrl))
                {
                    return;
                }

                string fullUrl =
                    rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        ? rawUrl
                        : $"https://www.bing.com{rawUrl}";

                if (dedup.Add(fullUrl))
                {
                    candidates.Add(fullUrl);
                }
            }

            string? primaryRelativeUrl = image.TryGetProperty("url", out JsonElement urlElement)
                ? urlElement.GetString()
                : null;

            string? urlBase = image.TryGetProperty("urlbase", out JsonElement urlBaseElement)
                ? urlBaseElement.GetString()
                : null;

            bool primaryIsUhd = !string.IsNullOrWhiteSpace(primaryRelativeUrl) &&
                                primaryRelativeUrl.Contains("_UHD", StringComparison.OrdinalIgnoreCase);

            // 如果主URL已经是UHD，优先保留该参数化地址；否则先尝试手工拼接UHD地址。
            if (primaryIsUhd)
            {
                AddCandidate(primaryRelativeUrl);
            }

            if (!string.IsNullOrWhiteSpace(urlBase))
            {
                AddCandidate($"{urlBase}_UHD.jpg&rf=LaDigue_UHD.jpg&pid=hp&w={PREFERRED_UHD_WIDTH}&h={PREFERRED_UHD_HEIGHT}&rs=1&c=4");
                AddCandidate($"{urlBase}_UHD.jpg");
            }

            AddCandidate(primaryRelativeUrl);

            if (!string.IsNullOrWhiteSpace(urlBase))
            {
                AddCandidate($"{urlBase}_1920x1080.jpg");
            }

            return candidates;
        }

        /// <summary>
        /// 提取壁纸标题
        /// </summary>
        /// <param name="image">必应图片信息节点</param>
        /// <returns>标题</returns>
        private static string ExtractWallpaperTitle(JsonElement image)
        {
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

            return title;
        }

        #endregion
    }
}

