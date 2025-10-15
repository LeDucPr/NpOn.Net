using CommonDb.DbCommands;
using Enums;

namespace MssqlExtCm.Sql;

public class MssqlCommand : NpOnDbCommand
{
    private MssqlCommand(string? commandText)
        : base(EDb.Mssql, commandText)
    {
    }

    public static MssqlCommand Create(string? commandText)
    {
        return new MssqlCommand(commandText);
    }
}