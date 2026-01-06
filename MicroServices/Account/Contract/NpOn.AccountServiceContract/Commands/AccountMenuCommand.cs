using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountMenuAddOrChangeCommand : BaseAccountCommand
{
    [ProtoMember(1)] public Guid? Id { get; set; }
    [ProtoMember(2)] public MenuItemType? Type { get; set; }
    [ProtoMember(3)] public required string TitleKey { get; set; }
    [ProtoMember(4)] public required string Icon { get; set; }
    [ProtoMember(5)] public string? Path { get; set; }
    [ProtoMember(6)] public Guid? ParentId { get; set; }
    [ProtoMember(7)] public int? DisplayOrder { get; set; }
    [ProtoMember(8)] public required bool Disabled { get; set; }
    [ProtoMember(9)] public MenuScope? Scope { get; set; }
    [ProtoMember(10)] public string? Module { get; set; }
    [ProtoMember(11)] public AccountMenuStatus? MenuStatus { get; set; }
    [ProtoMember(12)] public DateTime? CreatedAt { get; set; }
    [ProtoMember(13)] public DateTime? UpdatedAt { get; set; }
}

[ProtoContract]
public class AccountMenuDeleteCommand : BaseAccountCommand
{
    [ProtoMember(1)] public required Guid Id { get; set; }
}