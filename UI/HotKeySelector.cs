using System;
using System.Drawing;
using System.Windows.Forms;
using ToggleDesktop.Core;

namespace ToggleDesktop.UI
{
    /// <summary>
    /// 热键选择控件
    /// </summary>
    public class HotKeySelector : UserControl
    {
        #region 私有字段

        private TextBox _textBox = null!;
        private Button _clearButton = null!;
        private Label _statusLabel = null!;
        private uint _modifiers = 0;
        private Keys _key = Keys.None;
        private bool _isRecording = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 热键字符串
        /// </summary>
        public string HotKeyString
        {
            get
            {
                if (_key == Keys.None)
                    return "";
                return HotKeyManager.GetHotKeyString(_modifiers, _key);
            }
            set
            {
                SetHotKey(value);
            }
        }

        /// <summary>
        /// 修饰键
        /// </summary>
        public uint Modifiers => _modifiers;

        /// <summary>
        /// 主键
        /// </summary>
        public Keys Key => _key;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _key == Keys.None;

        #endregion

        #region 事件

        /// <summary>
        /// 热键更改事件
        /// </summary>
        public event EventHandler? HotKeyChanged;

        #endregion

        #region 构造函数

        public HotKeySelector()
        {
            InitializeComponent();
        }

        #endregion

        #region 初始化

        private void InitializeComponent()
        {
            this.Size = new Size(200, 60);
            this.MinimumSize = new Size(180, 60);

            // 文本框
            _textBox = new TextBox()
            {
                Location = new Point(0, 0),
                Size = new Size(140, 23),
                ReadOnly = false, // 设为false以接收键盘事件
                PlaceholderText = "点击录制热键...",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TabStop = true // 确保可以获得焦点
            };
            _textBox.Click += TextBox_Click;
            _textBox.KeyDown += TextBox_KeyDown;
            _textBox.KeyUp += TextBox_KeyUp;
            _textBox.Enter += TextBox_Enter; // 获得焦点时开始录制
            _textBox.TextChanged += TextBox_TextChanged; // 防止用户直接输入
            _textBox.PreviewKeyDown += TextBox_PreviewKeyDown; // 更早的按键捕获

            // 清除按钮
            _clearButton = new Button()
            {
                Text = "清除",
                Location = new Point(145, 0),
                Size = new Size(50, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _clearButton.Click += ClearButton_Click;

            // 状态标签
            _statusLabel = new Label()
            {
                Text = "未设置热键",
                Location = new Point(0, 28),
                Size = new Size(200, 15),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            this.Controls.AddRange(new Control[] { _textBox, _clearButton, _statusLabel });

            // 更新显示
            UpdateDisplay();
        }

        #endregion

        #region 事件处理

        private void TextBox_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            // 当TextBox获得焦点时自动开始录制
            if (!_isRecording)
            {
                StartRecording();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"TextBox_PreviewKeyDown触发: KeyCode={e.KeyCode}, 修饰键={e.Modifiers}, 录制状态={_isRecording}");
            
            if (_isRecording)
            {
                // 确保所有按键都被处理
                e.IsInputKey = true;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"TextBox_KeyDown触发: KeyCode={e.KeyCode}, 录制状态={_isRecording}");
            
            if (!_isRecording)
            {
                System.Diagnostics.Debug.WriteLine("不在录制状态，忽略按键");
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            HandleKeyInput(e);
        }

        /// <summary>
        /// 处理按键输入的通用方法
        /// </summary>
        private void HandleKeyInput(KeyEventArgs e)
        {
            if (!_isRecording)
                return;

            // 记录修饰键
            _modifiers = 0;
            if (e.Control) _modifiers |= HotKeyManager.MOD_CONTROL;
            if (e.Alt) _modifiers |= HotKeyManager.MOD_ALT;
            if (e.Shift) _modifiers |= HotKeyManager.MOD_SHIFT;

            System.Diagnostics.Debug.WriteLine($"修饰键: Ctrl={e.Control}, Alt={e.Alt}, Shift={e.Shift}");

            // 记录主键（排除单独的修饰键）
            Keys keyCode = e.KeyCode;
            if (keyCode != Keys.ControlKey && keyCode != Keys.ShiftKey && 
                keyCode != Keys.Menu && keyCode != Keys.LWin && keyCode != Keys.RWin)
            {
                _key = keyCode;
                System.Diagnostics.Debug.WriteLine($"热键录制完成: {HotKeyManager.GetHotKeyString(_modifiers, _key)}");
                StopRecording();
                OnHotKeyChanged();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"忽略修饰键: {keyCode}");
            }

            UpdateDisplay();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"TextBox_KeyUp触发: KeyCode={e.KeyCode}, 录制状态={_isRecording}");
            
            if (_isRecording)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                
                // 如果KeyDown没有触发，在KeyUp中处理按键
                HandleKeyInput(e);
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            // 防止用户直接输入文本，只允许热键录制修改
            if (!_isRecording && _textBox.Text != HotKeyString && !string.IsNullOrEmpty(_textBox.Text))
            {
                // 如果不是在录制状态且文本被意外修改，恢复正确的文本
                _textBox.Text = HotKeyString;
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearHotKey();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置热键
        /// </summary>
        /// <param name="hotKeyString">热键字符串</param>
        public void SetHotKey(string hotKeyString)
        {
            if (string.IsNullOrWhiteSpace(hotKeyString))
            {
                ClearHotKey();
                return;
            }

            if (HotKeyManager.ParseHotKeyString(hotKeyString, out uint modifiers, out Keys key))
            {
                _modifiers = modifiers;
                _key = key;
                UpdateDisplay();
                OnHotKeyChanged();
            }
        }

        /// <summary>
        /// 清除热键
        /// </summary>
        public void ClearHotKey()
        {
            _modifiers = 0;
            _key = Keys.None;
            UpdateDisplay();
            OnHotKeyChanged();
        }

        /// <summary>
        /// 开始录制热键
        /// </summary>
        public void StartRecording()
        {
            _isRecording = true;
            _textBox.Text = "请按下热键组合...";
            _textBox.BackColor = Color.LightYellow;
            _textBox.SelectAll(); // 全选文本
            _textBox.Focus();
            _statusLabel.Text = "正在录制热键，按下组合键...";
            _statusLabel.ForeColor = Color.Blue;
            
            // 添加调试信息
            System.Diagnostics.Debug.WriteLine("开始录制热键，TextBox已获得焦点");
        }

        /// <summary>
        /// 停止录制热键
        /// </summary>
        public void StopRecording()
        {
            _isRecording = false;
            _textBox.BackColor = SystemColors.Window;
        }

        /// <summary>
        /// 验证当前热键是否可用
        /// </summary>
        /// <returns>热键是否可用</returns>
        public bool ValidateHotKey()
        {
            if (IsEmpty)
                return true;

            // 使用智能测试方法，排除当前程序已注册的热键
            return HotKeyManager.Instance.TestHotKeyAvailability(_modifiers, _key);
        }

        #endregion

        #region 私有方法

        private void UpdateDisplay()
        {
            if (IsEmpty)
            {
                _textBox.Text = "";
                _statusLabel.Text = "未设置热键";
                _statusLabel.ForeColor = Color.Gray;
            }
            else
            {
                _textBox.Text = HotKeyString;
                
                // 验证热键可用性
                if (_isRecording)
                {
                    _statusLabel.Text = "正在录制热键...";
                    _statusLabel.ForeColor = Color.Blue;
                }
                else
                {
                    bool isRegisteredByThis = HotKeyManager.Instance.IsHotKeyRegisteredByThis(_modifiers, _key);
                    bool isAvailable = ValidateHotKey();
                    
                    if (isRegisteredByThis)
                    {
                        _statusLabel.Text = "当前使用的热键";
                        _statusLabel.ForeColor = Color.Green;
                    }
                    else if (isAvailable)
                    {
                        _statusLabel.Text = "热键可用";
                        _statusLabel.ForeColor = Color.Green;
                    }
                    else
                    {
                        _statusLabel.Text = "热键已被占用";
                        _statusLabel.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void OnHotKeyChanged()
        {
            HotKeyChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 重写方法

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_textBox != null && _clearButton != null)
            {
                _textBox.Width = this.Width - _clearButton.Width - 5;
                _clearButton.Left = _textBox.Right + 5;
                
                if (_statusLabel != null)
                {
                    _statusLabel.Width = this.Width;
                }
            }
        }

        #endregion
    }
}
