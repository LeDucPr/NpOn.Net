using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

[ProtoContract]
public class AccountInfoGetByAccountIdQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required string AccountId { get; set; }
}

[ProtoContract]
public class AccountInfoGetByAccountIdsQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid[]? AccountIds { get; set; }
}
