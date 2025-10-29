using CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace CassandraExtCm.Connections;

public class CassandraConnectOption : DbNpOnConnectOption<CassandraDriver>
{
    public override bool IsConnectValid()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Keyspace))
            {
                _logger.LogError($"Keyspace is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(Keyspace)}");
            }

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                _logger.LogError($"ConnectionString is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(ConnectionString)}");
            }

            // if (Port == null || Port == 0) // Default = 9042 (Cassandra/ScyllaDb)
            // {
            //     _logger.LogError($"Port is require for {GetType()}");
            //     throw new ArgumentNullException($"{GetType()} is require {nameof(Port)}");
            // }
        }
        catch (ArgumentNullException)
        {
            return false;
        }
        return base.IsValid();
    }

    public override bool IsValid(string? propertyName = null)
    {
        if (propertyName == null)
            return true;
        return propertyName switch
        {
            nameof(SetConnectionString) => !string.IsNullOrWhiteSpace(ConnectionString),
            nameof(SetKeyspace) => !string.IsNullOrWhiteSpace(Keyspace),
            nameof(SetDatabaseName) => !string.IsNullOrWhiteSpace(DatabaseName),
            nameof(SetCollectionName) => !string.IsNullOrWhiteSpace(CollectionName),
            _ => true
        };
    }
}