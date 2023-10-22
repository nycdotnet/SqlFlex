using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using Npgsql;
using SqlFlex.Core;
using System;
using System.ComponentModel;
using System.Data;

namespace SqlFlex.Ui.ViewModels
{
    public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
    {
        public MainWindowViewModel()
        {
            QueryDocument = new TextDocument("""
                select 'hello', true, false, gs.val, gs.val * 2, gs.val * 3, gs.val * 4, gs.val * 5, gs.val * 6, gs.val * 7, gs.val * 8, gs.val * 9
                from generate_series(0, 99999) gs(val)
                """);
            ResultsDocument = new TextDocument("/* Results will be displayed here */");
            ConnectionHeadline = NotConnected;
            ConnectionViewModel = new();
            PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        /// <summary>
        /// NOTE: When setting this, if there was previously a value, it will automatically be disposed.
        /// </summary>
        public FlexDbConnection? DbConnection {
            get => _dbConnection;
            set {
                _dbConnection?.Dispose();
                _dbConnection = value;
                OnPropertyChanged(nameof(DbConnection));
            }
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DbConnection))
            {
                if (DbConnection?.Connection is not null)
                {
                    try
                    {
                        DbConnection.Open();
                        if (DbConnection.Connection is NpgsqlConnection pgConn)
                        {
                            ConnectionHeadline = $"Connected to {pgConn.Host} (db: {pgConn.Database})";
                        }
                        else
                        {
                            ConnectionHeadline = $"Connected to unsupported database.";
                        }
                    }
                    catch (Exception ex)
                    {
                        ConnectionHeadline = $"Connection error: {ex.Message}";
                    }
                }
                ConnectionReady = DbConnection?.Connection?.State == ConnectionState.Open;
            }
        }

        public void Dispose()
        {
            _dbConnection?.Dispose();
        }

        private FlexDbConnection? _dbConnection;

        public TextDocument QueryDocument { get; init; }

        public TextDocument ResultsDocument { get; init; }

        [ObservableProperty]
        private ConnectViewModel _connectionViewModel;

        [ObservableProperty]
        private string _connectionHeadline;

        [ObservableProperty]
        private bool _connectionReady;

        public const string NotConnected = "Not Connected";
    }
}
