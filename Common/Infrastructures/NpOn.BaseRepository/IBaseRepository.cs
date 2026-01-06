using Common.Extensions.NpOn.CommonBaseDomain;
using Common.Infrastructures.NpOn.CommonDb.DbResults;

namespace Common.Infrastructures.NpOn.BaseRepository;

public interface IBaseRepository
{
    Task<INpOnWrapperResult?> Add<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain;
    Task<INpOnWrapperResult?> Update<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain;
    Task<INpOnWrapperResult?> Merge<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain;
    Task<INpOnWrapperResult?> Delete<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain;
    Task<INpOnWrapperResult?> Add(List<BaseDomain> domains, bool isUseDefaultWhenNull = false);
    Task<INpOnWrapperResult?> Update(List<BaseDomain> domains, bool isUseDefaultWhenNull = false);
    Task<INpOnWrapperResult?> Merge(List<BaseDomain> domains, bool isUseDefaultWhenNull = false);
    Task<INpOnWrapperResult?> Delete(List<BaseDomain> domains, bool isUseDefaultWhenNull = false);
    Task<INpOnWrapperResult?> Execute(RepositoryCommand repositoryCommand);
}