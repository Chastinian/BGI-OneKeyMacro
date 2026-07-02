using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Fischless.HotkeyCapture;
using Vanara.PInvoke;

namespace BetterGenshinImpact.OneKeyMacro.Helper;

/// <summary>
/// 低级键盘钩子，可检测按键的 Down 和 Up 事件。
/// </summary>
public class KeyboardLowLevelHook : IDisposable
{
    private User32.SafeHHOOK _hook;
    private readonly User32.HookProc _proc;
    private Hotkey? _hotkey;
    private bool _isKeyDown;

    public event Action? KeyDown;
    public event Action? KeyUp;

    public KeyboardLowLevelHook(string hotkeyStr)
    {
        try
        {
            _hotkey = new Hotkey(hotkeyStr);
        }
        catch
        {
            _hotkey = null;
        }

        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule != null)
        {
            _hook = User32.SetWindowsHookEx(
                User32.HookType.WH_KEYBOARD_LL,
                _proc,
                Kernel32.GetModuleHandle(curModule.ModuleName),
                0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _hotkey != null && _hotkey.Key != System.Windows.Forms.Keys.None)
        {
            // 跳过由 SendInput 注入的事件，避免自身发送的按键被误判为热键操作
            var kbStruct = Marshal.PtrToStructure<User32.KBDLLHOOKSTRUCT>(lParam);
            if ((kbStruct.flags & 0x10) != 0) // LLKHF_INJECTED
            {
                return User32.CallNextHookEx(_hook, nCode, wParam, lParam);
            }

            var key = (System.Windows.Forms.Keys)kbStruct.vkCode;

            bool ctrl = (User32.GetKeyState((int)User32.VK.VK_CONTROL) & 0x8000) != 0;
            bool shift = (User32.GetKeyState((int)User32.VK.VK_SHIFT) & 0x8000) != 0;
            bool alt = (User32.GetKeyState((int)User32.VK.VK_MENU) & 0x8000) != 0;
            bool win = (User32.GetKeyState((int)User32.VK.VK_LWIN) & 0x8000) != 0 ||
                       (User32.GetKeyState((int)User32.VK.VK_RWIN) & 0x8000) != 0;

            bool modifiersMatch = ctrl == _hotkey.Control && shift == _hotkey.Shift &&
                                  alt == _hotkey.Alt && win == _hotkey.Windows;

            if (modifiersMatch && key == _hotkey.Key)
            {
                var msg = (User32.WindowMessage)wParam;
                if (msg == User32.WindowMessage.WM_KEYDOWN || msg == User32.WindowMessage.WM_SYSKEYDOWN)
                {
                    if (!_isKeyDown)
                    {
                        _isKeyDown = true;
                        KeyDown?.Invoke();
                    }
                }
                else if (msg == User32.WindowMessage.WM_KEYUP || msg == User32.WindowMessage.WM_SYSKEYUP)
                {
                    if (_isKeyDown)
                    {
                        _isKeyDown = false;
                        KeyUp?.Invoke();
                    }
                }
            }
        }

        return User32.CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    public void UpdateHotkey(string hotkeyStr)
    {
        try
        {
            _hotkey = new Hotkey(hotkeyStr);
        }
        catch
        {
            _hotkey = null;
        }
    }

    public void Dispose()
    {
        if (_hook != null && !_hook.IsInvalid)
            _hook.Dispose();
    }
}