using ProtoBuf;

namespace Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;

[ProtoContract]
[ProtoInclude(100, typeof(NpOnGrpcCell))]
[ProtoInclude(200, typeof(NpOnGrpcColumn))]
[ProtoInclude(300, typeof(NpOnGrpcRow))]
[ProtoInclude(400, typeof(NpOnGrpcTable))]
public interface INpOnGrpcObject
{
}