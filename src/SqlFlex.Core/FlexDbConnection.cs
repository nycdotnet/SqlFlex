using Npgsql;
using SqlFlex.Core.Postgres;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;

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

        public static readonly Dictionary<Type, FlexSchemaInfo> SchemaDictionaries = new();

        public IDbConnection? Connection { get; private set; }

        public static async Task<FlexSchemaInfo> GetOrAddPlatformSchemaInfo<T>(T connection, DbDataReader reader)
        {
            if (!SchemaDictionaries.TryGetValue(connection!.GetType(), out var schemaDictionary))
            {
                var schema = await reader.GetSchemaTableAsync();

                schemaDictionary = new();

                for (var i = 0; i < schema!.Columns.Count; i++)
                {
                    var col = schema!.Columns[i];
                    if (col.ColumnName == "DataType")
                    {
                        schemaDictionary.DataTypeColumnIndex = i;
                    }
                }

                SchemaDictionaries.Add(connection.GetType(), schemaDictionary);
            }
            return schemaDictionary;
        }

        public async Task<string> ExecuteToCsvAsync(string query)
        {
            if (Connection is null)
            {
                return $"Unexpected: {nameof(Connection)} is null.";
            }

            using var sqlFlexReader = Connection switch
            {
                NpgsqlConnection pgConnection => new PostgresReader(pgConnection, query),
                _ => null
            };

            if (sqlFlexReader is null)
            {
                return $"Unsupported connection type {Connection.GetType().FullName}!!";
            }

            var reader = await sqlFlexReader.ExecuteReaderAsync();
            //var platformSchema = await GetOrAddPlatformSchemaInfo(Connection, reader);  // this may be needed for platforms other than postgres.
            if (!reader.CanGetColumnSchema())
            {
                throw new NotSupportedException("Can't get the column schema for this reader");
            }
            var schema = await reader.GetColumnSchemaAsync();
            var serializers = new Func<NpgsqlDataReader, int, ReadOnlyMemory<char>>[schema.Count];
            for (var i = 0; i < schema.Count; i++)
            {
                if (schema[i].DataType == typeof(int))
                {
                    // this method may be bullshit.  need to measure.
                    serializers[i] = (NpgsqlDataReader reader, int ordinal) => $"{reader.GetInt32(ordinal)}".AsMemory();
                }
            }

            //for (var i = 0; i < schema.Rows.Count; i++)
            //{

            //    var r = platformSchema.Rows[i];
            //    var type = r[platformSchema.DataTypeColumnIndex] as Type;

            //}

            var pipe = new Pipe();



            return "";

            //if (Connection is NpgsqlConnection pgConn)
            //{
            //    var sb = new StringBuilder();
            //    using var command = new NpgsqlCommand(query, pgConn);

            //    try
            //    {
            //        using var reader = await command.ExecuteReaderAsync();
            //        var schema = await reader.GetSchemaTableAsync();
                    
            //        var serializers = new Func<NpgsqlDataReader, int, string>[schema.Rows.Count];
            //        for (var i = 0; i < schema.Rows.Count; i++)
            //        {
            //            var r = schema.Rows[i];
            //            var type = r[12] as Type;
            //            if (type == typeof(int))
            //            {
            //                serializers[i] = (NpgsqlDataReader reader, int ordinal) => $"{reader.GetInt32(ordinal)}";
            //            }
            //            else if (type == typeof(bool))
            //            {
            //                serializers[i] = (NpgsqlDataReader reader, int ordinal) => $"{reader.GetBoolean(ordinal)}";
            //            }
            //            else if (type == typeof(string))
            //            {
            //                serializers[i] = (NpgsqlDataReader reader, int ordinal) => {
            //                    var s = reader.GetString(ordinal);
            //                    if (s.Contains(','))
            //                    {
            //                        return $"\"{s}\"";
            //                    }
            //                    return s;
            //                };
            //            }
            //            else
            //            {
            //                throw new NotSupportedException($"This data type is not supported: {type.FullName}");
            //            }
            //        }

            //        while (await reader.ReadAsync())
            //        {
            //            for(var i = 0; i < serializers.Length; i++)
            //            {
            //                sb.Append(serializers[i](reader, i));
            //                if (i + 1 < serializers.Length)
            //                {
            //                    sb.Append(',');
            //                }
            //            }
            //            sb.Append('\n');
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        sb.AppendLine($"Error running query: {ex.Message}");
            //    }
                
            //    return sb.ToString();
            //}
            //else
            //{
            //    return "Unsupported connection type!!";
            //}
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
