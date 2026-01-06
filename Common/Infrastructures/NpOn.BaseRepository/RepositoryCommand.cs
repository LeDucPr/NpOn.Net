using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;

namespace Common.Infrastructures.NpOn.BaseRepository;

public class RepositoryCommand
{
    public required string CommandText { get; set; }
    public required EExecType ExecType { get; set; }
    public NpOnDbCommandParam[]? Parameters { get; set; }
}