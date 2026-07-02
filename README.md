# BetterGenshinImpact.OneKeyMacro — 一键宏（按角色）独立版

从 [BetterGenshinImpact](https://github.com/babalae/better-genshin-impact) 中提取的"一键宏（按角色）"功能，独立为轻量级 Windows 桌面程序。用户手动选择出战角色，通过全局热键触发键盘鼠标宏脚本循环执行，**无需图像识别**。

---

## 项目结构

```
├── BetterGenshinImpact.OneKeyMacro.sln    # 解决方案文件
├── README.md                               # 本文件
├── BetterGenshinImpact.OneKeyMacro/        # 主程序源码
│   ├── Assets/
│   │   ├── combat_avatar.json             # 角色数据（启动时加载）
│   │   └── avatar_macro.json              # 角色宏配置（启动时自动加载，可选）
│   ├── Config/DefaultOneKeyMacroConfig.cs  # 角色数据加载器
│   ├── Helper/
│   │   ├── KeyboardLowLevelHook.cs        # 低级键盘钩子（Hold 模式）
│   │   └── User32Helper.cs                # VK 虚拟键助手
│   ├── Model/
│   │   ├── AvatarMacro.cs                 # 角色宏配置模型
│   │   ├── CombatAvatar.cs               # 角色信息模型
│   │   ├── CombatCommand.cs              # 单条战斗指令
│   │   ├── Method.cs                      # 指令方法枚举
│   │   └── UserSettings.cs               # 用户持久化设置
│   ├── Script/CombatScriptParser.cs       # 宏脚本解析器
│   ├── Service/OneKeyMacroService.cs      # 核心服务（热键+宏循环）
│   ├── ViewModel/MainViewModel.cs         # MVVM 视图模型
│   ├── App.xaml / App.xaml.cs             # 应用入口
│   ├── MainWindow.xaml / .cs              # 主窗口
│   └── README.md                           # 详细使用文档
├── Fischless.WindowsInput/                # 键盘鼠标模拟库（项目引用）
├── Fischless.HotkeyCapture/               # 全局热键监听库（项目引用）
└── bin/Release/                           # ★ Release 编译产物
```

---

## 功能特性

- **角色选择**：下拉搜索 100+ 原神角色（支持别名）
- **宏编号**：每个角色支持 5 个独立宏脚本（编号 1-5）
- **热键监听**：
  - **Hold 模式** — 按住热键时循环执行宏，松开立即停止
  - **Toggle 模式** — 按一次启动，再按一次停止
- **宏脚本语法**：与原 BetterGI 兼容，支持 `attack` / `charge` / `dash` / `jump` / `walk` / `wait` / `moveby` / `keydown` / `mousedown` / `scroll` / `round` 等指令
- **自动加载角色配置**：启动时自动检测 `Assets/avatar_macro.json` 并加载
- **设置记忆**：上次使用的热键、模式、角色、宏编号自动保存到 `%LocalAppData%\BetterGenshinImpact.OneKeyMacro\settings.json`
- **文件操作**：支持导入/导出 `.txt` 脚本和 BetterGI 兼容的 `avatar_macro.json` 角色配置

---

## 快速开始

### 📥 下载（推荐）

直接从 GitHub Releases 页面下载编译好的成品：

> **[GitHub Releases](https://github.com/YOUR_USERNAME/YOUR_REPO/releases)**

在 Releases 页面找到最新版本，下载 `BetterGenshinImpact.OneKeyMacro.zip`，解压后双击 `BetterGenshinImpact.OneKeyMacro.exe` 即可运行。

> **提示**：以管理员权限运行以注册全局热键。

> **⚠️ 防倒卖声明**：本程序为开源免费软件，**永久免费**，源代码完全公开。如果您是从淘宝、闲鱼、拼多多等平台**付费购买**的，您已经被骗了。请立即申请退款并举报商家。目前GitHub Releases 为唯一官方下载渠道。任何以"一键连招脚本""定制宏"等名义售卖本程序或 BetterGI 的行为一律为倒卖行为。

### 配置角色宏

将 BetterGI 的 `avatar_macro.json` 放入 `Assets/` 目录，启动程序时会自动加载。也可在界面中通过"加载角色配置"按钮手动导入。

### 从源码编译

```cmd
dotnet build BetterGenshinImpact.OneKeyMacro.sln -c Release
```

编译产物：
```
BetterGenshinImpact.OneKeyMacro\bin\Release\net8.0-windows10.0.22621.0\
```

### 依赖项

| 依赖 | 说明 |
|------|------|
| .NET 8.0 SDK | 编译环境 |
| Fischless.WindowsInput | 键盘鼠标输入模拟 |
| Fischless.HotkeyCapture | 全局热键捕获 |
| CommunityToolkit.Mvvm | MVVM 框架 |
| Newtonsoft.Json | JSON 序列化 |
| Vanara.PInvoke.User32 | Windows API 封装 |

---

## 配置文件说明

### `Assets/avatar_macro.json`（可选）
角色宏配置文件，与 BetterGI 兼容。启动时自动加载。格式示例：
```json
[
  {
    "Name": "胡桃",
    "MacroPriority": 1,
    "ScriptContent1": "attack, charge(0.8), jump, wait(0.2)"
  },
  {
    "Name": "那维莱特",
    "MacroPriority": 1,
    "ScriptContent1": "mousedown, wait(2), mouseup"
  }
]
```

### `Assets/combat_avatar.json`（必需）
角色数据文件，包含所有角色名和别名映射。从原 BetterGI 项目提取。

### 用户设置
自动保存在：
```
%LocalAppData%\BetterGenshinImpact.OneKeyMacro\settings.json
```
内容示例：
```json
{
  "HotkeyString": "Ctrl+F1",
  "HotkeyMode": "Hold",
  "SelectedAvatarName": "胡桃",
  "MacroPriority": 1
}
```

---

## 链接

- BetterGenshinImpact: https://github.com/babalae/better-genshin-impact
- 宏脚本语法文档: https://www.bettergi.com/feats/macro/onem.html