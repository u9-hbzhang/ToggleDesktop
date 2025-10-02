using System;
using System.Drawing;
using System.Windows.Forms;
using ToggleDesktop.Core;
using ToggleDesktop.Utils;

namespace ToggleDesktop.UI
{
    /// <summary>
    /// 设置窗口
    /// </summary>
    public partial class SettingsForm : Form
    {
        private DesktopIconManager _manager;
        private SettingsManager _settings;

        /// <summary>
        /// 热键更改事件
        /// </summary>
        public event Action<string>? OnHotKeyChanged;

        #region 控件
        private TabControl tabControl = null!;
        private TabPage generalTab = null!;
        private TabPage aboutTab = null!;
        
        // 通用设置控件
        private GroupBox statusGroupBox = null!;
        private Label currentStatusLabel = null!;
        private Label windowCountLabel = null!;
        private Button refreshButton = null!;
        
        private GroupBox operationGroupBox = null!;
        private Button toggleButton = null!;
        private Button showButton = null!;
        private Button hideButton = null!;
        
        private GroupBox settingsGroupBox = null!;
        private CheckBox autoStartCheckBox = null!;
        private CheckBox showNotificationsCheckBox = null!;
        private Label hotKeyLabel = null!;
        private HotKeySelector hotKeySelector = null!;
        
        // 关于页面控件
        private PictureBox logoBox = null!;
        private Label titleLabel = null!;
        private Label versionLabel = null!;
        private Label descriptionLabel = null!;
        private Label copyrightLabel = null!;
        private LinkLabel githubLinkLabel = null!;
        
        private Button okButton = null!;
        private Button cancelButton = null!;
        #endregion

        public SettingsForm()
        {
            _manager = DesktopIconManager.Instance;
            _settings = SettingsManager.Instance;
            
            InitializeComponent();
            LoadSettings();
            UpdateStatus();
            
            // 订阅状态变化事件
            _manager.OnDesktopIconsStateChanged += OnDesktopIconStateChanged;
        }

        private void InitializeComponent()
        {
            // 窗口基本设置
            this.Text = "ToggleDesktop 设置";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Icon = IconHelper.CreateSettingsIcon();

            // 创建主布局
            CreateTabControl();
            CreateGeneralTab();
            CreateAboutTab();
            CreateButtons();
            
            // 设置布局
            this.Controls.Add(tabControl);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }

        private void CreateTabControl()
        {
            tabControl = new TabControl()
            {
                Location = new Point(12, 12),
                Size = new Size(460, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            generalTab = new TabPage("常规设置");
            aboutTab = new TabPage("关于");
            
            tabControl.TabPages.Add(generalTab);
            tabControl.TabPages.Add(aboutTab);
        }

        private void CreateGeneralTab()
        {
            int yOffset = 20;

            // 状态信息组
            statusGroupBox = new GroupBox()
            {
                Text = "当前状态",
                Location = new Point(20, yOffset),
                Size = new Size(420, 80)
            };

            currentStatusLabel = new Label()
            {
                Location = new Point(15, 25),
                Size = new Size(200, 20),
                Text = "桌面图标状态: 检测中..."
            };

            windowCountLabel = new Label()
            {
                Location = new Point(15, 45),
                Size = new Size(200, 20),
                Text = "检测到窗口: 0 个"
            };

            refreshButton = new Button()
            {
                Text = "刷新状态",
                Location = new Point(300, 25),
                Size = new Size(80, 30)
            };
            refreshButton.Click += RefreshButton_Click;

            statusGroupBox.Controls.AddRange(new Control[] { currentStatusLabel, windowCountLabel, refreshButton });

            yOffset += 100;

            // 操作控制组
            operationGroupBox = new GroupBox()
            {
                Text = "快速操作",
                Location = new Point(20, yOffset),
                Size = new Size(420, 60)
            };

            toggleButton = new Button()
            {
                Text = "切换显示",
                Location = new Point(15, 25),
                Size = new Size(80, 25)
            };
            toggleButton.Click += ToggleButton_Click;

            showButton = new Button()
            {
                Text = "显示图标",
                Location = new Point(105, 25),
                Size = new Size(80, 25)
            };
            showButton.Click += ShowButton_Click;

            hideButton = new Button()
            {
                Text = "隐藏图标",
                Location = new Point(195, 25),
                Size = new Size(80, 25)
            };
            hideButton.Click += HideButton_Click;

            operationGroupBox.Controls.AddRange(new Control[] { toggleButton, showButton, hideButton });

            yOffset += 80;

            // 设置选项组
            settingsGroupBox = new GroupBox()
            {
                Text = "程序设置",
                Location = new Point(20, yOffset),
                Size = new Size(420, 160)
            };

            autoStartCheckBox = new CheckBox()
            {
                Text = "开机自动启动",
                Location = new Point(15, 25),
                Size = new Size(120, 20)
            };
            autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;

            showNotificationsCheckBox = new CheckBox()
            {
                Text = "显示通知消息",
                Location = new Point(15, 50),
                Size = new Size(120, 20),
                Checked = true
            };
            showNotificationsCheckBox.CheckedChanged += ShowNotificationsCheckBox_CheckedChanged;

            hotKeyLabel = new Label()
            {
                Text = "全局热键:",
                Location = new Point(15, 80),
                Size = new Size(70, 20)
            };

            hotKeySelector = new HotKeySelector()
            {
                Location = new Point(90, 80),
                Size = new Size(250, 65)
            };
            hotKeySelector.HotKeyChanged += HotKeySelector_HotKeyChanged;

            settingsGroupBox.Controls.AddRange(new Control[] { 
                autoStartCheckBox, showNotificationsCheckBox, 
                hotKeyLabel, hotKeySelector 
            });

            generalTab.Controls.AddRange(new Control[] { statusGroupBox, operationGroupBox, settingsGroupBox });
        }

        private void CreateAboutTab()
        {
            // Logo
            logoBox = new PictureBox()
            {
                Location = new Point(180, 20),
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            // 使用默认图标
            logoBox.Image = IconHelper.CreateAppIcon().ToBitmap();

            // 标题
            titleLabel = new Label()
            {
                Text = "ToggleDesktop",
                Location = new Point(150, 100),
                Size = new Size(140, 30),
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 版本
            versionLabel = new Label()
            {
                Text = "版本 1.0.0",
                Location = new Point(150, 135),
                Size = new Size(140, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 描述
            descriptionLabel = new Label()
            {
                Text = "一键切换Windows桌面图标显示/隐藏的工具\n让您更好地欣赏精美壁纸",
                Location = new Point(50, 170),
                Size = new Size(340, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 版权
            copyrightLabel = new Label()
            {
                Text = "Copyright © 2024 ToggleDesktop",
                Location = new Point(50, 220),
                Size = new Size(340, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };

            // GitHub链接
            githubLinkLabel = new LinkLabel()
            {
                Text = "访问项目主页",
                Location = new Point(170, 250),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            githubLinkLabel.Click += GithubLinkLabel_Click;

            aboutTab.Controls.AddRange(new Control[] { 
                logoBox, titleLabel, versionLabel, descriptionLabel, copyrightLabel, githubLinkLabel 
            });
        }

        private void CreateButtons()
        {
            okButton = new Button()
            {
                Text = "确定",
                Location = new Point(315, 405),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            okButton.Click += OkButton_Click;

            cancelButton = new Button()
            {
                Text = "取消",
                Location = new Point(400, 405),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            cancelButton.Click += CancelButton_Click;
        }

        #region 事件处理

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            _manager.RefreshDesktopWindows();
            UpdateStatus();
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            _manager.ToggleDesktopIcons();
            UpdateStatus();
        }

        private void ShowButton_Click(object sender, EventArgs e)
        {
            _manager.ShowDesktopIcons();
            UpdateStatus();
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            _manager.HideDesktopIcons();
            UpdateStatus();
        }

        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool newState = autoStartCheckBox.Checked;
            
            // 尝试设置自启动
            bool success = AutoStartManager.SetAutoStart(newState);
            
            if (success)
            {
                _settings.AutoStart = newState;
                _settings.Save();
            }
            else
            {
                // 设置失败，恢复checkbox状态
                autoStartCheckBox.CheckedChanged -= AutoStartCheckBox_CheckedChanged;
                autoStartCheckBox.Checked = !newState;
                autoStartCheckBox.CheckedChanged += AutoStartCheckBox_CheckedChanged;
            }
        }

        private void ShowNotificationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _settings.ShowNotifications = showNotificationsCheckBox.Checked;
            _settings.Save();
        }

        private void HotKeySelector_HotKeyChanged(object? sender, EventArgs e)
        {
            if (hotKeySelector != null)
            {
                string newHotKey = hotKeySelector.HotKeyString;
                _settings.HotKey = newHotKey;
                _settings.Save();
                
                // 通知TrayManager重新注册热键
                // 这里我们需要通过事件或其他方式通知
                OnHotKeyChanged?.Invoke(newHotKey);
            }
        }

        private void GithubLinkLabel_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/toggledesktop/toggledesktop",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnDesktopIconStateChanged(bool isHidden)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus()));
            }
            else
            {
                UpdateStatus();
            }
        }

        #endregion

        #region 私有方法

        private void LoadSettings()
        {
            // 同步自启动状态（优先使用注册表的实际状态）
            bool actualAutoStart = AutoStartManager.IsAutoStartEnabled();
            _settings.AutoStart = actualAutoStart;
            
            autoStartCheckBox.Checked = _settings.AutoStart;
            showNotificationsCheckBox.Checked = _settings.ShowNotifications;
            hotKeySelector.HotKeyString = _settings.HotKey;
        }

        private void SaveSettings()
        {
            _settings.AutoStart = autoStartCheckBox.Checked;
            _settings.ShowNotifications = showNotificationsCheckBox.Checked;
            _settings.Save();
        }

        private void UpdateStatus()
        {
            currentStatusLabel.Text = $"桌面图标状态: {(_manager.IsHidden ? "隐藏" : "显示")}";
            windowCountLabel.Text = $"检测到窗口: {_manager.GetDesktopWindowCount()} 个";
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _manager.OnDesktopIconsStateChanged -= OnDesktopIconStateChanged;
            }
            base.Dispose(disposing);
        }
    }
}
