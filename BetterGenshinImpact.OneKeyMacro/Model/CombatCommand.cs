using BetterGenshinImpact.OneKeyMacro.Service;

namespace BetterGenshinImpact.OneKeyMacro.Model;

/// <summary>
/// 战斗宏指令
/// </summary>
public class CombatCommand
{
    public string Name { get; set; }

    public Method Method { get; set; }

    public List<string>? Args { get; set; }

    public List<int> ActivatingRound { get; set; } = [];

    public CombatCommand(string name, string command)
    {
        Name = name.Trim();
        command = command.Trim();
        var startIndex = command.IndexOf('(');
        if (startIndex > 0)
        {
            var endIndex = command.IndexOf(')');
            var method = command[..startIndex].Trim();
            Method = Method.GetEnumByCode(method);

            var parameters = command.Substring(startIndex + 1, endIndex - startIndex - 1);
            Args = [.. parameters.Split(',', StringSplitOptions.TrimEntries)];
        }
        else
        {
            Method = Method.GetEnumByCode(command);
            Args = [];
        }

        // 校验参数
        if (Method == Method.Walk)
        {
            if (Args.Count != 2)
                throw new ArgumentException("walk方法必须有两个入参: walk(s, 0.2)");
            var s = double.Parse(Args[1]);
            if (s <= 0)
                throw new ArgumentException("行走时间必须大于0");
        }
        else if (Method == Method.W || Method == Method.A || Method == Method.S || Method == Method.D)
        {
            if (Args.Count != 1)
                throw new ArgumentException("w/a/s/d方法必须有一个入参");
        }
        else if (Method == Method.MoveBy)
        {
            if (Args.Count != 2)
                throw new ArgumentException("moveby方法必须有两个入参: moveby(100, 100)");
        }
        else if (Method == Method.KeyDown || Method == Method.KeyUp || Method == Method.KeyPress)
        {
            if (Args.Count != 1)
                throw new ArgumentException($"{Method.Alias[0]}方法必须有一个入参");
        }
        else if (Method == Method.Scroll)
        {
            if (Args.Count != 1)
                throw new ArgumentException("scroll方法必须有一个入参: scroll(1)");
        }
    }

    public override string ToString()
    {
        return $"<CombatCommand {Name}, {Method.Alias[0]}({string.Join(",", Args ?? [])})>";
    }

    public void Execute(OneKeyMacroService service, CancellationToken ct)
    {
        if (Method == Method.Attack)
        {
            if (Args is { Count: > 0 })
            {
                var s = double.Parse(Args![0]);
                service.Attack((int)TimeSpan.FromSeconds(s).TotalMilliseconds, ct);
            }
            else
            {
                service.Attack(0, ct);
            }
        }
        else if (Method == Method.Charge)
        {
            if (Args is { Count: > 0 })
            {
                var s = double.Parse(Args![0]);
                service.Charge((int)TimeSpan.FromSeconds(s).TotalMilliseconds, ct);
            }
            else
            {
                service.Charge(1000, ct);
            }
        }
        else if (Method == Method.Walk)
        {
            var s = double.Parse(Args![1]);
            service.Walk(Args![0], (int)TimeSpan.FromSeconds(s).TotalMilliseconds);
        }
        else if (Method == Method.W)
        {
            var s = double.Parse(Args![0]);
            service.Walk("w", (int)TimeSpan.FromSeconds(s).TotalMilliseconds);
        }
        else if (Method == Method.A)
        {
            var s = double.Parse(Args![0]);
            service.Walk("a", (int)TimeSpan.FromSeconds(s).TotalMilliseconds);
        }
        else if (Method == Method.S)
        {
            var s = double.Parse(Args![0]);
            service.Walk("s", (int)TimeSpan.FromSeconds(s).TotalMilliseconds);
        }
        else if (Method == Method.D)
        {
            var s = double.Parse(Args![0]);
            service.Walk("d", (int)TimeSpan.FromSeconds(s).TotalMilliseconds);
        }
        else if (Method == Method.Wait)
        {
            var s = double.Parse(Args![0]);
            service.Wait((int)TimeSpan.FromSeconds(s).TotalMilliseconds, ct);
        }
        else if (Method == Method.Sleep)
        {
            var s = double.Parse(Args![0]);
            service.Wait((int)TimeSpan.FromSeconds(s).TotalMilliseconds, ct);
        }
        else if (Method == Method.Dash)
        {
            if (Args is { Count: > 0 })
            {
                var s = double.Parse(Args![0]);
                service.Dash((int)TimeSpan.FromSeconds(s).TotalMilliseconds);
            }
            else
            {
                service.Dash();
            }
        }
        else if (Method == Method.Jump)
        {
            service.Jump();
        }
        else if (Method == Method.MouseDown)
        {
            if (Args is { Count: > 0 })
                service.MouseDown(Args![0]);
            else
                service.MouseDown("left");
        }
        else if (Method == Method.MouseUp)
        {
            if (Args is { Count: > 0 })
                service.MouseUp(Args![0]);
            else
                service.MouseUp("left");
        }
        else if (Method == Method.Click)
        {
            if (Args is { Count: > 0 })
                service.Click(Args![0]);
            else
                service.Click("left");
        }
        else if (Method == Method.MoveBy)
        {
            if (Args is { Count: 2 })
            {
                var x = int.Parse(Args![0]);
                var y = int.Parse(Args[1]);
                service.MoveBy(x, y);
            }
        }
        else if (Method == Method.KeyDown)
        {
            service.KeyDown(Args![0]);
        }
        else if (Method == Method.KeyUp)
        {
            service.KeyUp(Args![0]);
        }
        else if (Method == Method.KeyPress)
        {
            service.KeyPress(Args![0]);
        }
        else if (Method == Method.Scroll)
        {
            service.Scroll(int.Parse(Args![0]));
        }
        else if (Method == Method.Round)
        {
            // 回合标记，不做任何操作
        }
        else if (Method == Method.Skill || Method == Method.Burst)
        {
            // skill/burst 不在此处实现，需要游戏上下文（CD检测等）
            // 如果需要支持，可以映射到 KeyPress
        }
    }
}