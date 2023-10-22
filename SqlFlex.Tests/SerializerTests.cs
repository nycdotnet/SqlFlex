using SqlFlex.Core;
using System.Data;
using System.Text;
using System.Threading.Channels;

namespace SqlFlex.Tests
{
    public class SerializerTests
    {
        [Fact]
        public async Task CanOpenPostgresConnection()
        {
            using var conn = await GetPostgresConnection();
            conn.State.Should().Be(ConnectionState.Open);
        }

        [Fact]
        public async Task CanWriteToChannel()
        {
            var sb = new StringBuilder();
            using var flexDbConnection = new FlexDbConnection(GetPostgresConnectionString());
            flexDbConnection.Open();
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10) {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
            var writeTask = Task.Run(() => flexDbConnection.WriteCsvChannel("""
                SELECT 1, 'a', true UNION ALL
                SELECT 2, 'b', false UNION ALL
                SELECT 3, 'c', true UNION ALL
                SELECT 4, 'd', false UNION ALL
                SELECT 5, 'e', true;
                """, channel));
            await foreach(var item in channel.Reader.ReadAllAsync())
            {
                sb.Append(item);
            }

            sb.ToString().Should().Be("""
                1,a,True
                2,b,False
                3,c,True
                4,d,False
                5,e,True

                """.ReplaceLineEndings("\n"));
        }


        [Fact]
        public async Task CanStringifyInt()
        {
            using var conn = await GetPostgresConnection();
            using var command = new NpgsqlCommand("""select 1 as "one", 0 as "zero";""", conn);
            using var reader = await command.ExecuteReaderAsync();
            using var schema = await reader.GetSchemaTableAsync();

            var sut = new PostgresFieldSerializer(schema!);
            reader.Read().Should().BeTrue();

            var resultOne = sut.Stringify(reader, 0);
            resultOne.Should().Be("1");

            var resultZero = sut.Stringify(reader, 1);
            resultZero.Should().Be("0");
        }

        [Fact]
        public async Task CanStringifyBoolean()
        {
            using var conn = await GetPostgresConnection();
            using var command = new NpgsqlCommand("""select true as "truth", false as "lies";""", conn);
            using var reader = await command.ExecuteReaderAsync();
            using var schema = await reader.GetSchemaTableAsync();

            var sut = new PostgresFieldSerializer(schema!);
            reader.Read().Should().BeTrue();
            
            var trueResult = sut.Stringify(reader, 0);
            trueResult.Should().Be("True");

            var falseResult = sut.Stringify(reader, 1);
            falseResult.Should().Be("False");
        }

        [Fact]
        public async Task CanStringifyString()
        {
            using var conn = await GetPostgresConnection();
            using var command = new NpgsqlCommand("""select 'hello' as "greeting", 'goodbye' as "sendoff";""", conn);
            using var reader = await command.ExecuteReaderAsync();
            using var schema = await reader.GetSchemaTableAsync();

            var sut = new PostgresFieldSerializer(schema!);
            reader.Read().Should().BeTrue();

            var trueResult = sut.Stringify(reader, 0);
            trueResult.Should().Be("hello");

            var falseResult = sut.Stringify(reader, 1);
            falseResult.Should().Be("goodbye");
        }

        /// <summary>
        /// Returns an open connection to the SqlFlex testing database
        /// </summary>
        private async Task<NpgsqlConnection> GetPostgresConnection()
        {
            var conn = new NpgsqlConnection(GetPostgresConnectionString().ToString());
            await conn.OpenAsync();
            return conn;
        }

        private NpgsqlConnectionStringBuilder GetPostgresConnectionString()
        {
            return new NpgsqlConnectionStringBuilder()
            {
                Host = "localhost",
                Port = 5432,
                Database = "sql-flex",
                Password = "SqlFlexInsecureDevPassword",
                Username = "postgres"
            };
        }
    }
}
