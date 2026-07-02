using System;
using Vanara.PInvoke;

namespace BetterGenshinImpact.OneKeyMacro.Helper;

public class User32Helper
{
    public static User32.VK ToVk(string key)
    {
        key = key.ToUpper();
        if (!key.StartsWith("VK_"))
        {
            key = $"VK_{key}";
        }

        return (User32.VK)Enum.Parse(typeof(User32.VK), key);
    }
}