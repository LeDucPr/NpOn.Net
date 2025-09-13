using Cassandra.Connections;
using CommonDb.Connections;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cassandra;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
    // public async Task RunCassandraExample()
    // {
    //     // 1. Tạo đối tượng options
    //     var cassandraOptions = new CassandraDbConnectOptions()
    //         .SetConnectionString("127.0.0.1:9042")
    //         .SetKeyspace<CassandraDriver>("ScarLight");
    //
    //     // 2. Sử dụng Factory để tạo driver
    //     var factory = new DbDriverFactory();
    //     IDbDriver driver = factory.CreateDriver(DbLanguage.Cassandra, cassandraOptions);
    //
    //     // 3. Inject driver vào DbConnect và sử dụng
    //     // (Giả định bạn có một logger đã được cấu hình)
    //     var logger = new NullLogger<DbConnect>(); 
    //
    //     await using (var connection = new DbConnect(driver, logger))
    //     {
    //         await connection.OpenAsync();
    //         Console.WriteLine($"Successfully connected to {connection.Database} version {connection.ServerVersion}");
    //
    //         await using (var command = connection.CreateCommand())
    //         {
    //             command.CommandText = "SELECT cluster_name FROM system.local";
    //             var clusterName = await command.ExecuteScalarAsync();
    //             Console.WriteLine($"Cluster Name: {clusterName}");
    //         }
    //     }
    // }
}