using ObjectHandlerFlow.AlgObjs.Attributes;
using ProjectBaseDomain.Attributes;
using ProjectEnums.AccountEnums;

namespace ProjectBaseDomain;

[TableLoader("acc_srv_account")]
public class TestDomain : BaseDomain
{
    [Field("id")] public Guid Id { get; set; }
    [Field("username")] public required string UserName { get; set; }
    [Field("password")] public required string Password { get; set; }
    [Field("full_name")] public string? FullName { get; set; }
    [Field("phone_number")] public string? PhoneNumber { get; set; }
    [Field("created_at")] public DateTime CreatedAt { get; set; }
    [Field("updated_at")] public DateTime UpdatedAt { get; set; }
    [Field("avatar_url")] public string? AvatarUrl { get; set; }
    [Field("permission")] public EPermission Permission { get; set; }
    [Field("email")] public string? Email { get; set; }

    public override Dictionary<string, string>? FieldMap { get; protected set; }
}