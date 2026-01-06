using Common.Extensions.NpOn.CommonBaseDomain;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(Domains.Account))]
[ProtoInclude(200, typeof(AccountLogin))]
[ProtoInclude(300, typeof(AccountInfo))]
[ProtoInclude(400, typeof(AccountPermissionController))]
[ProtoInclude(500, typeof(AccountPermissionException))]
[ProtoInclude(600, typeof(AccountGroup))]
[ProtoInclude(700, typeof(AccountAddress))]
[ProtoInclude(800, typeof(AccountMenu))]

public abstract class BaseAccountDomain : BaseDomain
{
    public BaseAccountDomain()
    {
    }

    #region Field Config

    public override Dictionary<string, string>? FieldMap { get; protected set; }

    protected override void FieldMapper()
    {
        FieldMap ??= new();
        // FieldMap.Add(nameof(Id), "id");
        // FieldMap.Add(nameof(CreatedAt), "created_at");
    }

    #endregion Field Config
}