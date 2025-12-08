using ObjectHandlerFlow.AlgObjs.Attributes;
using ProjectBaseDomain;
using ProjectBaseDomain.Attributes;
using ProjectEnums.AccountEnums;
using ProtoBuf;

namespace AccountServiceObject.Domains;

[TableLoader("acc_srv_account")]
public sealed class AccountDomain : BaseDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid Id { get; set; }

    [ProtoMember(2)] [Field("username")] public required string UserName { get; set; }
    [ProtoMember(3)] [Field("password")] public required string Password { get; set; }
    [ProtoMember(4)] [Field("full_name")] public string? FullName { get; set; }

    [ProtoMember(5)]
    [Field("phone_number")]
    public string? PhoneNumber { get; set; }

    [ProtoMember(6)] [Field("avatar_url")] public string? AvatarUrl { get; set; }
    [ProtoMember(7)] [Field("permission")] public EPermission Permission { get; set; }
    [ProtoMember(8)] [Field("email")] public string? Email { get; set; }
    [ProtoMember(9)] [Field("created_at")] public DateTime CreatedAt { get; set; }

    [ProtoMember(10)]
    [Field("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public override Dictionary<string, string>? FieldMap { get; protected set; }
}