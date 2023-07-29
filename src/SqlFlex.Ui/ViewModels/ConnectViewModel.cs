using CommunityToolkit.Mvvm.ComponentModel;
using SqlFlex.Core;
using System;
using System.Data;

namespace SqlFlex.Ui.ViewModels
{
    public partial class ConnectViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _provider;
        [ObservableProperty]
        private string _host;
        [ObservableProperty]
        private string _username;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _database;
        [ObservableProperty]
        private string _connectionHeadline;

        public ConnectViewModel()
        {
            Provider = "Npgsql";
            Host = "localhost";
            Username = "postgres";
            Password = "SqlFlexInsecureDevPassword";
            Database = "sql-flex";
            ConnectionHeadline = NotConnected;
        }

        public void SetConnection(Exception ex)
        {
            ConnectionHeadline = $"Connection error: {ex.Message}";
        }
        public void SetConnection(FlexDbConnection? connection)
        {
            ConnectionHeadline = (connection?.Connection) switch
            {
                null => NotConnected,
                { State: ConnectionState.Open } => $"Connected to {Host} (db: {Database})",
                _ => $"Connection to {Host} is {connection.Connection.State}",
            };
        }

        public const string NotConnected = "Not Connected";
    }
}
