# 一键宏（按角色） - BetterGenshinImpact 独立版

从 [BetterGenshinImpact](https://github.com/babalae/better-genshin-impact) 中提取的"一键宏（按角色）"功能独立程序。按角色执行键盘鼠标宏脚本，无需图像识别——用户手动选择当前出战的角色。

## 快速开始

### 📥 下载（推荐）

直接从 GitHub Releases 页面下载编译好的成品：

> **[GitHub Releases](https://github.com/YOUR_USERNAME/YOUR_REPO/releases)**

在 Releases 页面找到最新版本，下载 `BetterGenshinImpact.OneKeyMacro.zip`，解压后双击 `BetterGenshinImpact.OneKeyMacro.exe` 即可运行。

> **⚠️ 防倒卖声明**：本程序为开源免费软件，**永久免费**，源代码完全公开。如果您是从淘宝、闲鱼、拼多多等平台**付费购买**的，您已经被骗了。请立即申请退款并举报商家。目前 GitHub Releases 为唯一官方下载渠道。
> 
> 本项目同时基于 BetterGenshinImpact 的组件开发，BetterGI 同样是完全免费的开源项目。如有任何平台或个人以"一键连招脚本""定制宏"等名义售卖本程序或 BetterGI，一律为倒卖行为。

### 从源码编译

```cmd
dotnet build BetterGenshinImpact.OneKeyMacro.csproj -c Release
```

编译产物：
```
bin\Release\net8.0-windows10.0.22621.0\BetterGenshinImpact.OneKeyMacro.exe
```

### 运行

直接双击编译产物或下载的 exe，或：

```cmd
dotnet run --project BetterGenshinImpact.OneKeyMacro.csproj -c Debug
```

> **注意**：程序需要以管理员权限运行才能注册全局热键。如果使用 Hold 模式，建议以管理员权限运行。

---

## 界面说明

程序窗口分为五个区域：

```
┌─────────────────────────────────────────────┐
│ 角色选择                                     │
│  当前角色: [      下拉搜索框      ]  [编号▼] │
├─────────────────────────────────────────────┤
│ 快捷键设置                                   │
│  热键: [ Ctrl+F1 ]  模式: [Hold ▼]  ☑ 启用  │
├─────────────────────────────────────────────┤
│ 宏脚本                                       │
│  [打开] [保存] [加载角色配置] [保存角色配置] [帮助] │
│  ┌───────────────────────────────────────┐  │
│  │  attack(0.5), wait(0.3), dash         │  │
│  └───────────────────────────────────────┘  │
├─────────────────────────────────────────────┤
│ 脚本语法参考                                 │
│  （常用命令速查）                             │
├─────────────────────────────────────────────┤
│ [状态文本]                    [启动 / 停止] │
└─────────────────────────────────────────────┘
```

---

## 功能详解

### 1. 选择角色

在"角色选择"下拉框中输入或选择角色名称。内置了来自原 BetterGI 项目 `combat_avatar.json` 的 100+ 原神角色列表，支持别名搜索。

右侧的数字框（1-5）是**宏编号**，用于在同个角色上切换不同宏脚本。

### 2. 修改热键

在"快捷键设置"区域：
- **热键输入框**：支持组合键格式 `Ctrl+F1`、`Alt+A`、`Shift+Q` 等
- **模式下拉框**：
  - `Hold` — 按住热键时循环执行宏，松开即停止
  - `Toggle` — 按一次启动宏，再按一次停止
- **启用复选框**：关闭后热键不会触发宏

修改热键文本后会自动重新注册。如果注册失败，请检查热键是否被其他程序占用，或是否以管理员权限运行。

> **⚠️ 注意**：热键输入框中的按键名需使用 **.NET Keys 枚举名**（如 `F1`、`Space`），不是 VK 码。完整列表：[Keys Enum](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys)

> **💡 启动时自动加载**：如果 `Assets/avatar_macro.json` 存在，程序会在启动时自动加载该文件中的所有角色宏配置，无需手动导入。角色选择后会自动显示对应的宏脚本。

### 设置记忆

程序会自动保存您上次的设置到：
```
%LocalAppData%\BetterGenshinImpact.OneKeyMacro\settings.json
```

记忆内容包括：热键组合、模式（Hold/Toggle）、角色名称、宏编号。下次启动程序时会自动恢复。

### 3. 编写与编辑宏脚本

在"宏脚本"文本框中编写宏指令。指令以**英文逗号**分隔，每轮执行完毕自动进入下一轮循环。

**示例：**
```
attack(0.5), wait(0.3), dash
```

**文件操作按钮：**

| 按钮 | 功能 |
|------|------|
| **打开** | 从 `.txt` 文件加载宏脚本 |
| **保存** | 将当前脚本保存为 `.txt` 文件（自动以角色名命名） |
| **加载角色配置** | 从 BetterGI 兼容的 `avatar_macro.json` 导入角色宏配置 |
| **保存角色配置** | 将当前脚本保存到角色配置列表，并可导出为 JSON 文件 |
| **帮助** | 打开 BetterGI 宏脚本在线文档 |

### 4. 加载角色配置文件（导入 BetterGI 宏）

此功能兼容 BetterGenshinImpact 的 `avatar_macro.json` 格式。点击 **加载角色配置**，选择一个 JSON 文件即可。

配置文件中每个角色的宏按 **macroPriority**（或当前选中的宏编号 1-5）区分。导入后：
- 在角色下拉框中选择角色，会自动加载对应的脚本
- 切换宏编号（1-5），会切换到对应编号的脚本

### 5. 启动与停止

有两种方式控制宏的执行：
- 点击界面下方的 **启动 / 停止** 按钮
- 按下绑定的**全局热键**

运行状态会显示在左下角的状态文本中。

---

## 宏脚本语法

### 基础命令

| 命令 | 说明 | 示例 |
|------|------|------|
| `attack` | 点击一次左键（普通攻击） | `attack` |
| `attack(1.5)` | 持续左键攻击 1.5 秒 | `attack(2)` |
| `charge(1.5)` | 按住左键重击 1.5 秒 | `charge(2)` |
| `dash` | 冲刺（右键点击） | `dash` |
| `jump` | 跳跃（空格） | `jump` |
| `walk(w, 0.5)` | 向指定方向行走 (w/a/s/d) | `walk(a, 0.3)` |
| `w(0.5)` | 向前走 0.5 秒 | `w(1)` |
| `a(0.5)` | 向左走 0.5 秒 | `a(0.3)` |
| `s(0.5)` | 向后走 0.5 秒 | `s(0.5)` |
| `d(0.5)` | 向右走 0.5 秒 | `d(0.3)` |
| `wait(0.3)` | 等待 0.3 秒 | `wait(0.5)` |

### 鼠标与按键宏

| 命令 | 说明 | 示例 |
|------|------|------|
| `mousedown` | 按下鼠标左键 | `mousedown` |
| `mouseup` | 抬起鼠标左键 | `mouseup` |
| `click` | 鼠标左键点击 | `click` |
| `moveby(100, 0)` | 鼠标相对移动 (x, y) | `moveby(50, -50)` |
| `scroll(1)` | 滚轮向上滚动 1 格 | `scroll(3)` |
| `scroll(-1)` | 滚轮向下滚动 1 格 | `scroll(-2)` |
| `keydown(D)` | 按下键盘按键 (VK 码) | `keydown(LSHIFT)` |
| `keyup(D)` | 抬起键盘按键 | `keyup(LSHIFT)` |
| `keypress(D)` | 按下并抬起按键 | `keypress(SPACE)` |

### 高级语法：回合限定

用 `round()` 和 `|` 分隔不同回合的指令块：

```
round(1)| attack, wait(0.3), dash
round(2-4)| attack(1), jump
round(5)| charge(2)
```

- `round(1)` — 仅第 1 轮执行后续指令
- `round(2-4)` — 第 2~4 轮执行
- `round(1,3,5)` — 第 1、3、5 轮执行

### 常用宏示例

**胡桃 A重跳：**
```
attack, charge(0.8), jump, wait(0.2)
```

**那维莱特转圈：**
```
mousedown, wait(2), moveby(200, 0), wait(0.1), moveby(200, 0), wait(2), mouseup
```

**甘雨蓄力射箭：**
```
charge(2.5), wait(0.5)
```

**普通连招：**
```
attack(1), wait(0.1), dash, wait(0.1), attack(0.5), jump
```

---

## 按键码参考

`keydown` / `keyup` / `keypress` 使用 Windows 虚拟键码（VK 码）：

| 常用按键 | VK 码 |
|----------|-------|
| 数字 0-9 | `0` ~ `9` |
| 字母 A-Z | `A` ~ `Z` |
| 空格 | `SPACE` |
| 回车 | `RETURN` |
| 左 Shift | `LSHIFT` |
| 左 Ctrl | `LCONTROL` |
| 左 Alt | `LMENU` |
| Tab | `TAB` |
| Esc | `ESCAPE` |
| E | `E` |
| F | `F` |
| Q | `Q` |

完整列表参见 [Microsoft Virtual-Key Codes](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)。

---

## 热键格式

| 格式 | 示例 |
|------|------|
| 单键 | `F1`、`F12` |
| Ctrl + 键 | `Ctrl+F1` |
| Alt + 键 | `Alt+A` |
| Shift + 键 | `Shift+Q` |
| Win + 键 | `Win+T` |
| 多修饰键 | `Ctrl+Shift+F1` |

> **⚠️ 重要**：热键输入框中的按键名**必须使用 .NET Keys 枚举名**（如 `F1`、`A`、`Space`），**不能使用 VK 码**（如 `VK_F1` 是无效的）。这与宏脚本中的 `keydown`/`keyup` 命令不同（那些使用 VK 码）。常用按键名参考：`F1`~`F12`、`A`~`Z`、`0`~`9`、`Space`、`Tab`、`Escape`、`Enter`、`Up`/`Down`/`Left`/`Right`。
>
> 完整的 .NET Keys 枚举值列表请参阅：[Keys Enum (System.Windows.Forms)](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys)
>
> **提示**：热键注册失败时，请尝试以管理员权限运行程序。

---

## 配置文件与更新

### `Assets/avatar_macro.json`（可选，启动时自动加载）

角色宏配置文件，与 BetterGI 兼容。放在 `Assets/` 目录下即可在启动时自动加载。格式示例：

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

角色数据文件，包含所有角色名和别名映射。该文件随 BetterGI 项目更新而更新。

### 📥 更新角色数据以支持新角色

当游戏推出新角色后，本程序需要更新 `combat_avatar.json` 才能在角色列表中显示新角色。更新方法：

1. **从 BetterGI 获取最新文件**：前往 [BetterGenshinImpact 仓库](https://github.com/babalae/better-genshin-impact)，下载最新的：
   - `BetterGenshinImpact/GameTask/AutoFight/Assets/combat_avatar.json`
   - `BetterGenshinImpact/User/avatar_macro_default.json`（可选，包含默认宏）

2. **替换本地文件**：
   - 将 `combat_avatar.json` 覆盖到本程序的 `Assets/combat_avatar.json`
   - 将 `avatar_macro_default.json` 重命名为 `avatar_macro.json` 后放入 `Assets/` 目录（如果 BetterGI 已有新角色的默认宏）

3. **手动添加新角色**（如 BetterGI 尚未更新）：编辑 `Assets/combat_avatar.json`，参照现有条目的格式添加：
   ```json
   {
     "Id": "new_character_id",
     "Name": "新角色中文名",
     "NameEn": "NewCharacterEnName",
     "Weapon": "Sword",
     "SkillCd": 6.0,
     "SkillHoldCd": 10.0,
     "BurstCd": 20.0,
     "Alias": ["新角色", "昵称1", "昵称2"]
   }
   ```
   - `Name`：必须是中文角色名（程序用中文匹配）
   - `Alias`：该角色的所有别名（昵称、简称等），用于搜索

4. **重启程序**即可看到新角色。

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

可以手动编辑此文件来修改默认设置，也可以直接删除以恢复默认值。

---

## 兼容性

- **宏脚本语法**：与 BetterGenshinImpact 的宏系统兼容
- **角色配置文件**：可直接导入 BetterGI 的 `avatar_macro.json`
- **角色数据**：使用 BetterGI 的 `combat_avatar.json` 角色列表

---

## 构建说明

**依赖项：**

```
Fischless.WindowsInput    ← 键盘鼠标模拟
Fischless.HotkeyCapture   ← 全局热键监听
```

**NuGet 包：**
```
CommunityToolkit.Mvvm     ← MVVM 框架
Newtonsoft.Json           ← JSON 序列化
Vanara.PInvoke.User32     ← Windows API 调用
```

编译指令：
```cmd
dotnet build BetterGenshinImpact.OneKeyMacro.csproj -c Debug
```

输出路径：
```
bin\Release\net8.0-windows10.0.22621.0\BetterGenshinImpact.OneKeyMacro.exe