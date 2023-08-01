using Npgsql;

namespace SqlFlex.Core.Postgres
{
    public class PostgresReader : IDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly NpgsqlCommand _command;

        public PostgresReader(NpgsqlConnection pgConnection, string query)
        {
            _conn = pgConnection;
            _command = new NpgsqlCommand(query, _conn);
        }

        public Task<NpgsqlDataReader> ExecuteReaderAsync() => _command.ExecuteReaderAsync();

        public void Dispose()
        {
            _command?.Dispose();
            _conn?.Dispose();
        }
    }
}
