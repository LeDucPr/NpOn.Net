using System.Reflection;
using AccountServiceObject.BusinessObjects;
using CommonDb.DbResults.Grpc;
using ProtoBuf;

namespace AccountServiceObject;

[ProtoInclude(100, typeof(AccountObject))]
[ProtoInclude(200, typeof(AccountLoginInfoObject))]
public abstract class BaseAccountObjectFromGrpcTable : NpOnBaseGrpcObject
{
    #region Field Config

    public override Dictionary<string, string>? FieldMap { get; protected set; }

    #endregion Field Config

    protected abstract override void FieldMapper();
}
