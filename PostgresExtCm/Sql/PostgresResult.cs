using CommonDb.DbCommands;
using Npgsql;
using System.Data; // <-- Thêm namespace cho DataTable
using System.Threading.Tasks;

namespace PostgresExtCm.Sql;

public class PostgresResult : NpOnDbResult<DataTable> // <-- Thay đổi kiểu generic
{
    public List<Dictionary<string, object?>> ExtractedData { get; private set; }
    private PostgresResult(DataTable? data, int recordsAffected) 
        : base(EDb.Postgres) 
    {
        ExtractedData = new List<Dictionary<string, object?>>();
    }

    public static async Task<PostgresResult> CreateAsync(NpgsqlDataReader reader)
    {
        var dataTable = new DataTable();

        await using (reader)
        {
            dataTable.Load(reader);
        }

        // reader.RecordsAffected vẫn có thể truy cập được sau khi reader đã được load và đóng.
        return new PostgresResult(dataTable, reader.RecordsAffected);
    }
}