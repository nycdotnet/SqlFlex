using Npgsql;
using System.Data;
using System.Text;

namespace SqlFlex.Core
{
    public sealed class FlexDbConnection : IDisposable
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

        public async Task<string> ExecuteToCsvAsync(string query)
        {
            if (Connection is Npgsql.NpgsqlConnection pgConn)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Running query......");
                using var command = new NpgsqlCommand(query, pgConn);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    sb.AppendLine(reader.GetString(0));
                }
                return sb.ToString();
            }
            else
            {
                return "Unsupported connection type!!";
            }
        }

        /// <summary>
        /// Will close the connection if it is open, dispose of it, and null it out.
        /// </summary>
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
