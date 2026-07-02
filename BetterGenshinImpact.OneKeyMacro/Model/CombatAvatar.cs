using System;
using System.Collections.Generic;

namespace BetterGenshinImpact.OneKeyMacro.Model;

[Serializable]
public class CombatAvatar
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 角色中文名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色英文名
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// 武器类型
    /// </summary>
    public string Weapon { get; set; } = string.Empty;

    /// <summary>
    /// 别名
    /// </summary>
    public List<string> Alias { get; set; } = [];
}