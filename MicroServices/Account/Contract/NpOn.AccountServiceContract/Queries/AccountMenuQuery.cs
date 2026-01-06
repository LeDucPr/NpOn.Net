using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

[ProtoContract]
public class AccountMenuSearchQuery : BaseAccountCommand
{
    [ProtoMember(1)] public string? Keyword { get; set; }
    [ProtoMember(2)] public required int PageSize { get; set; }
    [ProtoMember(3)] public required int PageIndex { get; set; }
}
[ProtoContract]
public class AccountMenuGetByIdQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid Id { get; set; }
}
[ProtoContract]
public class AccountMenuGetByParentIdQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid ParentId  { get; set; }
}