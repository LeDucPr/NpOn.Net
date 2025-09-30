using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonMode;
using Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDbExtCm.Connections;
using PostgresExtCm.Connections;

namespace DbFactory;

public interface IDbDriverFactory
{
    #region properties

    public int GetAliveConnectionNumbers { get; }
    public List<NpOnDbConnection>? ValidConnections { get; }
    public NpOnDbConnection? FirstValidConnection { get; }

    #endregion properties


    #region Create Connections

    IDbDriverFactory WithDatabaseType(EDb eDb);
    IDbDriverFactory WithOption(INpOnConnectOptions option);
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
    private INpOnConnectOptions? _option;
    private int? _connectionNumber;

    #endregion private parameters


    #region implement properties

    private readonly ILogger<DbDriverFactory> _logger = new Logger<DbDriverFactory>(new NullLoggerFactory());
    private List<NpOnDbConnection>? _connections;
    public int GetAliveConnectionNumbers => _connections?.Count(c => c.Driver.IsValidSession) ?? 0;

    public List<NpOnDbConnection>? ValidConnections =>
        _connections?.Where(c => c.Driver.IsValidSession).ToList();

    private List<NpOnDbConnection>? InvalidConnections =>
        _connections?.Where(c => !c.Driver.IsValidSession).ToList();

    public NpOnDbConnection? FirstValidConnection => _connections?.FirstOrDefault(c => c.Driver.IsValidSession);

    #endregion implement properties


    #region Create Connections

    public DbDriverFactory(EDb eDb, INpOnConnectOptions option, int connectionNumber = 1)
    {
        if (!option.IsValidWithConnect())
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

    public IDbDriverFactory WithOption(INpOnConnectOptions option)
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

            bool validateOption = eValidateString == null ? !_option.IsValid() : !_option.IsValidRequireFromBase(eValidateString);
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

            if (typeof(INpOnConnectOptions) == _option.GetType())
            {
                throw new TypeInitializationException(typeof(INpOnConnectOptions).ToString(),
                    new Exception("Need to configure driver correctly"));
            }

            _connections = new List<NpOnDbConnection>();
            for (int i = 0; i++ < _connectionNumber; i++)
            {
                // logger.LogInformation("Creating a database driver for {DatabaseType}", eDb);
                NpOnDbConnection? newConnection = _eDb switch
                {
                    EDb.Cassandra => CreateCassandraConnection(_option),
                    EDb.Postgres => CreatePostgresConnection(_option),
                    EDb.MongoDb => CreateMongoDbConnection(_option),
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

    private NpOnDbConnection CreateCassandraConnection(INpOnConnectOptions options)
    {
        if (options is not CassandraConnectOptions cassandraOptions)
        {
            throw new ArgumentException("Invalid options for Cassandra. Expected CassandraConnectOptions.",
                nameof(options));
        }

        INpOnDbDriver driver = CreateCassandraDriver(cassandraOptions);
        return new NpOnDbConnection<CassandraDriver>(driver);
    }

    private INpOnDbDriver CreateCassandraDriver(INpOnConnectOptions options)
    {
        if (options is not CassandraConnectOptions cassandraOptions)
        {
            throw new ArgumentException("Invalid options provided for CassandraCM. Expected CassandraConnectOptions.",
                nameof(options));
        }

        return new CassandraDriver(cassandraOptions);
    }

    #endregion Cassandra


    #region Postgres

    private NpOnDbConnection CreatePostgresConnection(INpOnConnectOptions options)
    {
        if (options is not PostgresConnectOptions postgresOptions)
        {
            throw new ArgumentException("Invalid options for Postgres. Expected PostgresConnectOptions.",
                nameof(options));
        }

        INpOnDbDriver driver = CreatePostgresDriver(postgresOptions);
        return new NpOnDbConnection<PostgresDriver>(driver);
    }

    private INpOnDbDriver CreatePostgresDriver(INpOnConnectOptions options)
    {
        if (options is not PostgresConnectOptions postgresOptions)
        {
            throw new ArgumentException("Invalid options provided for PostgresSQL. Expected PostgresConnectOptions.",
                nameof(options));
        }

        return new PostgresDriver(postgresOptions);
    }

    #endregion Postgres


    #region MongoDb

    private NpOnDbConnection CreateMongoDbConnection(INpOnConnectOptions options)
    {
        if (options is not MongoDbConnectOptions mongoOptions)
        {
            throw new ArgumentException("Invalid options for MongoDB. Expected MongoDbConnectOptions.",
                nameof(options));
        }

        INpOnDbDriver driver = CreateMongoDbDriver(mongoOptions);
        return new NpOnDbConnection<MongoDbDriver>(driver);
    }

    private INpOnDbDriver CreateMongoDbDriver(INpOnConnectOptions options)
    {
        if (options is not MongoDbConnectOptions mongoOptions)
        {
            throw new ArgumentException("Invalid options provided for MongoDB. Expected MongoDbConnectOptions.",
                nameof(options));
        }

        return new MongoDbDriver(mongoOptions);
    }

    #endregion MongoDb
}