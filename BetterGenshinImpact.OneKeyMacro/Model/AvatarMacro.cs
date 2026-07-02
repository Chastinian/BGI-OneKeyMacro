using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BetterGenshinImpact.OneKeyMacro.Script;

namespace BetterGenshinImpact.OneKeyMacro.Model;

/// <summary>
/// 角色宏配置
/// </summary>
public class AvatarMacro
{
    public string Name { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScriptContent1 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScriptContent2 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScriptContent3 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScriptContent4 { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ScriptContent5 { get; set; }

    /// <summary>
    /// 角色当前使用的战斗宏编号 (1-5)，如果为0则使用默认宏1
    /// </summary>
    public int MacroPriority { get; set; } = 0;

    public string? GetScriptContent(int index)
    {
        return index switch
        {
            1 => ScriptContent1,
            2 => ScriptContent2,
            3 => ScriptContent3,
            4 => ScriptContent4,
            5 => ScriptContent5,
            _ => null
        };
    }

    public string? GetScriptContent()
    {
        var priority = MacroPriority > 0 ? MacroPriority : 1;
        if (priority < 1 || priority > 5)
            priority = 1;
        return GetScriptContent(priority);
    }

    public List<CombatCommand>? LoadCommands(bool validate = true)
    {
        var content = GetScriptContent();
        if (string.IsNullOrWhiteSpace(content))
            return null;
        return CombatScriptParser.ParseLineCommands(content, Name, validate);
    }
}