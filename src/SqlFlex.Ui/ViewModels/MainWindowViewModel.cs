using CommunityToolkit.Mvvm.ComponentModel;
using SqlFlex.Core;
using System;

namespace SqlFlex.Ui.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IDisposable
    {
        public MainWindowViewModel()
        {
            QueryText = "-- enter a query here.";
            ResultText = "/* Results will be displayed here */";
            ConnectViewModel = new();
        }
        private FlexDbConnection? dbConnection;
        [ObservableProperty]
        private string _queryText;
        [ObservableProperty]
        private string _resultText;
        [ObservableProperty]
        private ConnectViewModel _connectViewModel;
        public FlexDbConnection? DbConnection {
            get => dbConnection;
            set {
                dbConnection?.Dispose();
                dbConnection = value;
                OnPropertyChanged(nameof(DbConnection));
            }
        }

        public void Dispose()
        {
            dbConnection?.Dispose();
        }
    }
}
