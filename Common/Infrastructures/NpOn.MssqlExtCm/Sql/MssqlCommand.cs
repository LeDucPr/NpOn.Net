using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;

namespace Common.Infrastructures.NpOn.MssqlExtCm.Sql;

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