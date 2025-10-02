using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ToggleDesktop.UI
{
    /// <summary>
    /// 图标帮助类，用于创建系统托盘图标
    /// </summary>
    public static class IconHelper
    {
        /// <summary>
        /// 创建桌面图标显示状态的托盘图标
        /// </summary>
        /// <param name="isHidden">桌面图标是否隐藏</param>
        /// <returns>系统托盘图标</returns>
        public static Icon CreateTrayIcon(bool isHidden)
        {
            const int size = 16;
            using (var bitmap = new Bitmap(size, size))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                if (isHidden)
                {
                    // 隐藏状态：绘制一个带斜线的桌面图标
                    DrawDesktopIcon(graphics, size, Color.Gray);
                    DrawDiagonalLine(graphics, size, Color.Red);
                }
                else
                {
                    // 显示状态：绘制正常的桌面图标
                    DrawDesktopIcon(graphics, size, Color.DodgerBlue);
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }

        /// <summary>
        /// 绘制桌面图标样式
        /// </summary>
        /// <param name="graphics">绘图对象</param>
        /// <param name="size">图标大小</param>
        /// <param name="color">图标颜色</param>
        private static void DrawDesktopIcon(Graphics graphics, int size, Color color)
        {
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(Color.White, 1f))
            {
                // 绘制主体矩形
                int margin = 2;
                int iconSize = size - margin * 2;
                Rectangle rect = new Rectangle(margin, margin, iconSize, iconSize);
                
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(pen, rect);

                // 绘制小图标网格
                using (var gridPen = new Pen(Color.White, 0.5f))
                {
                    int gridSize = iconSize / 4;
                    for (int x = 1; x < 4; x++)
                    {
                        graphics.DrawLine(gridPen, 
                            margin + x * gridSize, margin,
                            margin + x * gridSize, margin + iconSize);
                    }
                    for (int y = 1; y < 4; y++)
                    {
                        graphics.DrawLine(gridPen,
                            margin, margin + y * gridSize,
                            margin + iconSize, margin + y * gridSize);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制对角线（表示隐藏状态）
        /// </summary>
        /// <param name="graphics">绘图对象</param>
        /// <param name="size">图标大小</param>
        /// <param name="color">线条颜色</param>
        private static void DrawDiagonalLine(Graphics graphics, int size, Color color)
        {
            using (var pen = new Pen(color, 2f))
            {
                graphics.DrawLine(pen, 0, 0, size, size);
            }
        }

        /// <summary>
        /// 创建设置图标
        /// </summary>
        /// <returns>设置图标</returns>
        public static Icon CreateSettingsIcon()
        {
            const int size = 16;
            using (var bitmap = new Bitmap(size, size))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                // 绘制齿轮图标
                using (var brush = new SolidBrush(Color.DarkGray))
                {
                    int center = size / 2;
                    int radius = size / 3;
                    
                    // 简化的齿轮：中心圆 + 8个小矩形
                    graphics.FillEllipse(brush, center - radius/2, center - radius/2, radius, radius);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        int x = (int)(center + Math.Cos(angle) * radius * 0.8);
                        int y = (int)(center + Math.Sin(angle) * radius * 0.8);
                        graphics.FillRectangle(brush, x - 1, y - 1, 2, 2);
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }

        /// <summary>
        /// 创建应用程序主图标
        /// </summary>
        /// <returns>应用程序图标</returns>
        public static Icon CreateAppIcon()
        {
            const int size = 32;
            using (var bitmap = new Bitmap(size, size))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                // 绘制大的桌面图标
                DrawDesktopIcon(graphics, size, Color.DodgerBlue);

                // 添加装饰性元素
                using (var brush = new SolidBrush(Color.Orange))
                {
                    int starSize = 4;
                    graphics.FillEllipse(brush, size - starSize - 2, 2, starSize, starSize);
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}
