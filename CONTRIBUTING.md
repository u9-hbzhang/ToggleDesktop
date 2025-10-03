# 贡献指南

感谢您考虑为 ToggleDesktop 做出贡献！

## 如何贡献

### 报告 Bug

如果您发现 bug，请通过 [GitHub Issues](https://github.com/yourusername/ToggleDesktop/issues) 报告，包含以下信息：

1. **环境信息**
   - 操作系统版本（如：Windows 11 22H2）
   - .NET 版本
   - ToggleDesktop 版本

2. **重现步骤**
   - 详细的操作步骤
   - 预期行为
   - 实际行为

3. **其他信息**
   - 截图或录屏（如果有）
   - 错误日志
   - 相关配置

### 提出新功能

欢迎提出新功能建议！请通过 Issues 说明：

1. **功能描述** - 详细说明您想要的功能
2. **使用场景** - 为什么需要这个功能
3. **实现思路** - 如果有的话

### 提交代码

#### 开发环境设置

1. **安装依赖**
   ```bash
   # 需要 .NET 8.0 SDK
   dotnet --version
   ```

2. **克隆仓库**
   ```bash
   git clone https://github.com/yourusername/ToggleDesktop.git
   cd ToggleDesktop
   ```

3. **创建分支**
   ```bash
   git checkout -b feature/your-feature-name
   ```

#### 代码规范

1. **命名规范**
   - 类名：PascalCase（如：`DesktopIconManager`）
   - 方法名：PascalCase（如：`ToggleDesktopIcons`）
   - 私有字段：_camelCase（如：`_iconManager`）
   - 局部变量：camelCase（如：`isHidden`）

2. **注释规范**
   ```csharp
   /// <summary>
   /// 方法的简短描述
   /// </summary>
   /// <param name="paramName">参数说明</param>
   /// <returns>返回值说明</returns>
   public bool MethodName(string paramName)
   {
       // 代码逻辑注释
   }
   ```

3. **代码风格**
   - 使用 4 空格缩进
   - 花括号独占一行
   - 每行不超过 120 字符
   - 使用 `var` 当类型明显时

#### 提交规范

提交信息格式：
```
<type>: <subject>

<body>

<footer>
```

**Type 类型：**
- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建/工具相关

**示例：**
```
feat: 添加壁纸自动下载功能

- 实现定时任务
- 添加设置选项
- 更新UI界面

Closes #123
```

#### Pull Request 流程

1. **确保代码质量**
   ```bash
   # 编译通过
   dotnet build
   
   # 运行测试
   dotnet test
   ```

2. **提交 PR**
   - 清晰描述改动内容
   - 关联相关 Issue
   - 添加截图（如果UI有改动）

3. **代码审查**
   - 响应审查意见
   - 及时更新代码

4. **合并**
   - 审查通过后将被合并

### 文档贡献

文档同样重要！您可以：

- 改进现有文档
- 翻译文档
- 添加使用示例
- 完善 API 文档

## 开发资源

### 项目架构

- **Core/** - 核心业务逻辑
- **UI/** - 用户界面
- **Utils/** - 工具类
- **Test/** - 测试代码

### 关键技术

- Windows API 调用
- COM 互操作
- 全局热键注册
- HTTP 异步请求
- 系统托盘编程

### 有用的链接

- [Windows API 文档](https://docs.microsoft.com/en-us/windows/win32/api/)
- [.NET 文档](https://docs.microsoft.com/en-us/dotnet/)
- [C# 编码规范](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## 行为准则

请遵守以下准则：

1. **友善和尊重** - 尊重所有贡献者
2. **建设性反馈** - 提供有帮助的建议
3. **开放包容** - 欢迎不同观点
4. **专注问题** - 就事论事

## 获取帮助

如有疑问，可以：

- 提 Issue 询问
- 在 Discussion 讨论
- 发送邮件：u9_hbzhang@outlook.com

## 感谢

感谢每一位贡献者！🎉

您的贡献让 ToggleDesktop 变得更好！

---

再次感谢您的贡献！❤️

