using Definitions.NpOn.ProjectEnums.AccountEnums;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;

[ProtoContract]
public class AccountAddressAddOrChangeCommand : BaseAccountCommand
{
    // [ProtoMember(1)] public Guid? Id { get; set; }
    [ProtoMember(2)] public required Guid AccountId { get; set; }
    [ProtoMember(3)] public Guid? CountryId { get; set; }
    [ProtoMember(4)] public Guid? ProvinceId { get; set; }
    [ProtoMember(5)] public Guid? DistrictId { get; set; }
    [ProtoMember(6)] public Guid? WardId { get; set; }
    [ProtoMember(7)] public string? AddressLine { get; set; }
    [ProtoMember(8)] public EAddressType AddressType { get; set; } = EAddressType.Other;

}