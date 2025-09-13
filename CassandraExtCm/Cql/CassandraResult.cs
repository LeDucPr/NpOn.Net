using Cassandra;
using CommonDb.DbCommands;

namespace CassandraExtCm.Cql;

public class CassandraResult : DbResult<RowSet>
{
    public CassandraResult() : base(EDb.Cassandra)
    {
    }
}
