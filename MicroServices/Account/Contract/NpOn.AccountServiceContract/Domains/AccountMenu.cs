using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_menu")]
public class AccountMenu : BaseAccountDomain
{
    #region Properties

    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid? Id { get; set; }

    [ProtoMember(2)] [Field("type")] public MenuItemType? Type { get; set; }
    [ProtoMember(3)] [Field("title_key")] public string? TitleKey { get; set; }
    [ProtoMember(4)] [Field("icon")] public string? Icon { get; set; }
    [ProtoMember(5)] [Field("path")] public string? Path { get; set; }
    [ProtoMember(6)] [Field("parent_id")] public Guid? ParentId { get; set; }

    [ProtoMember(7)]
    [Field("display_order")]
    public int? DisplayOrder { get; set; }

    [ProtoMember(8)] [Field("disabled")] public bool? Disabled { get; set; }
    [ProtoMember(9)] [Field("scope")] public MenuScope? Scope { get; set; }
    [ProtoMember(10)] [Field("module")] public string? Module { get; set; }

    [ProtoMember(11)]
    [Field("menu_status")]
    public AccountMenuStatus? MenuStatus { get; set; }

    [ProtoMember(12)]
    [Field("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ProtoMember(13)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    #endregion

    public AccountMenu()
    {
    }

    public AccountMenu(AccountMenuRModel accountMenuRModel)
    {
        Id = accountMenuRModel.Id;
        Type = accountMenuRModel.Type;
        TitleKey = accountMenuRModel.TitleKey;
        Icon = accountMenuRModel.Icon;
        Path = accountMenuRModel.Path;
        ParentId = accountMenuRModel.ParentId;
        DisplayOrder = accountMenuRModel.DisplayOrder;
        Disabled = accountMenuRModel.Disabled;
        Scope = accountMenuRModel.Scope;
        Module = accountMenuRModel.Module;
        MenuStatus = accountMenuRModel.MenuStatus;
    }

    public AccountMenu(AccountMenuAddOrChangeCommand accountMenuAddOrChangeCommand)
    {
        Id = accountMenuAddOrChangeCommand.Id;
        Type = accountMenuAddOrChangeCommand.Type;
        TitleKey = accountMenuAddOrChangeCommand.TitleKey;
        Icon = accountMenuAddOrChangeCommand.Icon;
        Path = accountMenuAddOrChangeCommand.Path;
        ParentId = accountMenuAddOrChangeCommand.ParentId;
        DisplayOrder = accountMenuAddOrChangeCommand.DisplayOrder;
        Disabled = accountMenuAddOrChangeCommand.Disabled;
        Scope = accountMenuAddOrChangeCommand.Scope;
        Module = accountMenuAddOrChangeCommand.Module;
        MenuStatus = accountMenuAddOrChangeCommand.MenuStatus;
        CreatedAt = accountMenuAddOrChangeCommand.CreatedAt;
        UpdatedAt = accountMenuAddOrChangeCommand.UpdatedAt;
    }

    public void Delete()
    {
        MenuStatus = AccountMenuStatus.Deleted;
        UpdatedAt = DateTime.Now;
    }

    public void Change(AccountMenuAddOrChangeCommand accountMenuAddOrChangeCommand)
    {
        Type = accountMenuAddOrChangeCommand.Type;
        TitleKey = accountMenuAddOrChangeCommand.TitleKey;
        Icon = accountMenuAddOrChangeCommand.Icon;
        Path = accountMenuAddOrChangeCommand.Path;
        ParentId = accountMenuAddOrChangeCommand.ParentId;
        DisplayOrder = accountMenuAddOrChangeCommand.DisplayOrder;
        Disabled = accountMenuAddOrChangeCommand.Disabled;
        Scope = accountMenuAddOrChangeCommand.Scope;
        Module = accountMenuAddOrChangeCommand.Module;
        MenuStatus = accountMenuAddOrChangeCommand.MenuStatus;
        CreatedAt = accountMenuAddOrChangeCommand.CreatedAt;
        UpdatedAt = DateTime.Now;
    }
}