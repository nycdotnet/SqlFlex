using Avalonia.Controls;
using Avalonia.Interactivity;
using SqlFlex.ViewModels;

namespace SqlFlex;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async void btnConnect_Click(object sender, RoutedEventArgs args)
    {
        var connect = new ConnectWindow();
        var result = await connect.ShowDialog<ConnectViewModel>(this);
    }
}