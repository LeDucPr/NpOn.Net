using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;

namespace Common.Infrastructures.NpOn.DbFactory.Generics;

public interface IDbFactoryWrapper
{
    string? FactoryOptionCode { get; }
    EDb DbType { get; }
    Task<INpOnWrapperResult?> ExecuteAsync(INpOnDbCommand dbCommand);
    Task<INpOnWrapperResult?> ExecuteAsync(string queryString);
    Task<INpOnWrapperResult?> ExecuteAsync(string queryString, List<NpOnDbCommandParam> parameters);

    Task<INpOnWrapperResult?> ExecuteFunc(string funcName, Dictionary<string, object> parameters,
        bool isUseInputJson = false,
        string? isUseOutputJsonAsName = null);

    Task<INpOnWrapperResult?> ExecuteFuncParams<TEnumDbType>(string funcName,
        List<INpOnDbCommandParam<TEnumDbType>>? parameters) where TEnumDbType : Enum;
}