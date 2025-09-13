using CommonDb.DbCommands;

namespace Cassandra.Cql;

public class CassandraResult : DbResult<RowSet>
{
    public CassandraResult() : base(EDb.Cassandra)
    {
    }
}
