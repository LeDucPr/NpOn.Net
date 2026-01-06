using NpgsqlTypes;
using ProtoBuf;

namespace Common.Infrastructures.NpOn.CommonDb.DbCommands;

[ProtoContract]
[ProtoInclude(1000, typeof(NpOnDbCommandParamGrpc<NpgsqlDbType>))]
public class NpOnDbCommandParamGrpc
{
    [ProtoMember(1)] public required string ParamName { get; set; }
    [ProtoMember(2)] public string? ParamValue { get; set; }
}

[ProtoContract]
public class NpOnDbCommandParamGrpc<TEnum> : NpOnDbCommandParamGrpc where TEnum : Enum
{
    [ProtoMember(3)] public required TEnum ParamType { get; set; }
}

public static class NpOnDbCommandParamGrpcExtensions
{
    public static NpOnDbCommandParam ToDbParam(this NpOnDbCommandParamGrpc gRpcParam)
    {
        // generic grpc param
        if (gRpcParam is NpOnDbCommandParamGrpc<NpgsqlDbType> typed)
        {
            return new NpOnDbCommandParam<NpgsqlDbType>
            {
                ParamName = typed.ParamName,
                ParamValue = typed.ParamValue,
                ParamType = typed.ParamType
            };
        }

        // base param
        return new NpOnDbCommandParam
        {
            ParamName = gRpcParam.ParamName,
            ParamValue = gRpcParam.ParamValue
        };
    }
}

[ProtoContract]
public class NpOnDbCommandParamGrpcList
{
    [ProtoMember(1)]
    public List<NpOnDbCommandParamGrpc> Items { get; set; } = new();
}
