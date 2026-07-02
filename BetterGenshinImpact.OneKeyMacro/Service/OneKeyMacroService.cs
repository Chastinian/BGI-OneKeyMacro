using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.OneKeyMacro.Helper;
using BetterGenshinImpact.OneKeyMacro.Model;
using BetterGenshinImpact.OneKeyMacro.Script;
using Fischless.HotkeyCapture;
using Fischless.WindowsInput;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace BetterGenshinImpact.OneKeyMacro.Service;

/// <summary>
/// 一键宏核心服务。区分两种热键模式：
///   Hold: 通过低级键盘钩子监听按键按下/松开，按下启动宏，松开停止宏
///   Toggle: 通过 HotkeyHook 监听按键，每次按下切换启动/停止
/// </summary>
public class OneKeyMacroService : IDisposable
{
    private readonly ILogger<OneKeyMacroService> _logger;

    // Hold 模式专用：低级键盘钩子
    private KeyboardLowLevelHook? _keyboardHook;

    // Toggle 模式专用：原生热键注册
    private HotkeyHook? _hotkeyHook;

    private InputSimulator _simulator = new();
    private volatile bool _isRunning = false;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    private string _selectedAvatarName = string.Empty;
    private string _macroScript = string.Empty;
    private List<CombatCommand>? _commands;

    public bool IsEnabled { get; set; } = true;

    /// <summary>"Hold" = 按住循环，"Toggle" = 按下切换</summary>
    public string HotkeyMode { get; set; } = "Hold";

    public string SelectedAvatarName
    {
        get => _selectedAvatarName;
        set { _selectedAvatarName = value; ReloadCommands(); }
    }

    public string MacroScript
    {
        get => _macroScript;
        set { _macroScript = value; ReloadCommands(); }
    }

    public string HotkeyString { get; set; } = "Ctrl+F1";
    public bool IsRunning => _isRunning;
    public event Action<bool>? RunningStateChanged;

    public bool HotkeyRegistered { get; private set; }

    public OneKeyMacroService(ILogger<OneKeyMacroService> logger)
    {
        _logger = logger;
    }

    // ───────────── 脚本加载 ─────────────

    private void ReloadCommands()
    {
        if (string.IsNullOrWhiteSpace(_macroScript) || string.IsNullOrWhiteSpace(_selectedAvatarName))
        {
            _commands = null;
            return;
        }

        try
        {
            _commands = CombatScriptParser.ParseLineCommands(_macroScript, _selectedAvatarName, validate: false);
            _logger.LogInformation("加载{Name}的宏脚本成功，共{Cnt}条指令", _selectedAvatarName, _commands.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析宏脚本失败");
            _commands = null;
        }
    }

    // ───────────── 热键注册 ─────────────

    public void RegisterHotkey()
    {
        UnregisterHotkey();

        if (HotkeyMode == "Toggle")
        {
            RegisterToggleHotkey();
        }
        else
        {
            RegisterHoldHotkey();
        }
    }

    private void RegisterHoldHotkey()
    {
        try
        {
            _keyboardHook?.Dispose();
            _keyboardHook = new KeyboardLowLevelHook(HotkeyString);
            _keyboardHook.KeyDown += OnHoldKeyDown;
            _keyboardHook.KeyUp += OnHoldKeyUp;
            HotkeyRegistered = true;
            _logger.LogInformation("Hold 热键已注册: {Hotkey}", HotkeyString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hold 热键注册失败: {Hotkey}", HotkeyString);
            HotkeyRegistered = false;
        }
    }

    private void RegisterToggleHotkey()
    {
        try
        {
            if (_hotkeyHook == null)
            {
                _hotkeyHook = new HotkeyHook();
                _hotkeyHook.KeyPressed += OnToggleHotkeyPressed;
            }

            _hotkeyHook.UnregisterHotKey();
            var hotkey = new Hotkey(HotkeyString);
            _hotkeyHook.RegisterHotKey(hotkey.ModifierKey, hotkey.Key);
            HotkeyRegistered = true;
            _logger.LogInformation("Toggle 热键已注册: {Hotkey}", HotkeyString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toggle 热键注册失败: {Hotkey}", HotkeyString);
            HotkeyRegistered = false;
        }
    }

    public void UnregisterHotkey()
    {
        try
        {
            _keyboardHook?.Dispose();
            _keyboardHook = null;
        }
        catch { }

        try
        {
            _hotkeyHook?.UnregisterHotKey();
        }
        catch { }

        HotkeyRegistered = false;
    }

    // ───────────── Hold 模式：按下 → 启动，松开 → 停止 ─────────────

    private void OnHoldKeyDown()
    {
        if (!IsEnabled) return;
        lock (_lock)
        {
            if (_commands == null || _commands.Count == 0)
            {
                _logger.LogWarning("宏脚本为空，无法启动");
                return;
            }

            if (_isRunning)
            {
                // 已经处于运行状态（可能在 Toggle 启动后切回 Hold），先停止旧任务再重新开始
                StopInternal();
            }

            StartInternal();
        }
    }

    private void OnHoldKeyUp()
    {
        lock (_lock)
        {
            StopInternal();
        }
    }

    // ───────────── Toggle 模式：按下 → 切换 ─────────────

    private void OnToggleHotkeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (!IsEnabled) return;
        lock (_lock)
        {
            if (_isRunning)
                StopInternal();
            else
            {
                if (_commands == null || _commands.Count == 0)
                {
                    _logger.LogWarning("宏脚本为空，无法启动");
                    return;
                }
                StartInternal();
            }
        }
    }

    // ───────────── 启停控制 ─────────────

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning) return;
            if (_commands == null || _commands.Count == 0) return;
            StartInternal();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            StopInternal();
        }
    }

    private void StartInternal()
    {
        if (_isRunning) return;
        if (_commands == null || _commands.Count == 0) return;

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ExecuteMacroLoop(_cts.Token));
        try { RunningStateChanged?.Invoke(true); } catch { }
        _logger.LogInformation("一键宏启动: {Avatar}", _selectedAvatarName);
    }

    private void StopInternal()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _cts?.Cancel();
        ReleaseAllKeys();
        try { RunningStateChanged?.Invoke(false); } catch { }
        _logger.LogInformation("一键宏停止: {Avatar}", _selectedAvatarName);
    }

    // ───────────── 宏执行循环 ─────────────

    private readonly object _pressedKeysLock = new();
    private readonly HashSet<string> _pressedKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _pressedMouseKeys = new(StringComparer.OrdinalIgnoreCase);

    private void ExecuteMacroLoop(CancellationToken ct)
    {
        try
        {
            var round = 1;
            while (!ct.IsCancellationRequested && _isRunning && IsEnabled)
            {
                _logger.LogInformation("→ {Name}执行宏 (第{Round}轮)", _selectedAvatarName, round);
                foreach (var command in _commands!)
                {
                    if (ct.IsCancellationRequested || !_isRunning) break;
                    if (command.ActivatingRound.Count > 0 && !command.ActivatingRound.Contains(round)) continue;
                    try { ExecuteCommand(command, ct); }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) { _logger.LogError(ex, "执行指令失败: {Cmd}", command); }
                }
                round++;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "宏循环异常"); }
        finally
        {
            ReleaseAllKeys();
            lock (_lock) { _isRunning = false; }
            try { RunningStateChanged?.Invoke(false); } catch { }
            _logger.LogInformation("→ {Name}停止宏", _selectedAvatarName);
        }
    }

    private void ExecuteCommand(CombatCommand command, CancellationToken ct)
    {
        command.Execute(this, ct);
        if (command.Method == Method.KeyDown) TrackPressedKey(command.Args![0]);
        else if (command.Method == Method.KeyUp) TrackReleasedKey(command.Args![0]);
        else if (command.Method == Method.MouseDown) TrackPressedMouseKey(command.Args is { Count: > 0 } ? command.Args[0] : "left");
        else if (command.Method == Method.MouseUp) TrackReleasedMouseKey(command.Args is { Count: > 0 } ? command.Args[0] : "left");
    }

    #region Input

    public void Attack(int ms, CancellationToken ct) { while (ms >= 0) { ct.ThrowIfCancellationRequested(); _simulator.Mouse.LeftButtonClick(); ms -= 200; Thread.Sleep(200); } }
    public void Charge(int ms, CancellationToken ct) { _simulator.Mouse.LeftButtonDown(); WaitMs(ms, ct); _simulator.Mouse.LeftButtonUp(); }
    public void Walk(string key, int ms) { var vk = key.ToLower() switch { "w" => User32.VK.VK_W, "s" => User32.VK.VK_S, "a" => User32.VK.VK_A, "d" => User32.VK.VK_D, _ => User32.VK.VK_NONAME }; if (vk == User32.VK.VK_NONAME) return; _simulator.Keyboard.KeyDown(vk); Thread.Sleep(ms); _simulator.Keyboard.KeyUp(vk); }
    public void Wait(int ms, CancellationToken ct) => WaitMs(ms, ct);
    public void Dash(int ms = 200) { _simulator.Keyboard.KeyPress(User32.VK.VK_RBUTTON); Thread.Sleep(ms); }
    public void Jump() => _simulator.Keyboard.KeyPress(User32.VK.VK_SPACE);
    public void MouseDown(string key = "left") { key = key.ToLower(); switch (key) { case "left": _simulator.Mouse.LeftButtonDown(); break; case "right": _simulator.Mouse.RightButtonDown(); break; case "middle": _simulator.Mouse.MiddleButtonDown(); break; } }
    public void MouseUp(string key = "left") { key = key.ToLower(); switch (key) { case "left": _simulator.Mouse.LeftButtonUp(); break; case "right": _simulator.Mouse.RightButtonUp(); break; case "middle": _simulator.Mouse.MiddleButtonUp(); break; } }
    public void Click(string key = "left") { key = key.ToLower(); switch (key) { case "left": _simulator.Mouse.LeftButtonClick(); break; case "right": _simulator.Mouse.RightButtonClick(); break; case "middle": _simulator.Mouse.MiddleButtonClick(); break; } }
    public void MoveBy(int x, int y) => _simulator.Mouse.MoveMouseBy(x, y);
    public void KeyDown(string key) => _simulator.Keyboard.KeyDown(User32Helper.ToVk(key));
    public void KeyUp(string key) => _simulator.Keyboard.KeyUp(User32Helper.ToVk(key));
    public void KeyPress(string key) => _simulator.Keyboard.KeyPress(User32Helper.ToVk(key));
    public void Scroll(int amount) => _simulator.Mouse.VerticalScroll(amount);

    #endregion

    #region Key Tracking

    private void TrackPressedKey(string key) { lock (_pressedKeysLock) _pressedKeys.Add(key); }
    private void TrackReleasedKey(string key) { lock (_pressedKeysLock) _pressedKeys.Remove(key); }
    private void TrackPressedMouseKey(string key) { lock (_pressedKeysLock) _pressedMouseKeys.Add(key); }
    private void TrackReleasedMouseKey(string key) { lock (_pressedKeysLock) _pressedMouseKeys.Remove(key); }
    private void ReleaseAllKeys()
    {
        string[] keys; string[] mouseKeys;
        lock (_pressedKeysLock) { keys = [.. _pressedKeys]; mouseKeys = [.. _pressedMouseKeys]; _pressedKeys.Clear(); _pressedMouseKeys.Clear(); }
        foreach (var key in keys) { try { _simulator.Keyboard.KeyUp(User32Helper.ToVk(key)); } catch { } }
        foreach (var mk in mouseKeys) { try { switch (mk.ToLower()) { case "left": _simulator.Mouse.LeftButtonUp(); break; case "right": _simulator.Mouse.RightButtonUp(); break; case "middle": _simulator.Mouse.MiddleButtonUp(); break; } } catch { } }
    }

    #endregion

    private void WaitMs(int ms, CancellationToken ct) { if (ms <= 0) return; var end = DateTime.UtcNow.AddMilliseconds(ms); while (DateTime.UtcNow < end) { ct.ThrowIfCancellationRequested(); Thread.Sleep(10); } }

    public void Dispose()
    {
        Stop();
        try { _keyboardHook?.Dispose(); } catch { }
        try { _hotkeyHook?.Dispose(); } catch { }
        _cts?.Dispose();
    }
}