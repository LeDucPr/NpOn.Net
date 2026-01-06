using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

[ProtoContract]
// acc_srv_account_address
public class AccountAddressRModel : BaseAccountRModelFromGrpcTable
{
    [ProtoMember(1)] public Guid Id { get; set; }
    [ProtoMember(2)] public required Guid AccountId { get; set; }
    [ProtoMember(3)] public Guid? CountryId { get; set; }
    [ProtoMember(4)] public Guid? ProvinceId { get; set; }
    [ProtoMember(5)] public Guid? DistrictId { get; set; }
    [ProtoMember(6)] public Guid? WardId { get; set; }
    [ProtoMember(7)] public string? AddressLine { get; set; }
    [ProtoMember(8)] public EAddressType AddressType { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(Id), "id");
        FieldMap.Add(nameof(AccountId), "acc_srv_account_id");
        FieldMap.Add(nameof(CountryId), "country_id");
        FieldMap.Add(nameof(ProvinceId), "province_id");
        FieldMap.Add(nameof(DistrictId), "district_id");
        FieldMap.Add(nameof(WardId), "ward_id");
        FieldMap.Add(nameof(AddressLine), "address_line");
        FieldMap.Add(nameof(AddressType), "address_type");
    }
}