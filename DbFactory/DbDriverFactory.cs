using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonDb.DbCommands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DbFactory;

public interface IDbDriverFactory
{
    INpOnDbDriver? CreateDriver(EDb eDb, INpOnConnectOptions options);
}

public class DbDriverFactory : IDbDriverFactory
{
    private ILogger<DbDriverFactory> _logger = new Logger<DbDriverFactory>(new NullLoggerFactory());
    public INpOnDbDriver? CreateDriver(EDb eDb, INpOnConnectOptions options)
    {
        try
        {
            // logger.LogInformation("Creating a database driver for {DatabaseType}", eDb);
            INpOnDbDriver? driver = eDb switch
            {
                EDb.Cassandra => CreateCassandraDriver(options),
                // EDb.Postgres => CreatePostgresDriver(options),
                _ => throw new NotSupportedException($"The database type '{eDb}' is not supported by this factory.")
            };
            // logger.LogInformation("Successfully created a {DriverType}", driver.GetType().Name);
            return driver;
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception.Message);
        }
        catch (NotImplementedException exception)
        {
            _logger.LogError(exception.Message);
        }

        return null;
    }

    private INpOnDbDriver? CreateCassandraDriver(INpOnConnectOptions options)
    {
        if (options is not CassandraNpOnConnectOptions cassandraOptions)
        {
            throw new ArgumentException("Invalid options provided for CassandraCM. Expected CassandraConnectOptions.",
                nameof(options));
        }

        return new CassandraDriver(cassandraOptions);
    }

    // private IDbDriver CreatePostgresDriver(IConnectOptions options)
    // {
    //     if (options is not PostgresConnectOptions postgresOptions)
    //     {
    //         throw new ArgumentException("Invalid options provided for PostgreSQL. Expected PostgresConnectOptions.", nameof(options));
    //     }
    //     
    //     return new PostgresDriver(postgresOptions.ConnectionString);
    // }
}