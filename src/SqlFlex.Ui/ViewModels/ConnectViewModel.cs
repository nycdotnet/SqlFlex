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
            switch (connection?.Connection)
            {
                case null:
                    ConnectionHeadline = NotConnected;
                    break;
                case { State: ConnectionState.Open }:
                    ConnectionHeadline = $"Connected to {Host} (db: {Database})";
                    break;
                default:
                    ConnectionHeadline = $"Connection to {Host} is {connection.Connection.State}";
                    break;
            }
        }

        public const string NotConnected = "Not Connected";
    }
}
