using CommunityToolkit.Mvvm.ComponentModel;

namespace SqlFlex.Ui.ViewModels
{
    public partial class ConnectViewModel : ObservableObject
    {
        public ConnectViewModel()
        {
            Provider = "Npgsql";
            Host = "localhost";
            Username = "postgres";
            Password = "SqlFlexInsecureDevPassword";
            Database = "sql-flex";
        }

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
    }
}
