using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using BetterGenshinImpact.OneKeyMacro.Config;
using BetterGenshinImpact.OneKeyMacro.Model;
using BetterGenshinImpact.OneKeyMacro.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BetterGenshinImpact.OneKeyMacro.ViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly OneKeyMacroService _service;
    private readonly ILogger<MainViewModel> _logger;

    // 设置持久化路径
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BetterGenshinImpact.OneKeyMacro",
        "settings.json");

    [ObservableProperty]
    private ObservableCollection<string> _avatarNames = [];

    [ObservableProperty]
    private string _selectedAvatarName = string.Empty;

    [ObservableProperty]
    private string _macroScript = string.Empty;

    [ObservableProperty]
    private string _hotkeyString = "Ctrl+F1";

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isRunning = false;

    [ObservableProperty]
    private string _statusText = "就绪";

    public string[] HotkeyModes { get; } = ["Hold", "Toggle"];

    [ObservableProperty]
    private string _selectedHotkeyMode = "Hold";

    [ObservableProperty]
    private string _scriptFilePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AvatarMacro> _savedMacros = [];

    [ObservableProperty]
    private int _selectedMacroPriority = 1;

    public string[] MacroPriorities { get; } = ["1", "2", "3", "4", "5"];

    [ObservableProperty]
    private string _selectedMacroPriorityStr = "1";

    private bool _loadingSettings = true; // 加载设置时不触发保存

    public MainViewModel(OneKeyMacroService service, ILogger<MainViewModel> logger)
    {
        _service = service;
        _logger = logger;

        _service.RunningStateChanged += running =>
        {
            IsRunning = running;
            StatusText = running ? "运行中..." : "已停止";
        };

        // 加载角色列表
        AvatarNames = new ObservableCollection<string>(DefaultOneKeyMacroConfig.CombatAvatarNames);

        // 加载默认宏脚本
        LoadDefaultScript();

        // 加载持久化设置
        LoadSettings();

        // 启动时自动加载 Assets/avatar_macro.json
        AutoLoadAvatarMacro();

        // 设置快捷键（不在此处注册，由窗口 Loaded 事件触发）
        _service.HotkeyString = HotkeyString;
        _service.HotkeyMode = SelectedHotkeyMode;
        _service.IsEnabled = IsEnabled;
        _service.SelectedAvatarName = SelectedAvatarName;
        _service.MacroScript = MacroScript;

        _loadingSettings = false;
    }

    // ───────────── 持久化设置 ─────────────

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonConvert.DeserializeObject<UserSettings>(json);
                if (settings != null)
                {
                    _hotkeyString = settings.HotkeyString;
                    _selectedHotkeyMode = settings.HotkeyMode;
                    _selectedAvatarName = settings.SelectedAvatarName;
                    if (settings.MacroPriority >= 1 && settings.MacroPriority <= 5)
                    {
                        _selectedMacroPriority = settings.MacroPriority;
                        _selectedMacroPriorityStr = settings.MacroPriority.ToString();
                    }
                    OnPropertyChanged(nameof(HotkeyString));
                    OnPropertyChanged(nameof(SelectedHotkeyMode));
                    OnPropertyChanged(nameof(SelectedAvatarName));
                    OnPropertyChanged(nameof(SelectedMacroPriorityStr));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载设置失败，使用默认值");
        }
    }

    private void SaveSettings()
    {
        if (_loadingSettings) return;

        try
        {
            var settings = new UserSettings
            {
                HotkeyString = _hotkeyString,
                HotkeyMode = _selectedHotkeyMode,
                SelectedAvatarName = _selectedAvatarName,
                MacroPriority = _selectedMacroPriority
            };

            var dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null) Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "保存设置失败");
        }
    }

    // ───────────── 热键初始化 ─────────────

    public void InitializeHotkey()
    {
        _service.RegisterHotkey();
    }

    /// <summary>
    /// 启动时自动加载 Assets/avatar_macro.json
    /// </summary>
    private void AutoLoadAvatarMacro()
    {
        var macroPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "avatar_macro.json");
        try
        {
            if (File.Exists(macroPath))
            {
                var json = File.ReadAllText(macroPath);
                var macros = JsonConvert.DeserializeObject<List<AvatarMacro>>(json);
                if (macros != null && macros.Count > 0)
                {
                    SavedMacros = new ObservableCollection<AvatarMacro>(macros);
                    _logger.LogInformation("已自动加载 {Count} 个角色宏配置", macros.Count);

                    // 如果未选择角色，按持久化设置加载指定角色的宏
                    if (!string.IsNullOrWhiteSpace(SelectedAvatarName))
                    {
                        var savedMacro = SavedMacros.FirstOrDefault(m => m.Name == SelectedAvatarName);
                        if (savedMacro != null)
                        {
                            var content = savedMacro.GetScriptContent(SelectedMacroPriority)
                                          ?? savedMacro.GetScriptContent();
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                MacroScript = content;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "自动加载 avatar_macro.json 失败");
        }
    }

    private void LoadDefaultScript()
    {
        var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default_macro.txt");
        if (File.Exists(scriptPath))
        {
            MacroScript = File.ReadAllText(scriptPath);
            ScriptFilePath = scriptPath;
        }
        else
        {
            MacroScript = "attack(0.5), wait(0.3), dash";
        }
    }

    // ───────────── 属性变更回调 ─────────────

    partial void OnSelectedAvatarNameChanged(string value)
    {
        _service.SelectedAvatarName = value;
        SaveSettings();

        var savedMacro = SavedMacros.FirstOrDefault(m => m.Name == value);
        if (savedMacro != null)
        {
            var content = savedMacro.GetScriptContent();
            if (!string.IsNullOrWhiteSpace(content))
            {
                MacroScript = content;
            }
        }
    }

    partial void OnMacroScriptChanged(string value)
    {
        _service.MacroScript = value;
    }

    partial void OnHotkeyStringChanged(string value)
    {
        _service.UnregisterHotkey();
        _service.HotkeyString = value;
        _service.RegisterHotkey();
        SaveSettings();
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _service.IsEnabled = value;
    }

    partial void OnSelectedHotkeyModeChanged(string value)
    {
        _service.HotkeyMode = value;
        _service.UnregisterHotkey();
        _service.RegisterHotkey(); // 关键修复：切换模式后重新注册热键
        SaveSettings();
    }

    partial void OnSelectedMacroPriorityStrChanged(string value)
    {
        if (int.TryParse(value, out var priority))
        {
            SelectedMacroPriority = priority;
            SaveSettings();

            if (!string.IsNullOrWhiteSpace(SelectedAvatarName))
            {
                var savedMacro = SavedMacros.FirstOrDefault(m => m.Name == SelectedAvatarName);
                if (savedMacro != null)
                {
                    var content = savedMacro.GetScriptContent(priority);
                    if (!string.IsNullOrWhiteSpace(content))
                        MacroScript = content;
                }
            }
        }
    }

    // ───────────── 命令 ─────────────

    [RelayCommand]
    private void StartStop()
    {
        if (_service.IsRunning)
            _service.Stop();
        else
            _service.Start();
    }

    [RelayCommand]
    private void LoadScriptFromFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
            Title = "加载宏脚本"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                MacroScript = File.ReadAllText(dialog.FileName);
                ScriptFilePath = dialog.FileName;
                StatusText = $"已加载: {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void SaveScriptToFile()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
            Title = "保存宏脚本",
            FileName = $"{SelectedAvatarName}_macro.txt"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(dialog.FileName, MacroScript);
                ScriptFilePath = dialog.FileName;
                StatusText = $"已保存: {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                StatusText = $"保存失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void LoadMacroConfig()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "加载角色宏配置文件"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var macros = JsonConvert.DeserializeObject<List<AvatarMacro>>(json);
                if (macros != null)
                {
                    SavedMacros = new ObservableCollection<AvatarMacro>(macros);
                    StatusText = $"已加载 {macros.Count} 个角色宏配置";

                    if (!string.IsNullOrWhiteSpace(SelectedAvatarName))
                    {
                        var savedMacro = SavedMacros.FirstOrDefault(m => m.Name == SelectedAvatarName);
                        if (savedMacro != null)
                        {
                            var content = savedMacro.GetScriptContent();
                            if (!string.IsNullOrWhiteSpace(content))
                                MacroScript = content;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void SaveMacroConfig()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "保存角色宏配置",
            FileName = "avatar_macro.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var existing = SavedMacros.FirstOrDefault(m => m.Name == SelectedAvatarName);
                if (existing != null)
                {
                    SaveContentToPriority(existing);
                }
                else
                {
                    var newMacro = new AvatarMacro { Name = SelectedAvatarName };
                    SaveContentToPriority(newMacro);
                    SavedMacros.Add(newMacro);
                }

                var json = JsonConvert.SerializeObject(SavedMacros, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);
                StatusText = $"已保存 {SavedMacros.Count} 个角色宏配置";
            }
            catch (Exception ex)
            {
                StatusText = $"保存失败: {ex.Message}";
            }
        }
    }

    private void SaveContentToPriority(AvatarMacro macro)
    {
        switch (SelectedMacroPriority)
        {
            case 1: macro.ScriptContent1 = MacroScript; break;
            case 2: macro.ScriptContent2 = MacroScript; break;
            case 3: macro.ScriptContent3 = MacroScript; break;
            case 4: macro.ScriptContent4 = MacroScript; break;
            case 5: macro.ScriptContent5 = MacroScript; break;
        }
    }

    [RelayCommand]
    private void OpenHelpUrl()
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://www.bettergi.com/feats/macro/onem.html") { UseShellExecute = true });
    }
}