using Cassandra.Connections;
using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbCommands;
using Microsoft.Extensions.Logging.Abstractions;

namespace DbFactory;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        await RunCassandraExample(); 
    }
    
    public static async Task RunCassandraExample()
    {
        var cassandraOptions = new CassandraDbConnectOptions()
            .SetConnectionString("127.0.0.1:9042")
            .SetKeyspace<CassandraDriver>("ScarLight");
    
        var factory = new DbDriverFactory();
        IDbDriver driver = factory.CreateDriver(EDb.Cassandra, cassandraOptions);
    
        // var logger = new NullLogger<DbConnection<CassandraDriver>>(); 
    
        // await using (var connection = new DbConnection<CassandraDriver>(cassandraOptions!))
        await using (var connection = new DbConnection<CassandraDriver>(driver!))
        {
            // await connection.OpenAsync();
            // Console.WriteLine($"Successfully connected to {connection.Database} version {connection.ServerVersion}");
    
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from SEMAST limit 10";
                var clusterName = await command.ExecuteScalarAsync();
                Console.WriteLine($"Cluster Name: {clusterName}");
            }
        }
    }
}