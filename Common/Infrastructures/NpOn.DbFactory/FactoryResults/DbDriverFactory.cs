using System.Collections.Concurrent;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CassandraExtCm.Connections;
using Common.Infrastructures.NpOn.CommonDb.Connections;
using Common.Infrastructures.NpOn.MongoDbExtCm.Connections;
using Common.Infrastructures.NpOn.MssqlExtCm.Connections;
using Common.Infrastructures.NpOn.PostgresExtCm.Connections;
using Common.Infrastructures.NpOn.RedisExtCm.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Common.Infrastructures.NpOn.DbFactory.FactoryResults;

public interface IDbDriverFactory
{
    #region properties

    /// <summary>
    /// Connection khả dụng
    /// </summary>
    public int GetAliveConnectionNumbers { get; }

    public int GetConnectionNumbers { get; }
    public List<NpOnDbConnection>? ValidConnections { get; }
    public NpOnDbConnection? FirstValidConnection { get; }
    public string DriverOptionKey { get; }
    
    /// <summary> Lấy một kết nối khả dụng từ pool. </summary>
    Task<NpOnDbConnection?> GetConnectionAsync();
    /// <summary> Trả kết nối về lại pool. </summary>
    void ReleaseConnection(NpOnDbConnection connection);

    #endregion properties


    #region Create Connections

    IDbDriverFactory WithDatabaseType(EDb eDb);
    IDbDriverFactory WithOption(INpOnConnectOption option);
    IDbDriverFactory CreateConnections(int connectionNumber = 1);
    Task<IDbDriverFactory> Reset(bool isResetParameters = false);

    Task<int> OpenConnections(int connectionNumber = 1, bool isAutoFixConnectionNumber = true,
        bool isUseException = false);

    #endregion Create Connections
}

public class DbDriverFactory : IDbDriverFactory
{
    #region private parameters

    private EDb? _eDb;
    private INpOnConnectOption? _option;
    private int? _connectionNumber;
    
    // ConcurrentDictionary để theo dõi trạng thái kết nối (true = using, false = relaxing)
    private readonly ConcurrentDictionary<NpOnDbConnection, bool> _connectionStates = new();
    private SemaphoreSlim? _poolSemaphore;

    #endregion private parameters


    #region implement properties

    private readonly ILogger<DbDriverFactory> _logger = new Logger<DbDriverFactory>(new NullLoggerFactory());
    private List<NpOnDbConnection>? _connections;
    public int GetAliveConnectionNumbers => _connections?.Count(c => c.Driver.IsValidSession) ?? 0;
    public int GetConnectionNumbers => _connectionStates.Count;

    public List<NpOnDbConnection>? ValidConnections =>
        _connections?.Where(c => c.Driver.IsValidSession).ToList();

    private List<NpOnDbConnection>? InvalidConnections =>
        _connections?.Where(c => !c.Driver.IsValidSession).ToList();
    
    public NpOnDbConnection? FirstValidConnection => _connections?.FirstOrDefault(c => c.Driver.IsValidSession);
    public string DriverOptionKey => _option?.Code ?? throw new Exception(EDbError.Connection.GetDisplayName());

    #endregion implement properties


    #region Create Connections

    public DbDriverFactory(EDb eDb, INpOnConnectOption option, int connectionNumber = 1)
    {
        if (!option.IsConnectValid())
            throw new ArgumentException("Config Option for Database is Invalid.", nameof(option));
        _eDb = eDb;
        _option = option;
        _connectionNumber = connectionNumber;
        SelfCreateConnections(EConnectLink.SelfValidateConnection.GetDisplayName());
    }

    public IDbDriverFactory WithDatabaseType(EDb eDb)
    {
        _eDb = eDb;
        SelfCreateConnections(EConnectLink.SelfValidateConnection.GetDisplayName());
        return this;
    }

    public IDbDriverFactory WithOption(INpOnConnectOption option)
    {
        _option = option;
        SelfCreateConnections(EConnectLink.SelfValidateConnection.GetDisplayName());
        return this;
    }

    public IDbDriverFactory CreateConnections(int connectionNumber = 1)
    {
        _connectionNumber = connectionNumber;
        SelfCreateConnections(EConnectLink.SelfValidateConnection.GetDisplayName());
        return this;
    }

    public async Task<IDbDriverFactory> Reset(bool isResetParameters = false)
    {
        if (isResetParameters)
        {
            _eDb = null;
            _option = null;
            _connectionNumber = null;
        }

        if (_connections == null) return this;
        foreach (var connection in _connections) await connection.CloseAsync();
        return this;
    }

    public async Task<int> OpenConnections(int connectionNumber = 1, bool isAutoFixConnectionNumber = true,
        bool isUseException = false)
    {
        try
        {
            if (_connections == null)
                throw new Exception("connection not initialized");

            if (connectionNumber <= _connectionNumber || isAutoFixConnectionNumber)
                connectionNumber = (int)_connectionNumber!;
            else
                throw new Exception("The number of connections attempted to be initiated has exceeded the limit");

            List<NpOnDbConnection>? invalidConnections = InvalidConnections;
            if (invalidConnections is not { Count : > 0 })
            {
                throw new Exception(
                    $"no longer available connection. Full connection ({connectionNumber}/{_connectionNumber})");
            }

            if (GetAliveConnectionNumbers == 0 && invalidConnections.Count > 0) // open 1 (performance with many)
                await invalidConnections.First().OpenAsync();

            // foreach (var invalidConnection in invalidConnections)
            //     await invalidConnection.OpenAsync();

            if (ValidConnections is not { Count : > 0 } && isUseException)
            {
                throw new Exception("Cannot open any Connections");
            }

            return ValidConnections?.Count ?? 0;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            if (isUseException)
                throw new Exception("Cannot open any Connections");
            return 0;
        }
    }

    public async Task<NpOnDbConnection?> GetConnectionAsync()
    {
        if (_poolSemaphore == null) return null;
        
        // 1. Wait until a slot is available.
        // If the pool is full (count = 0), this line will suspend (await) until another thread calls ReleaseConnection.
        await _poolSemaphore.WaitAsync();
        _logger.LogInformation("Remaining available connections: {Count}", _poolSemaphore.CurrentCount);
        Console.WriteLine($"Remaining available connections: {_poolSemaphore.CurrentCount}");

        try
        {
            // Use a lock object (or _connectionStates itself) to ensure exclusive access when retrieving a connection.
            // Since the Semaphore has granted access, there must be at least one available connection (from a logical counting perspective).
            lock (_connectionStates)
            {
                // Find an available and valid connection
                var availablePair = _connectionStates.FirstOrDefault(pair => !pair.Value && pair.Key.Driver.IsValidSession);

                if (availablePair.Key != null)
                {
                    _connectionStates[availablePair.Key] = true; // using connection 
                    return availablePair.Key;
                }

                // If no valid free connection is found, try to find a free but invalid connection to reopen it.
                var invalidConnection = _connectionStates.FirstOrDefault(pair => !pair.Value && !pair.Key.Driver.IsValidSession).Key;
                if (invalidConnection != null)
                {
                    // Mark as in-use first to reserve its spot within the lock.
                    _connectionStates[invalidConnection] = true;
                    
                    // return if need faster 
                    // OpenAsync can be time-consuming, so it should be handled carefully.
                    // We will open it directly here for simplicity (note that the lock will block other threads from searching).
                    try 
                    {
                         // safe with status of Dictionary.
                         // OpenAsync within a lock might slightly slow down other threads' searches, but it's safe for the Dictionary's state.
                         invalidConnection.OpenAsync().GetAwaiter().GetResult(); 
                         return invalidConnection;
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Failed to open connection.");
                        _connectionStates[invalidConnection] = false; // Trả lại trạng thái rảnh nếu lỗi
                        // Revert to free state if an error occurs.
                        throw; // Throw the error for the catch block below to release the Semaphore.
                    }
                }
            }
            
            // Nếu tất cả kết nối đều đang bận hoặc không thể mở, trả về null
            _logger.LogWarning("No available connections in the pool.");
            return null;
        }
        catch
        {
            _poolSemaphore.Release(); // return slot 
            throw;
        }
    }

    public void ReleaseConnection(NpOnDbConnection? connection)
    {
        if (connection == null) return;

        if (_connectionStates.ContainsKey(connection))
        {
            bool wasInUse = _connectionStates[connection];
            _connectionStates[connection] = false; // Mark as relaxing
            
            // If the connection was actually in use and returned, we return a slot to the Semaphore.
            // This will wake up a thread waiting in WaitAsync() above.
            if (wasInUse && _poolSemaphore != null)
            {
                try 
                {
                    _poolSemaphore.Release();
                    _logger.LogInformation("Remaining available connections: {Count}", _poolSemaphore.CurrentCount);
                    Console.WriteLine($"Remaining available connections: {_poolSemaphore.CurrentCount}");
                }
                catch (SemaphoreFullException) 
                { 
                    // Ignore if releasing more than the capacity (usually due to incorrect logic, but caught for safety)
                }
            }
        }
        else
        {
            _logger.LogWarning("Attempted to release a connection that does not belong to this pool.");
        }
    }


    private IDbDriverFactory SelfCreateConnections(string? eValidateString)
    {
        try
        {
            if (_eDb == null)
            {
                throw new InvalidOperationException(
                    "Database type has not been set. Call WithDatabaseType() before creating connections.");
            }

            if (_option == null)
            {
                throw new InvalidOperationException(
                    "Connection options have not been set or are invalid. Call WithOptions() with valid options before creating connections.");
            }

            bool validateOption = eValidateString == null
                ? !_option.IsValid()
                : !_option.IsValidRequireFromBase(eValidateString);
            if (validateOption)
            {
                throw new InvalidOperationException(
                    "Connection options have not been set or are invalid. Call WithOptions() with valid options before creating connections.");
            }

            if (_connectionNumber == null)
            {
                throw new InvalidOperationException(
                    "Connection number have not been set or are invalid. Call CreateConnections() before creating connections.");
            }

            if (typeof(INpOnConnectOption) == _option.GetType())
            {
                throw new TypeInitializationException(typeof(INpOnConnectOption).ToString(),
                    new Exception("Need to configure driver correctly"));
            }

            _connections = new List<NpOnDbConnection>();
            _connectionStates.Clear();
            
            // Initialize the Semaphore with the number of permits equal to the maximum number of connections
            _poolSemaphore?.Dispose();
            _poolSemaphore = new SemaphoreSlim((int)_connectionNumber, (int)_connectionNumber);

            for (int i = 0; i < _connectionNumber; i++)
            {
                // logger.LogInformation("Creating a database driver for {DatabaseType}", eDb);
                NpOnDbConnection? newConnection = _eDb switch
                {
                    EDb.Cassandra => CreateCassandraConnection(_option),
                    EDb.ScyllaDb => CreateCassandraConnection(_option),
                    EDb.Postgres => CreatePostgresConnection(_option),
                    EDb.MongoDb => CreateMongoDbConnection(_option),
                    EDb.Mssql => CreateMssqlDbConnection(_option),
                    EDb.Redis => CreateRedisDbConnection(_option),
                    _ => throw new NotSupportedException($"The database type '{_eDb}' is not supported.")
                };
                if (newConnection == null)
                {
                    throw new NotSupportedException($"The database type '{_eDb}' is not supported.");
                }

                var connection = newConnection;
                _connections?.Add(connection);
                _connectionStates.TryAdd(connection, false); // Add to the pool with a relaxing state
                // logger.LogInformation("Successfully created a {DriverType}", driver.GetType().Name);
            }
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception.Message);
        }
        catch (NotImplementedException exception)
        {
            _logger.LogError(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception.Message);
        }

        return this;
    }

    #endregion Create Connections


    #region Cassandra

    private NpOnDbConnection CreateCassandraConnection(INpOnConnectOption option)
    {
        if (option is not CassandraConnectOption cassandraOptions)
        {
            throw new ArgumentException("Invalid options for Cassandra. Expected CassandraConnectOptions.",
                nameof(option));
        }

        INpOnDbDriver driver = CreateCassandraDriver(cassandraOptions);
        return new NpOnDbConnection<CassandraDriver>(driver);
    }

    private INpOnDbDriver CreateCassandraDriver(INpOnConnectOption option)
    {
        if (option is not CassandraConnectOption cassandraOptions)
        {
            throw new ArgumentException("Invalid options provided for CassandraCM. Expected CassandraConnectOptions.",
                nameof(option));
        }

        return new CassandraDriver(cassandraOptions);
    }

    #endregion Cassandra


    #region Postgres

    private NpOnDbConnection CreatePostgresConnection(INpOnConnectOption option)
    {
        if (option is not PostgresConnectOption postgresOptions)
        {
            throw new ArgumentException("Invalid options for Postgres. Expected PostgresConnectOptions.",
                nameof(option));
        }

        INpOnDbDriver driver = CreatePostgresDriver(postgresOptions);
        return new NpOnDbConnection<PostgresDriver>(driver);
    }

    private INpOnDbDriver CreatePostgresDriver(INpOnConnectOption option)
    {
        if (option is not PostgresConnectOption postgresOptions)
        {
            throw new ArgumentException("Invalid options provided for PostgresSQL. Expected PostgresConnectOptions.",
                nameof(option));
        }

        return new PostgresDriver(postgresOptions);
    }

    #endregion Postgres


    #region MongoDb

    private NpOnDbConnection CreateMongoDbConnection(INpOnConnectOption option)
    {
        if (option is not MongoDbConnectOption mongoOptions)
        {
            throw new ArgumentException("Invalid options for MongoDB. Expected MongoDbConnectOptions.",
                nameof(option));
        }

        INpOnDbDriver driver = CreateMongoDbDriver(mongoOptions);
        return new NpOnDbConnection<MongoDbDriver>(driver);
    }

    private INpOnDbDriver CreateMongoDbDriver(INpOnConnectOption option)
    {
        if (option is not MongoDbConnectOption mongoOptions)
        {
            throw new ArgumentException("Invalid options provided for MongoDB. Expected MongoDbConnectOptions.",
                nameof(option));
        }

        return new MongoDbDriver(mongoOptions);
    }

    #endregion MongoDb


    #region Mssql

    private NpOnDbConnection CreateMssqlDbConnection(INpOnConnectOption option)
    {
        if (option is not MssqlConnectOption mssqlOptions)
        {
            throw new ArgumentException("Invalid options for Mssql. Expected MssqlConnectOptions.",
                nameof(option));
        }

        INpOnDbDriver driver = CreateMssqlDriver(mssqlOptions);
        return new NpOnDbConnection<MssqlDriver>(driver);
    }

    private INpOnDbDriver CreateMssqlDriver(INpOnConnectOption option)
    {
        if (option is not MssqlConnectOption mssqlOptions)
        {
            throw new ArgumentException("Invalid options provided for Mssql. Expected MssqlConnectOptions.",
                nameof(option));
        }

        return new MssqlDriver(mssqlOptions);
    }

    #endregion Mssql


    #region Redis

    private NpOnDbConnection CreateRedisDbConnection(INpOnConnectOption option)
    {
        if (option is not RedisConnectOption redisOptions)
        {
            throw new ArgumentException("Invalid options for Redis. Expected RedisConnectOptions.",
                nameof(option));
        }

        INpOnDbDriver driver = CreateRedisDriver(redisOptions);
        return new NpOnDbConnection<RedisDriver>(driver);
    }

    private INpOnDbDriver CreateRedisDriver(INpOnConnectOption option)
    {
        if (option is not RedisConnectOption redisOptions)
        {
            throw new ArgumentException("Invalid options provided for Redis. Expected RedisConnectOptions.",
                nameof(option));
        }

        return new RedisDriver(redisOptions);
    }

    #endregion Redis
}