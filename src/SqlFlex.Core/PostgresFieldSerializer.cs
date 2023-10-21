using Npgsql;
using System.Data;

namespace SqlFlex.Core
{
    public class PostgresFieldSerializer
    {
        public PostgresFieldSerializer(DataTable? schema)
        {
            if (schema is null)
            {
                throw new ArgumentNullException("Unable to read schema table");
            }
            _serializers = new Func<NpgsqlDataReader, string>[schema.Rows.Count];
            FillSerializers(schema);
        }

        private void FillSerializers(DataTable schema)
        {
            for (var i = 0; i < schema.Rows.Count; i++)
            {
                var r = schema.Rows[i];
                var type = r[12] as Type;
                if (type == typeof(int))
                {
                    var capture = i;
                    _serializers[i] = (NpgsqlDataReader reader) => $"{reader.GetInt32(capture)}";
                }
                else if (type == typeof(bool))
                {
                    var capture = i;
                    _serializers[i] = (NpgsqlDataReader reader) => $"{reader.GetBoolean(capture)}";
                }
                else if (type == typeof(string))
                {
                    var capture = i;
                    _serializers[i] = (NpgsqlDataReader reader) => {
                        var s = reader.GetString(capture);
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
        }

        private Func<NpgsqlDataReader, string>[] _serializers;
        public int Count => _serializers.Length;

        public string Stringify(NpgsqlDataReader reader, int ordinal) => _serializers[ordinal](reader);
    }
}
