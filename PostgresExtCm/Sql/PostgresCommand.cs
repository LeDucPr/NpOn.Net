using CommonDb.DbCommands;
using Enums;

namespace PostgresExtCm.Sql;

public class PostgresCommand : NpOnDbCommand
{
    private PostgresCommand(string? commandText)
        : base(EDb.Postgres, commandText)
    {
    }

    public static PostgresCommand Create(string? commandText)
    {
        return new PostgresCommand(commandText);
    }
}