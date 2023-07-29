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
            QueryText = "-- enter a query here.";
            ResultText = "/* Results will be displayed here */";
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

        [ObservableProperty]
        private string _queryText;

        [ObservableProperty]
        private string _resultText;

        [ObservableProperty]
        private ConnectViewModel _connectionViewModel;

        [ObservableProperty]
        private string _connectionHeadline;

        [ObservableProperty]
        private bool _connectionReady;

        public const string NotConnected = "Not Connected";
    }
}
