using CommonDb.Connections;
using Microsoft.Extensions.Logging;

namespace Cassandra.Connections;

public class CassandraDbConnectOptions : DbConnectOptions<CassandraDriver>
{
    public override bool IsValid()
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
        }
        catch (ArgumentNullException)
        {
            return false;
        }
        return base.IsValid();
    }
}