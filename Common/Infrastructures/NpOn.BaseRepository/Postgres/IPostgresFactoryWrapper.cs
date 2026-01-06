using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.DbFactory.Generics;

namespace Common.Infrastructures.NpOn.BaseRepository.Postgres;

public interface IPostgresFactoryWrapper : IDbFactoryWrapper
{
    Task<INpOnWrapperResult?> Execute(RepositoryCommand repositoryCommand);
}