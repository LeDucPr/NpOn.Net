using Common.Extensions.NpOn.CommonBaseDomain.Attributes;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.HandleFlow.Attributes;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;

[ProtoContract]
[TableLoader("acc_srv_account_address")]
public class AccountAddress : BaseAccountDomain
{
    [ProtoMember(1)]
    [Field("id")]
    [Pk("id")]
    public Guid Id { get; set; }

    [ProtoMember(2)]
    [Field("acc_srv_account_id")]
    public Guid AccountId { get; set; }

    [ProtoMember(3)] [Field("country_id")] public Guid? CountryId { get; set; }

    [ProtoMember(4)]
    [Field("province_id")]
    public Guid? ProvinceId { get; set; }

    [ProtoMember(5)]
    [Field("district_id")]
    public Guid? DistrictId { get; set; }

    [ProtoMember(6)] [Field("ward_id")] public Guid? WardId { get; set; }

    [ProtoMember(7)]
    [Field("address_line")]
    public string? AddressLine { get; set; }

    [ProtoMember(8)]
    [Field("address_type")]
    public EAddressType AddressType { get; set; }

    [ProtoMember(9)] [Field("created_at")] public DateTime? CreatedAt { get; set; }

    [ProtoMember(10)]
    [Field("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public AccountAddress(AccountAddressAddOrChangeCommand command)
    {
        AccountId = command.AccountId.AsDefaultGuid();
        CountryId = command.CountryId;
        ProvinceId = command.ProvinceId;
        DistrictId = command.DistrictId;
        WardId = command.WardId;
        AddressLine = command.AddressLine;
        AddressType = command.AddressType;
        UpdatedAt = DateTime.Now;
        CreatedAt = DateTime.Now;
    }

    public AccountAddress(AccountAddressRModel model)
    {
        Id = model.Id;
        AccountId = model.AccountId.AsDefaultGuid();
        CountryId = model.CountryId;
        ProvinceId = model.ProvinceId;
        DistrictId = model.DistrictId;
        WardId = model.WardId;
        AddressLine = model.AddressLine;
        AddressType = model.AddressType;
        CreatedAt = model.CreatedAt;
        UpdatedAt = model.UpdatedAt;
    }

    public AccountAddress(AccountSyncFromOldSystemCommand command)
    {
        Id = IndexerMode.CreateGuid();
        Change(command);
    }

    public AccountAddress(AccountInfoAddOrChangeCommand command)
    {
        AccountId = command.AccountId.AsDefaultGuid();
        CountryId = command.CountryId;
        ProvinceId = command.ProvinceId;
        DistrictId = command.DistrictId;
        WardId = command.WardId;
        AddressLine = command.AddressLine;
        AddressType = EAddressType.Default;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;

    }

    public void Change(AccountSyncFromOldSystemCommand command)
    {
        AccountId = command.AccountId.AsDefaultGuid();
        CountryId = command.CountryId;
        ProvinceId = command.ProvinceId;
        DistrictId = command.DistrictId;
        WardId = command.WardId;
        AddressLine = command.AddressLine;
        AddressType = command.AddressType;
        CreatedAt = command.CreatedAt ?? DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeAddressType(EAddressType? addressType = null)
    {
        AddressType = addressType ?? EAddressType.Other;
        UpdatedAt = DateTime.Now;
    }
}