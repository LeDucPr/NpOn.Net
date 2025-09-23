using CommonDb.DbCommands;
using Npgsql;
using System.Data; // <-- Thêm namespace cho DataTable
using System.Threading.Tasks;

namespace PostgresExtCm.Sql;

public class PostgresResult : NpOnDbResult<DataTable, DataRow>
{
    public List<Dictionary<string, object?>>? ExtractedData { get; private set; }
    public int RecordsAffected { get; }

    public PostgresResult(DataTable? data, int recordsAffected = 0) : base(EDb.Postgres)
    {
        ExtractedData = new List<Dictionary<string, object?>>();
        RecordsAffected = recordsAffected;
    }

    public PostgresResult() : base(EDb.Postgres) // null 
    {
    }

    public override DataRow[]? GetRows()
    {
        base.GetRows();
        if (Result == null || Result.Rows.Count == 0)
            return null;
        return Result.Rows.Cast<DataRow>().ToArray();
    }

    public static async Task<PostgresResult> CreateAsync(NpgsqlDataReader reader)
    {
        var dataTable = new DataTable();
        await using (reader) dataTable.Load(reader);
        return new PostgresResult(dataTable, reader.RecordsAffected);
    }
}