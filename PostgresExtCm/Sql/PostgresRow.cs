using System.Data;
using CommonDb.DbCommands;

namespace PostgresExtCm.Sql;

public class PostgresRow : NpOnRow<DataRow>
{
    public PostgresRow(DataRow dataRow) : base(dataRow)
    {
    }

    private PostgresRow(DataRow dataRow, int index, Dictionary<string, INpOnTile?>? columns) : base(dataRow, index, columns)
    {
    }
    
    public static PostgresRow Create(DataRow row, int index)
    {
        if (row == null)
        {
            throw new ArgumentNullException(nameof(row));
        }
        var columns = new Dictionary<string, INpOnTile?>();
        foreach (DataColumn column in row.Table.Columns)
        {
            dynamic cellValue = row[column];
            Type columnType = column.DataType;
            Type genericTileType = typeof(NpOnTile<>);
            Type specificTileType = genericTileType.MakeGenericType(columnType);
            INpOnTile tile = (INpOnTile)Activator.CreateInstance(specificTileType, [cellValue, columnType])!;
            columns.Add(column.ColumnName, tile);
        }

        return new PostgresRow(row, index, columns);
    }
}