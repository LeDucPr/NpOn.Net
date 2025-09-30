using CommonDb.DbCommands;
using Enums;

namespace MongoDbExtCm.Bsons;

public class MongoCommand : NpOnDbCommand
{
    private MongoCommand(string? commandText)
        : base(EDb.MongoDb, commandText) 
    {
    }

    public static MongoCommand Create(string? commandText)
    {
        return new MongoCommand(commandText);
    }
}