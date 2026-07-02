using System;
using System.Collections.Generic;
using System.Linq;
using BetterGenshinImpact.OneKeyMacro.Model;

namespace BetterGenshinImpact.OneKeyMacro.Script;

public class CombatScriptParser
{
    public static string CurrentAvatarName = "当前角色";

    public static List<CombatCommand> ParseLineCommands(string lineWithoutAvatar, string avatarName, bool validate = true)
    {
        var parts = lineWithoutAvatar.Split("|", StringSplitOptions.RemoveEmptyEntries);
        var fullCombatCommands = new List<CombatCommand>();
        foreach (var part in parts)
        {
            var combatCommands = ParseLinePart(part, avatarName);
            if (combatCommands.Count > 0 && combatCommands[0].Method == Method.Round)
            {
                // 遇到round指令，作为回合分隔符使用
                var roundCommand = combatCommands[0];
                var activatingRounds = ParseRoundCommand(roundCommand);
                combatCommands.RemoveAt(0);
                foreach (var combatCommand in combatCommands)
                {
                    combatCommand.ActivatingRound = activatingRounds;
                }
            }
            fullCombatCommands.AddRange(combatCommands);
        }
        return fullCombatCommands;
    }

    public static List<int> ParseRoundCommand(CombatCommand roundCommand)
    {
        var activatingRounds = new List<int>();
        if (roundCommand.Args == null || roundCommand.Args.Count == 0)
        {
            throw new ArgumentException("round方法必须有入参，例：round(1)、round(1,3-5)");
        }
        foreach (var arg in roundCommand.Args)
        {
            if (arg.Contains('-'))
            {
                var parts = arg.Split('-', StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                    throw new ArgumentException("round方法的入参格式错误，例：round(1-3)");
                var start = int.Parse(parts[0]);
                var end = int.Parse(parts[1]);
                if (start > end || start <= 0)
                    throw new ArgumentException("round方法的入参格式错误");
                for (int i = start; i <= end; i++)
                    activatingRounds.Add(i);
            }
            else
            {
                var round = int.Parse(arg);
                if (round <= 0)
                    throw new ArgumentException("round方法的入参格式错误");
                activatingRounds.Add(round);
            }
        }
        return activatingRounds;
    }

    public static List<CombatCommand> ParseLinePart(string lineWithoutAvatar, string avatarName)
    {
        var oneLineCombatCommands = new List<CombatCommand>();
        var commandArray = lineWithoutAvatar.Split(",", StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < commandArray.Length; i++)
        {
            var command = commandArray[i];
            if (string.IsNullOrEmpty(command))
                continue;

            if (command.Contains('(') && !command.Contains(')'))
            {
                var j = i + 1;
                while (j < commandArray.Length)
                {
                    command += "," + commandArray[j];
                    if (command.Count("(".Contains) > 1)
                        throw new Exception("战斗脚本格式错误，指令括号无法配对");

                    if (command.Contains(')'))
                    {
                        i = j;
                        break;
                    }
                    j++;
                }

                if (!(command.Contains('(') && command.Contains(')')))
                    throw new Exception("战斗脚本格式错误，指令括号不完整");
            }

            var combatCommand = new CombatCommand(avatarName, command);
            oneLineCombatCommands.Add(combatCommand);
        }

        return oneLineCombatCommands;
    }
}