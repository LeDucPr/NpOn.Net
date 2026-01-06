using Common.Infrastructures.NpOn.CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructures.NpOn.MongoDbExtCm.Connections;

public class MongoDbConnectOption : DbNpOnConnectOption<MongoDbDriver>
{
    public override bool IsConnectValid()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                Logger.LogError($"ConnectionString is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(ConnectionString)}");
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                Logger.LogError($"DatabaseName is require for {GetType()}");
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