using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

[ProtoContract]
public class AccountPermissionExceptionGetByAccountIdQuery : BaseAccountCommand
{
    [ProtoMember(1)] public required string AccountId { get; set; }
}