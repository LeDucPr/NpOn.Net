using ProtoBuf;

namespace AccountServiceObject.QueryObjects;

[ProtoContract]
public class AccountLoginQuery : BaseAccountQuery
{
    [ProtoMember(1)] public string? Email { get; set; }
}