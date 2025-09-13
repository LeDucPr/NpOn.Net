using CommonDb.DbCommands;

namespace CassandraExtCm.Cql;

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