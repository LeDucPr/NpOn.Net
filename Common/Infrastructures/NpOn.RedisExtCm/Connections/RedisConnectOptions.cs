using Common.Infrastructures.NpOn.CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructures.NpOn.RedisExtCm.Connections;

/// <summary>
/// Defines connection options for Redis, inheriting from the common DbNpOnConnectOption.
/// </summary>
public class RedisConnectOption : DbNpOnConnectOption<RedisDriver>
{
    public override bool IsConnectValid()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                Logger.LogError("ConnectionString is required for {TypeName}", GetType().Name);
                throw new ArgumentNullException(nameof(ConnectionString),
                    $"{GetType().Name} requires a {nameof(ConnectionString)}.");
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
        return base.IsValid(propertyName) && !string.IsNullOrWhiteSpace(ConnectionString);
    }
}