using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbCommands;

namespace DbFactory;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        await RunCassandraExample(); 
    }
    
    [Obsolete("Obsolete")]
    public static async Task RunCassandraExample()
    {
        var cassandraOptions = new CassandraNpOnConnectOptions()
            .SetContactAddresses<INpOnDbDriver>(["127.0.0.1"])?
            .SetConnectionString("127.0.0.1:9042")
            .SetKeyspace<CassandraDriver>("ScarLight".ToLower());
    
        var factory = new DbDriverFactory();
        INpOnDbDriver driver = factory.CreateDriver(EDb.Cassandra, cassandraOptions);
    
        // var logger = new NullLogger<DbConnection<CassandraDriver>>(); 
    
        // await using (var connection = new DbConnection<CassandraDriver>(cassandraOptions!))
        await using (var connection = new NpOnDbConnection<CassandraDriver>(driver!))
        {
            CancellationToken token = CancellationToken.None;
            await connection.Driver.ConnectAsync(token);
            // await connection.OpenAsync();
            Console.WriteLine($"Successfully connected to {connection.Database} version {connection.ServerVersion}");

            INpOnDbCommand command = new NpOnDbCommand(EDb.Cassandra, "select * from SEMAST limit 10");

            var a =  await connection.Driver.Query(command);
        }
    }
}