using CommonDb.DbCommands;
using Enums;

namespace CassandraExtCm.Cql;

public class CassandraCommand : NpOnDbCommand
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