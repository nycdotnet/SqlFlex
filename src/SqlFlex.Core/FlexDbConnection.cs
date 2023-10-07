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
            // TODO: This is a massive co-mingling of concerns.  Need to separate:
            // - running of the query (per provider)
            // - parsing the schema (per provider)
            // - formatting the various data types (per provider/per format)
            // - combining the data per row
            // - forking if the output gets too big to a "file download".
            // This is interesting: https://www.stevejgordon.co.uk/creating-a-readonlysequence-from-array-data-in-dotnet
            if (Connection is NpgsqlConnection pgConn)
            {
                var sb = new StringBuilder();
                using var command = new NpgsqlCommand(query, pgConn);

                try
                {
                    using var reader = await command.ExecuteReaderAsync();

                    var schema = await reader.GetSchemaTableAsync() ?? throw new ApplicationException("Unable to read schema table");
                    
                    var serializers = new Func<NpgsqlDataReader, int, string>[schema.Rows.Count];
                    for (var i = 0; i < schema.Rows.Count; i++)
                    {
                        var r = schema.Rows[i];
                        var type = r[12] as Type;
                        if (type == typeof(int))
                        {
                            serializers[i] = (NpgsqlDataReader reader, int ordinal) => $"{reader.GetInt32(ordinal)}";
                        }
                        else if (type == typeof(bool))
                        {
                            serializers[i] = (NpgsqlDataReader reader, int ordinal) => $"{reader.GetBoolean(ordinal)}";
                        }
                        else if (type == typeof(string))
                        {
                            serializers[i] = (NpgsqlDataReader reader, int ordinal) => {
                                var s = reader.GetString(ordinal);
                                if (s.Contains(','))
                                {
                                    return $"\"{s}\"";
                                }
                                return s;
                            };
                        }
                        else
                        {
                            throw new NotSupportedException($"This data type is not supported: {type?.FullName ?? "Unknown type"}");
                        }
                    }

                    while (await reader.ReadAsync())
                    {
                        for(var i = 0; i < serializers.Length; i++)
                        {
                            sb.Append(serializers[i](reader, i));
                            if (i + 1 < serializers.Length)
                            {
                                sb.Append(',');
                            }
                        }
                        sb.Append('\n');
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error running query: {ex.Message}");
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
