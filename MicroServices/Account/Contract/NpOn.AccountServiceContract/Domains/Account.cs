using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account")]
public class Account : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid Id { get; set; }

    [ProtoMember(2)] [Field("username")] public string UserName { get; set; }
    [ProtoMember(3)] [Field("password")] public string Password { get; set; }
    [ProtoMember(4)] [Field("full_name")] public string? FullName { get; set; }

    [ProtoMember(5)]
    [Field("phone_number")]
    public string? PhoneNumber { get; set; }

    [ProtoMember(6)] [Field("avatar_url")] public string? AvatarUrl { get; set; }
    [ProtoMember(7)] [Field("permission")] public EPermission? Permission { get; set; }
    [ProtoMember(8)] [Field("email")] public string? Email { get; set; }
    [ProtoMember(9)] [Field("created_at")] public DateTime? CreatedAt { get; set; }

    [ProtoMember(10)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ProtoMember(11)] [Field("status")] public EAccountStatus Status { get; set; }

    public Account()
    {
    }

    public Account(AccountRModel accountRModel)
    {
        Id = accountRModel.Id;
        UserName = accountRModel.UserName;
        Password = accountRModel.Password;
        FullName = accountRModel.FullName;
        PhoneNumber = accountRModel.PhoneNumber;
        AvatarUrl = accountRModel.AvatarUrl;
        Permission = accountRModel.Permission;
        Email = accountRModel.Email;
        CreatedAt = accountRModel.CreatedAt;
        UpdatedAt = accountRModel.UpdatedAt;
    }

    public Account(AccountSignupCommand command)
    {
        UserName = command.UserName;
        Password = command.Password;
        FullName = command.FullName;
        PhoneNumber = command.PhoneNumber;
        AvatarUrl = command.AvatarUrl;
        Permission = EPermission.User;
        Email = command.Email;
        Status = EAccountStatus.Active;
        CreatedAt = DateTime.Now;
        // UpdatedAt = DateTime.Now;
    }

    public Account(AccountSyncFromOldSystemCommand command)
    {
        Id = command.AccountId;
        UserName = command.UserName;
        Password = command.HashPassword;
        FullName = command.FullName;
        PhoneNumber = command.PhoneNumber;
        CreatedAt = command.CreatedAt;
        AvatarUrl = command.AvatarUrl?.ToString();
        Permission = command.Permission;
        Email = command.Email;
        Status = command.AccountStatus ?? EAccountStatus.Active;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeStatus(AccountSetStatusCommand changeStatusCommand)
    {
        Status = changeStatusCommand.AccountStatus;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeNewPassword(AccountChangePasswordCommand command)
    {
        Password = command.NewPassword;
        UpdatedAt = DateTime.Now;
    }
}