using Enums;
using ObjectHandlerFlow.AlgObjs.Attributes;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;
using ProtoBuf;

namespace AccountServiceObject.BusinessObjects;

[ProtoContract]
[TableLoader("server_ctrl")]
public class AccountInfoAliasTestObject : BaseAccountObject
{
    [ProtoMember(1)] public string? UserName { get; set; }
    [ProtoMember(2)] public required EServer ServerType { get; set; }
    [ProtoMember(3)] public string? HashPassword { get; set; }
    [ProtoMember(4)] public int? Ccccccc { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(UserName), "server_name");
        FieldMap.Add(nameof(ServerType), "server_type");
        FieldMap.Add(nameof(HashPassword), "host");
        FieldMap.Add(nameof(Ccccccc), "port");
        base.FieldMapper();
    }
}

[ProtoContract]
[TableLoader("patient")]
public class AccountInfoAliasTestObject22222 : BaseAccountObject
{
    [ProtoMember(1)] public required string PatientId { get; set; }
    [ProtoMember(2)] public string? FullName { get; set; }
    [ProtoMember(3)] public DateTime? DateOfBirth { get; set; }
    [ProtoMember(4)] public string? Email { get; set; }
    [ProtoMember(5)] public string? LuUserId { get; set; }

    protected override void FieldMapper()
    {
        FieldMap ??= [];
        FieldMap.Add(nameof(PatientId), "patient_id");
        FieldMap.Add(nameof(FullName), "full_name");
        FieldMap.Add(nameof(DateOfBirth), "date_of_birth");
        FieldMap.Add(nameof(Email), "email");
        FieldMap.Add(nameof(LuUserId), "lu_user_id");
        base.FieldMapper();
    }
}