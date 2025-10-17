using CommonDb.DbResults;

namespace DbFactory;

public interface IDbFactoryWrapper
{
    string? FactoryOptionCode { get; }
    Task<INpOnWrapperResult?> QueryAsync(string queryString);
}