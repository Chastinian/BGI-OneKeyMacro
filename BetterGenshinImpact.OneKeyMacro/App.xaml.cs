using BetterGenshinImpact.OneKeyMacro.Service;
using BetterGenshinImpact.OneKeyMacro.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Threading;

namespace BetterGenshinImpact.OneKeyMacro;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Capture ALL exceptions before WPF message pump starts
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.ToString(), "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSingleton<OneKeyMacroService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败:\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { ServiceProvider?.GetRequiredService<OneKeyMacroService>().Dispose(); } catch { }
        base.OnExit(e);
    }
}