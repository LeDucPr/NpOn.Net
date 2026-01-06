using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;

[ProtoContract]
public class AccountGroupSearchQuery : BaseAccountCommand
{
    [ProtoMember(1)] public string? Keyword { get; set; }
    [ProtoMember(2)] public string? Leader { get; set; }
    [ProtoMember(3)] public int PageSize { get; set; }
    [ProtoMember(4)] public int PageIndex { get; set; }
}