using CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace MongoDbExtCm.Connections;

public class MongoDbConnectOptions : DbNpOnConnectOptions<MongoDbDriver>
{
    public override bool IsValidWithConnect()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                _logger.LogError($"ConnectionString is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(ConnectionString)}");
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                _logger.LogError($"DatabaseName is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(DatabaseName)}");
            }
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
            nameof(SetDatabaseName) => !string.IsNullOrWhiteSpace(DatabaseName),
            nameof(SetCollectionName) => !string.IsNullOrWhiteSpace(CollectionName),
            _ => true
        };
    }
}