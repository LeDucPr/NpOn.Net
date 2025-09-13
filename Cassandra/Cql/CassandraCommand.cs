using CommonDb.DbCommands;

namespace Cassandra.Cql;

public class CassandraCommand : DbCommand
{
    private CassandraCommand(string? commandText)
        : base(EDb.Cassandra, commandText)
    {
    }

    public static CassandraCommand Create(string? commandText)
    {
        return new CassandraCommand(commandText);
    }
}