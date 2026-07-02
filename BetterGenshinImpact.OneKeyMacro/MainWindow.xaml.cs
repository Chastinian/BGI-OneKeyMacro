using BetterGenshinImpact.OneKeyMacro.ViewModel;
using System.Windows;

namespace BetterGenshinImpact.OneKeyMacro;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        // Register hotkey after window is fully loaded (message pump is running)
        Loaded += (_, _) => ViewModel.InitializeHotkey();
    }
}