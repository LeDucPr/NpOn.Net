using CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace PostgresExtCm.Connections;

public class PostgresConnectOptions : DbNpOnConnectOptions<PostgresDriver>
{
    public override bool IsValid()
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
}

