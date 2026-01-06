using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;

namespace Common.Infrastructures.NpOn.MongoDbExtCm.Bsons;

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