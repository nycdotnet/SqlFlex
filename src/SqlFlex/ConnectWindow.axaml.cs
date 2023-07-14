using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlFlex.ViewModels;

namespace SqlFlex;

public partial class ConnectWindow : Window
{
    public ConnectWindow()
    {
        InitializeComponent();
        ViewModel = new();
        DataContext = ViewModel;
    }

    public void ConnectCommand(object sender, RoutedEventArgs args)
    {
        Close(ViewModel);
    }

    public ConnectViewModel ViewModel { get; set; }
}