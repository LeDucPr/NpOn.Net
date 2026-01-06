using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.BaseRepository;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract.ReadModels;

[ProtoContract]
public class CommandRModel : BaseGeneralRModel
{
    [ProtoMember(1)] public required string CommandText { get; set; }
    [ProtoMember(2)] public byte[]? ParamsPayload { get; set; }
    [ProtoMember(3)] public required EExecType ExecType { get; set; }
    [ProtoMember(4)] public required EDb DatabaseType { get; set; }
    [ProtoMember(5)] public required Type DeserializeParamType { get; set; }

    public NpOnDbCommandParam[]? Parameters =>
        ((NpOnDbCommandParamGrpcList?)ProtoBufMode.ProtoBufDeserialize(ParamsPayload, typeof(NpOnDbCommandParamGrpcList)))
        ?.Items.Select(x => x.ToDbParam()).ToArray();

    public RepositoryCommand ToCommand()
    {
        return new RepositoryCommand
        {
            CommandText = CommandText,
            ExecType = ExecType, 
            Parameters = Parameters
        };
    }
}