using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;

namespace Common.Infrastructures.NpOn.CassandraExtCm.Cql;

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