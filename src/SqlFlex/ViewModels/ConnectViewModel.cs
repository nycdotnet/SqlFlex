using CommunityToolkit.Mvvm.ComponentModel;

namespace SqlFlex.ViewModels
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

        public ConnectViewModel()
        {
            Provider = "Npgsql";
            Host = "host";
            Username = "username";
            Password = "pw";
            Database = "db";
        }
    }
}
