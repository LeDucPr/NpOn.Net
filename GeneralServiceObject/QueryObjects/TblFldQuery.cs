using CommonDb.DbCommands;
using ProtoBuf;

namespace GeneralServiceObject.QueryObjects;

[ProtoContract]
public class TblFldQuery : BaseGeneralQuery
{
    [ProtoMember(1)] public string? TblMaterId { get; set; }
    [ProtoMember(2)] public string? Code { get; set; }
    [ProtoMember(3)] public string? ExecFunc { get; set; }
}