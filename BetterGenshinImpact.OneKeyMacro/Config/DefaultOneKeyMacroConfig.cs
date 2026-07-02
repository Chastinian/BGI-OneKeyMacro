using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterGenshinImpact.OneKeyMacro.Model;
using Newtonsoft.Json;

namespace BetterGenshinImpact.OneKeyMacro.Config;

/// <summary>
/// 一键宏默认配置，从 combat_avatar.json 加载角色数据
/// </summary>
public class DefaultOneKeyMacroConfig
{
    public static List<string> CombatAvatarNames { get; private set; } = [];
    public static Dictionary<string, CombatAvatar> CombatAvatarMap { get; private set; } = new();
    public static Dictionary<string, string> CombatAvatarAliasToNameMap { get; private set; } = new();

    static DefaultOneKeyMacroConfig()
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "combat_avatar.json");
        if (!File.Exists(jsonPath))
        {
            // Fallback: try relative to current directory
            jsonPath = Path.Combine("Assets", "combat_avatar.json");
        }

        if (File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            var config = JsonConvert.DeserializeObject<IEnumerable<CombatAvatar>>(json)
                         ?? throw new Exception("combat_avatar.json deserialize failed");

            CombatAvatarNames = config.Select(x => x.Name).ToList();
            CombatAvatarMap = config.ToDictionary(x => x.Name);
            CombatAvatarAliasToNameMap = config
                .SelectMany(c => c.Alias.Select(a => new KeyValuePair<string, string>(a, c.Name)))
                .GroupBy(kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }
    }

    public static string AvatarAliasToStandardName(string alias)
    {
        if (CombatAvatarAliasToNameMap.TryGetValue(alias, out var name) && name != null)
            return name;

        throw new Exception($"角色名称校验失败：{alias}");
    }
}