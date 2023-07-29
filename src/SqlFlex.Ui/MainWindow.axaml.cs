using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SqlFlex.Core;
using SqlFlex.Ui.ViewModels;
using System.Threading.Tasks;

namespace SqlFlex.Ui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public MainWindowViewModel ViewModel { get; set; } = new();

    private void ConnectCommand(object sender, RoutedEventArgs args)
    {
        Dispatcher.UIThread.Post(async () => await ShowConnectWindow());
    }

    private void RunQueryCommand(object sender, RoutedEventArgs args)
    {
        Dispatcher.UIThread.Post(async () => await RunQuery());
    }

    private async Task RunQuery()
    {
        ViewModel.ResultText = $"Running query:\n{ViewModel.QueryText}";
    }

    private async Task ShowConnectWindow()
    {
        var connectWindow = new ConnectWindow();
        var newConnectionInfo = await connectWindow.ShowDialog<ConnectViewModel>(this);

        if (newConnectionInfo is null)
        {
            return;
        }

        ViewModel.ConnectionViewModel = newConnectionInfo;
        ViewModel.DbConnection = new FlexDbConnection(
            ViewModel.ConnectionViewModel.Provider,
            ViewModel.ConnectionViewModel.Host,
            ViewModel.ConnectionViewModel.Username,
            ViewModel.ConnectionViewModel.Password,
            ViewModel.ConnectionViewModel.Database);
    }
}
