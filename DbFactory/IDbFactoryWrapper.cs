using CommonDb.DbResults;

namespace DbFactory;

public interface IDbFactoryWrapper
{
    Task<INpOnWrapperResult?> QueryAsync(string queryString);
}