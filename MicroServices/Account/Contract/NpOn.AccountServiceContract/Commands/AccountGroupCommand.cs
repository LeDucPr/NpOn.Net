using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountGroupAddOrChangeCommand : BaseAccountCommand
{
    [ProtoMember(1)] public Guid? GroupId { get; set; }
    [ProtoMember(2)] public Guid Leader { get; set; }
    [ProtoMember(3)] public Guid[]? Members { get; set; }
    [ProtoMember(4)] public string? GroupName { get; set; }
    [ProtoMember(5)] public required EAccountGroupType[]? GroupTypes { get; set; }
}

[ProtoContract]
public class AccountGroupCopyCommand : BaseAccountCommand
{
    [ProtoMember(1)] public Guid GroupIdNeedCopy { get; set; }
    [ProtoMember(2)] public required AccountGroupCopyComponentCommand[] Components { get; set; }
}

[ProtoContract]
public class AccountGroupCopyComponentCommand 
{
    [ProtoMember(1)] public Guid Leader { get; set; }
    [ProtoMember(2)] public string? GroupName { get; set; }
    [ProtoMember(3)] public Guid[]? MemberExcludes { get; set; }
    [ProtoMember(4)] public Guid[]? MemberAdds { get; set; }
    [ProtoMember(5)] public required EAccountGroupType[] GroupTypes { get; set; }
}

[ProtoContract]
public class AccountGroupDeleteCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid GroupId { get; set; }
    [ProtoMember(2)] public Guid Leader { get; set; }
    [ProtoMember(3)] public Guid[]? Members { get; set; }
    [ProtoMember(4)] public string? GroupName { get; set; }
}