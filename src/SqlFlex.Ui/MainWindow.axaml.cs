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
    }

    private void ConnectCommand(object sender, RoutedEventArgs args)
    {
        Dispatcher.UIThread.Post(() => ShowConnectWindow());
    }

    private async Task ShowConnectWindow()
    {
        var connectWindow = new ConnectWindow();
        var connectionInfo = await connectWindow.ShowDialog<ConnectViewModel>(this);
        var dbConnection = new FlexDbConnection(
            connectionInfo.Provider,
            connectionInfo.Host,
            connectionInfo.Username,
            connectionInfo.Password,
            connectionInfo.Database);

        try
        {
            dbConnection.Open();
            Debug.WriteLine("Connected.");
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine("Error: " + ex.Message);
        }
    }
}
