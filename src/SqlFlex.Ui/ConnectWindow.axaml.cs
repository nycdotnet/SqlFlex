using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlFlex.Ui.ViewModels;

namespace SqlFlex.Ui;

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
