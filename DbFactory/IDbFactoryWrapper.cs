using CommonDb.DbResults;
using Enums;

namespace DbFactory;

public interface IDbFactoryWrapper
{
    string? FactoryOptionCode { get; }
    EDb DbType { get; }
    Task<INpOnWrapperResult?> QueryAsync(string queryString);
}