using Cassandra;
using CommonDb.DbCommands;

namespace CassandraExtCm.Cql;

public class CassandraResult : NpOnDbResult<RowSet>
{
    public List<Dictionary<string, object?>> ExtractedData { get; private set; }
    public CassandraResult() : base(EDb.Cassandra)
    {
        ExtractedData = new List<Dictionary<string, object?>>();
    }

    public CassandraResult(RowSet? rowSet) : base(EDb.Cassandra)
    {
        if (rowSet == null)
        {
            return;
        }
        SetResult(rowSet);
        ExtractedData = new List<Dictionary<string, object?>>();
        var columns = rowSet.Columns;
        foreach (var row in rowSet)
        {
            var extractedRow = new Dictionary<string, object?>();
            for (int i = 0; i < columns.Length; i++)
            {
                var columnInfo = columns[i];
                var value = row[i]; 
                extractedRow[columnInfo.Name] = value;
            }
            ExtractedData.Add(extractedRow);
        }
    }
}
