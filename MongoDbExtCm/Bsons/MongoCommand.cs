using CommonDb.DbCommands;

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