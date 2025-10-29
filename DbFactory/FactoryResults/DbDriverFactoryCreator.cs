using CassandraExtCm.Connections;
using CommonDb.Connections;
using CommonMode;
using Enums;
using MongoDbExtCm.Connections;
using MssqlExtCm.Connections;
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
    private INpOnConnectOption? _connectOptions;
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
                _connectOptions = new PostgresConnectOption().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator(dbType).GetAwaiter().GetResult();
                break;
            case EDb.Mssql:
                _connectOptions = new MssqlConnectOption().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator(dbType).GetAwaiter().GetResult();
                break;
            case EDb.MongoDb:
                _connectOptions = new MongoDbConnectOption().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator(dbType).GetAwaiter().GetResult();
                break;
            case EDb.Cassandra:
                _connectOptions = new CassandraConnectOption().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator(dbType).GetAwaiter().GetResult();
                break;
            case EDb.ScyllaDb:
                _connectOptions = new CassandraConnectOption().SetConnectionString(connectString);
                _dbDriverFactory = ConnectionCreator(dbType).GetAwaiter().GetResult();
                break;
        }
    }

    private async Task<IDbDriverFactory?> ConnectionCreator(EDb dbType)
    {
        try
        {
            DbErrors ??= [];
            IDbDriverFactory factory = new DbDriverFactory(dbType, _connectOptions!, _connectionNumber);
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