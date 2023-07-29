using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SqlFlex.Core;
using SqlFlex.Ui.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SqlFlex.Ui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetConnectionGridDataContext();
    }

    private void SetConnectionGridDataContext()
    {
        var grid = this.FindControl<Grid>("ConnectionGrid");
        if (grid is not null)
        {
            grid.DataContext = ConnectViewModel;
        }
    }

    public ConnectViewModel ConnectViewModel { get; set; } = new();
    public FlexDbConnection DbConnection { get; set; }

    private void ConnectCommand(object sender, RoutedEventArgs args)
    {
        Dispatcher.UIThread.Post(async () => await ShowConnectWindow());
    }

    private async Task ShowConnectWindow()
    {
        var connectWindow = new ConnectWindow();
        var newConnectionInfo = await connectWindow.ShowDialog<ConnectViewModel>(this);

        if (newConnectionInfo is null)
        {
            return;
        }
        ConnectViewModel = newConnectionInfo;
        SetConnectionGridDataContext();

        DbConnection = new FlexDbConnection(
            ConnectViewModel.Provider,
            ConnectViewModel.Host,
            ConnectViewModel.Username,
            ConnectViewModel.Password,
            ConnectViewModel.Database);

        try
        {
            DbConnection.Open();
        }
        catch (System.Exception ex)
        {
            ConnectViewModel.SetConnection(ex);
            return;
        }

        ConnectViewModel.SetConnection(DbConnection);
    }
}
