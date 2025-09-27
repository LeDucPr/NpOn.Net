using CassandraExtCm.Connections;
using CassandraExtCm.Results;
using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using MongoDbExtCm.Bsons;
using MongoDbExtCm.Connections;
using MongoDbExtCm.Results;

namespace DbFactory;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        // await RunCassandraExample();
        // await RunMongoDbExample();
    }

    [Obsolete("Obsolete")]
    public static async Task RunMongoDbExample()
    {
        var mongoOptions =
                new MongoDbConnectOptions().SetConnectionString(
                        "mongodb://root:password@localhost:27017/?authSource=admin")
                    .SetDatabaseName("config")?
                    .SetCollectionName<MongoDbDriver>($"config{DateTime.Now:DDMMYYYY}")
            ;
        IDbDriverFactory factory = new DbDriverFactory(EDb.MongoDb, mongoOptions!);
        // Driver & connection
        var aliveConnections = factory.GetAliveConnectionNumbers;
        var listConnections = factory.GetAliveConnectionNumbers;
        await factory.OpenConnections();
        var firstConnection = factory.FirstValidConnection;
        INpOnDbDriver driver = firstConnection!.Driver;
        try
        {
            Console.WriteLine("Connecting to MongoDB...");
            CancellationToken newToken = CancellationToken.None;
            await driver.ConnectAsync(newToken);
            Console.WriteLine($"Successfully connected to {driver.Name}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to connect to MongoDB: {ex.Message}");
            Console.ResetColor();
            return;
        }

        INpOnDbCommand command = MongoCommand.Create("{ }");
        Console.WriteLine($"Executing query: {command.CommandText}\n");
        var result = await driver.Query(command);

        if (result is MongoResultSetWrapper mongoResult)
        {
            PrintMongoTable(mongoResult);
        }
    }

    private static void PrintMongoTable(MongoResultSetWrapper table, string indent = "")
    {
        if (table.Rows.Count == 0)
        {
            Console.WriteLine($"{indent}Query executed successfully but returned 0 rows.");
            return;
        }
        // 1. Lấy và in Header
        var columnNames = table.Columns.Keys.ToList();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{indent}{string.Join(" | ", columnNames.Select(h => h.PadRight(20)))}");
        Console.ResetColor();
        Console.WriteLine($"{indent}{new string('-', columnNames.Count * 23)}");
        // 2. Lặp qua các hàng để in dữ liệu
        foreach (var rowWrapper in table.Rows.Values)
        {
            var rowData = new List<string>();
            foreach (var columnName in columnNames)
            {
                var cell = rowWrapper.Result[columnName];
                string cellDisplayValue;
                // Nếu ô dữ liệu là một "bảng con", hiển thị một placeholder
                if (cell is INpOnCell<MongoResultSetWrapper> subTableCell && subTableCell.Value != null &&
                    subTableCell.Value.Rows.Count > 0)
                    cellDisplayValue = $"[SUB-TABLE: {subTableCell.Value.Rows.Count} rows]";
                else
                    cellDisplayValue = cell.ValueAsObject?.ToString() ?? "NULL";
                rowData.Add(cellDisplayValue.PadRight(20));
            }
            Console.WriteLine($"{indent}{string.Join(" | ", rowData)}");
        }

        Console.WriteLine();
        foreach (var rowWrapper in table.Rows.Values)
        {
            foreach (var columnName in columnNames)
            {
                var cell = rowWrapper.Result[columnName];
                if (cell is INpOnCell<MongoResultSetWrapper> subTableCell && subTableCell.Value != null &&
                    subTableCell.Value.Rows.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{indent}  -> Details for sub-table in column '{columnName}':");
                    Console.ResetColor();
                    // Gọi đệ quy để in bảng con với thụt lề
                    PrintMongoTable(subTableCell.Value, indent + "    ");
                    Console.WriteLine();
                }
            }
        }
    }


    /// <summary>
    /// Cassandra test
    /// </summary>
    [Obsolete("Obsolete")]
    public static async Task RunCassandraExample()
    {
        var cassandraOptions = new CassandraConnectOptions()
            .SetContactAddresses<CassandraDriver>(["127.0.0.1"])?
            .SetConnectionString("127.0.0.1:9042")
            .SetKeyspace<CassandraDriver>("ScarLight".ToLower());

        IDbDriverFactory factory = new DbDriverFactory(EDb.Cassandra, cassandraOptions!);
        // factory = (await factory.Reset(true)).WithDatabaseType(EDb.Cassandra).WithOption(cassandraOptions!).CreateConnections(3);
        var aliveConnections = factory.GetAliveConnectionNumbers;
        var listConnections = factory.GetAliveConnectionNumbers;
        await factory.OpenConnections();
        var firstConnection = factory.FirstValidConnection;
        if (firstConnection != null)
        {
            CancellationToken newToken = CancellationToken.None;
            await firstConnection?.Driver.ConnectAsync(newToken)!;
            INpOnDbCommand availableCommand = new NpOnDbCommand(EDb.Cassandra, "select * from SEMAST limit 10");
            var availableResult = await firstConnection.Driver.Query(availableCommand);
            var af = availableResult.Status;
            if (af)
            {
                CassandraResultSetWrapper cassandraResult = (CassandraResultSetWrapper)availableResult;
                // 1. Lấy và in ra Header (tên các cột)
                // Lấy danh sách tên cột theo đúng thứ tự từ collection 'Columns'
                var columnNames = cassandraResult.Columns.Keys.ToList();
                var xxxxxxxxxxxx = cassandraResult.Columns.Count;
                Console.ForegroundColor = ConsoleColor.Green; // Tô màu cho header
                Console.WriteLine(string.Join(" | ", columnNames.Select(h => h.PadRight(15))));
                Console.ResetColor();
                Console.WriteLine(new string('-', columnNames.Count * 18)); // Dòng gạch ngang

                // 2. Lặp qua tất cả các hàng và in dữ liệu
                foreach (var rowWrapper in cassandraResult.Rows.Values)
                {
                    var rowData = new List<string>();
                    // Lặp qua danh sách tên cột để đảm bảo thứ tự in ra là chính xác
                    foreach (var columnName in columnNames)
                    {
                        // Lấy ô dữ liệu (cell) từ hàng hiện tại bằng tên cột
                        var cell = rowWrapper.Result[columnName];

                        // Lấy giá trị, xử lý giá trị null và định dạng cho đẹp
                        var cellValue = cell.ValueAsObject?.ToString() ?? "NULL";
                        rowData.Add(cellValue.PadRight(15));
                    }

                    Console.WriteLine(string.Join(" | ", rowData));
                }
            }
        }

        // var logger = new NullLogger<DbConnection<CassandraDriver>>(); 

        //// initializer option 1 
        // INpOnDbDriver driver = factory.CreateDriver(EDb.Cassandra, cassandraOptions);
        // await using (var connection = new NpOnDbConnection<CassandraDriver>(driver!))

        //// initializer option 2
        await using (var connection = new NpOnDbConnection<CassandraDriver>(cassandraOptions!))
        {
            CancellationToken token = CancellationToken.None;
            await connection.Driver.ConnectAsync(token);
            // await connection.OpenAsync();
            Console.WriteLine($"Successfully connected to {connection.Database} version {connection.ServerVersion}");

            INpOnDbCommand command = new NpOnDbCommand(EDb.Cassandra, "select * from SEMAST limit 10");

            var a = await connection.Driver.Query(command);
        }
    }
}