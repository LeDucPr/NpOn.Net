using System.Runtime.InteropServices;
using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonMode;
using Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDbExtCm.Connections;
using MssqlExtCm.Connections;
using PostgresExtCm.Connections;

namespace DbFactory.FactoryResults;

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

    #endregion private parameters


    #region implement properties

    private readonly ILogger<DbDriverFactory> _logger = new Logger<DbDriverFactory>(new NullLoggerFactory());
    private List<NpOnDbConnection>? _connections;
    public int GetAliveConnectionNumbers => _connections?.Count(c => c.Driver.IsValidSession) ?? 0;
    public int GetConnectionNumbers => _connections?.Count() ?? 0;

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

            foreach (var invalidConnection in invalidConnections)
            {
                await invalidConnection.OpenAsync();
            }

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
            for (int i = 0; i++ < _connectionNumber; i++)
            {
                // logger.LogInformation("Creating a database driver for {DatabaseType}", eDb);
                NpOnDbConnection? newConnection = _eDb switch
                {
                    EDb.Cassandra => CreateCassandraConnection(_option),
                    EDb.ScyllaDb => CreateCassandraConnection(_option),
                    EDb.Postgres => CreatePostgresConnection(_option),
                    EDb.MongoDb => CreateMongoDbConnection(_option),
                    EDb.Mssql => CreateMssqlDbConnection(_option),
                    _ => throw new NotSupportedException($"The database type '{_eDb}' is not supported.")
                };
                if (newConnection == null)
                {
                    throw new NotSupportedException($"The database type '{_eDb}' is not supported.");
                }

                var connection = newConnection;
                _connections?.Add(connection);
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
}