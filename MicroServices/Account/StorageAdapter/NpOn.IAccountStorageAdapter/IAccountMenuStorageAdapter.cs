using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

public interface IAccountMenuStorageAdapter
{
    Task<AccountMenuRModel?> AccountMenuGetById(AccountMenuGetByIdQuery query);
    Task<AccountMenuRModel?> AccountMenuGetByParentId(AccountMenuGetByParentIdQuery query);
    Task<List<AccountMenuRModel>?> AccountMenuSearch(AccountMenuSearchQuery query);
    Task<List<AccountMenuRModel>?> AccountMenuGetAll();
}