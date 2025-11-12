using Enums;

namespace RabbitMqBroker;


public class RabbitMqEventStorageRepository : IRabbitMqEventStorageRepository
{
    private readonly string _connectionString;
    private readonly string _dbName;
    private readonly string _tableName;

    public RabbitMqEventStorageRepository(string connectionString, string dbName, string tableName)
    {
        _connectionString = connectionString;
        _dbName = dbName;
        _tableName = tableName;
    }
}