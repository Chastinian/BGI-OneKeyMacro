using System;

namespace BetterGenshinImpact.OneKeyMacro.Model;

/// <summary>
/// 用户持久化设置
/// </summary>
[Serializable]
public class UserSettings
{
    public string HotkeyString { get; set; } = "Ctrl+F1";
    public string HotkeyMode { get; set; } = "Hold";
    public string SelectedAvatarName { get; set; } = string.Empty;
    public int MacroPriority { get; set; } = 1;
}