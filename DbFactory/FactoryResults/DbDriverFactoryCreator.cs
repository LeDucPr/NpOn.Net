using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonMode;
using Enums;
using MongoDbExtCm.Connections;
using PostgresExtCm.Connections;

namespace DbFactory.FactoryResults;

/// <summary>
/// khởi tạo cùng Starter
/// </summary>
public class DbDriverFactoryCreator
{
    public EDb DbType { get; set; }
    private List<EDbError>? DbErrors { get; set; }
    public List<string>? DbErrorStrings => DbErrors?.Select(e => e.GetDisplayName()).ToList();

    // private 
    private INpOnConnectOptions? _connectOptions;
    private IDbDriverFactory? _dbDriverFactory;
    public IDbDriverFactory? GetDbDriverFactory => _dbDriverFactory;
    private readonly int _connectionNumber = 1;

    private void SetDefaultFieldWithoutValue()
    {
        DbErrors ??= null;
        _connectOptions ??= null;
        _dbDriverFactory ??= null;
    }

    public DbDriverFactoryCreator(EDb dbType, string connectString, int connectionNumber = 1)
    {
        if (!dbType.IsValid())
        {
            SetDefaultFieldWithoutValue();
            return;
        }

        _connectionNumber = connectionNumber;
        switch (dbType)
        {
            case EDb.Postgres:
                _connectOptions = new PostgresConnectOptions().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator().GetAwaiter().GetResult();
                break;
            case EDb.MongoDb:
                _connectOptions = new MongoDbConnectOptions().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator().GetAwaiter().GetResult();
                break;
            case EDb.Cassandra:
                _connectOptions = new CassandraConnectOptions().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator().GetAwaiter().GetResult();
                break;
            case EDb.ScyllaDb:
                _connectOptions = new CassandraConnectOptions().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator().GetAwaiter().GetResult();
                break;
        }
    }

    private async Task<IDbDriverFactory?> ConnectionCreator()
    {
        try
        {
            DbErrors ??= [];
            IDbDriverFactory factory = new DbDriverFactory(EDb.Postgres, _connectOptions!, _connectionNumber);
            if (factory.GetConnectionNumbers != 0)
            {
                if (factory.FirstValidConnection == null)
                    _ = await factory.OpenConnections();
                if (factory.FirstValidConnection == null)
                {
                    DbErrors.Add(EDbError.CreateConnection);
                    return null;
                }
            }
            return factory;
        }
        catch
        {
            return null;
        }
    }
}