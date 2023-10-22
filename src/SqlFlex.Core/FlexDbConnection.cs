using Npgsql;
using System.Data;
using System.Text;
using System.Threading.Channels;

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

        public FlexDbConnection(NpgsqlConnectionStringBuilder postgresConnectionString)
        {
            Connection = new NpgsqlConnection(postgresConnectionString.ToString());
        }

        public IDbConnection? Connection { get; private set; }

        public async ValueTask WriteCsvChannel(string query, Channel<string> channel)
        {
            if (Connection is not NpgsqlConnection pgConn)
            {
                throw new NotSupportedException("Only Postgres is supported yet.");
            }
            var writer = channel.Writer;
            using var command = new NpgsqlCommand(query, pgConn);
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                var serializers = new PostgresFieldSerializer(await reader.GetSchemaTableAsync());
                var buffer = new StringBuilder();

                while (await reader.ReadAsync())
                {
                    for (var i = 0; i < serializers.Count; i++)
                    {
                        buffer.Append(serializers.Stringify(reader, i));
                        
                        if (i + 1 < serializers.Count)
                        {
                            buffer.Append(',');
                        }
                    }
                    buffer.Append('\n');
                    await writer.WriteAsync(buffer.ToString());
                    buffer.Clear();
                }
            }
            catch (Exception ex)
            {
                writer.Complete(ex);
                return;
            }
            writer.Complete();
        }
        

        public async Task<string> ExecuteToCsvAsync(string query)
        {
            // TODO: This is a massive co-mingling of concerns.  Need to separate:
            // - running of the query (per provider)
            // - parsing the schema (per provider) [WIP with PostgresFieldSerializer]
            // - formatting the various data types (per provider/per format) [WIP with PostgresFieldSerializer]
            // - combining the data per row
            // - forking if the output gets too big to a "file download".
            // This is interesting: https://www.stevejgordon.co.uk/creating-a-readonlysequence-from-array-data-in-dotnet
            // Also: https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
            if (Connection is NpgsqlConnection pgConn)
            {
                var outputBuffer = new StringBuilder();
                using var command = new NpgsqlCommand(query, pgConn);

                try
                {
                    using var reader = await command.ExecuteReaderAsync();

                    var serializers = new PostgresFieldSerializer(await reader.GetSchemaTableAsync());
                    
                    while (await reader.ReadAsync())
                    {
                        for (var i = 0; i < serializers.Count; i++)
                        {
                            outputBuffer.Append(serializers.Stringify(reader, i));
                            if (i + 1 < serializers.Count)
                            {
                                outputBuffer.Append(',');
                            }
                        }
                        outputBuffer.Append('\n');
                    }
                }
                catch (Exception ex)
                {
                    outputBuffer.AppendLine($"Error running query: {ex.Message}");
                }
                
                return outputBuffer.ToString();
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
