using CommonDb.DbCommands;
using CommonDb.DbResults;
using Enums;

namespace DbFactory;

public interface IDbFactoryWrapper
{
    string? FactoryOptionCode { get; }
    EDb DbType { get; }
    Task<INpOnWrapperResult?> QueryAsync(string queryString);
    Task<INpOnWrapperResult?> QueryAsync(string queryString, List<NpOnDbCommandParam> parameters);

    Task<INpOnWrapperResult?> ExecuteFunc(string funcName, Dictionary<string, object> parameters,
        bool isUseInputJson = false,
        string? isUseOutputJsonAsName = null);
}