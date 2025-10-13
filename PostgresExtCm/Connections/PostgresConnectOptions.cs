using CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace PostgresExtCm.Connections;

public class PostgresConnectOptions : DbNpOnConnectOptions<PostgresDriver>
{
    public override bool IsConnectValid()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                _logger.LogError($"ConnectionString is require for {GetType()}");
                throw new ArgumentNullException($"{GetType()} is require {nameof(ConnectionString)}");
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
        if (!base.IsValid())
            return false;
        if (propertyName == null)
            return true;
        return propertyName switch
        {
            nameof(SetConnectionString) => !string.IsNullOrWhiteSpace(ConnectionString),
            // nameof(SetDatabaseName) => !string.IsNullOrWhiteSpace(DatabaseName),
            _ => false
        };
    }
}