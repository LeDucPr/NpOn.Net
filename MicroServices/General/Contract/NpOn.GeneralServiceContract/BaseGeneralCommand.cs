using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.General.Contract.GeneralServiceContract.Commands;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(TblFldExecution))]
[ProtoInclude(200, typeof(DomainActionCommand))]
public abstract class BaseGeneralCommand : CommonAbsQuery
{
    [ProtoMember(1)] public override bool Status { get; set; }
    [ProtoMember(2)] public override EErrorCode? ErrorCode { get; set; }
    [ProtoMember(3)] public override string? Object { get; set; }
    [ProtoMember(4)] public sealed override DateTime QueryUtcTime { get; init; } = DateTime.UtcNow;
}