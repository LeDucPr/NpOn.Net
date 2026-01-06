using Common.Extensions.NpOn.CommonEnums;
using ProtoBuf;

namespace MicroServices.General.Contract.GeneralServiceContract.Commands;

[ProtoContract]
public class DomainActionCommand : BaseGeneralCommand
{
    [ProtoMember(1)] public required ERepositoryAction ActionType { get; set; }

    [ProtoMember(2)] public required Type DomainType { get; set; }
    [ProtoMember(3)] public byte[]? Payload { get; set; }
}