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
        ViewModel.ConnectViewModel = newConnectionInfo;
        ViewModel.DbConnection?.Dispose();

        // TODO: This logic should probably all live in the view model rather than in the form.
        ViewModel.DbConnection = new FlexDbConnection(
            ViewModel.ConnectViewModel.Provider,
            ViewModel.ConnectViewModel.Host,
            ViewModel.ConnectViewModel.Username,
            ViewModel.ConnectViewModel.Password,
            ViewModel.ConnectViewModel.Database);

        try
        {
            ViewModel.DbConnection.Open();
        }
        catch (System.Exception ex)
        {
            ViewModel.ConnectViewModel.SetConnection(ex);
            return;
        }

        ViewModel.ConnectViewModel.SetConnection(ViewModel.DbConnection);
    }
}
