using Npgsql;
using System.Data;

namespace SqlFlex
{
    public class FlexDbConnection
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

        public IDbConnection Connection { get; }

        public void Open()
        {
            Connection.Open();
        }
    }
}
