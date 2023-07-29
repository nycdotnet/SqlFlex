using Npgsql;
using System.Data;

namespace SqlFlex.Core
{
    public class FlexDbConnection : IDisposable
    {
        public FlexDbConnection(
            string Provider,
            string Host,
            string Username,
            string Password,
            string Database)
        {
            Connection = Provider switch {
                "Npgsql" => new NpgsqlConnection(new NpgsqlConnectionStringBuilder()
                {
                    Host = Host,
                    Username = Username,
                    Password = Password,
                    Database = Database
                }.ToString()),
                _ => throw new NotSupportedException("This provider is not supported.")
            };
        }

        public IDbConnection? Connection { get; private set; }

        public void Dispose()
        {
            if (Connection is null) return;

            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
            Connection?.Dispose();
            Connection = null;
        }

        public void Open()
        {
            Connection!.Open();
        }
    }
}
