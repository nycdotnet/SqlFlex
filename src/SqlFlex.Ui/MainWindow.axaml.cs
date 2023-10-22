using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SqlFlex.Core;
using SqlFlex.Ui.ViewModels;
using System.Threading.Channels;
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
        //TODO: switch over to Avalonia.AvaloniaEdit
        ViewModel.ResultsDocument.Text = $"Running query...";
        await Task.Yield();
        ViewModel.ResultsDocument.BeginUpdate();
        ViewModel.ResultsDocument.Text = "";
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10)
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        Dispatcher.UIThread.Post(() => {
            string query = ViewModel.QueryDocument.Text;
            Task.Run(() => WriteCsvChannel(channel, query));
        });
        int index = 0;
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            Dispatcher.UIThread.Post(() => {
                ViewModel.ResultsDocument.Insert(index, item);
                index += item.Length;
            });
        }
        ViewModel.ResultsDocument.EndUpdate();
    }

    private ValueTask WriteCsvChannel(Channel<string> channel, string query)
    {
        return ViewModel.DbConnection.WriteCsvChannel(query, channel);
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
