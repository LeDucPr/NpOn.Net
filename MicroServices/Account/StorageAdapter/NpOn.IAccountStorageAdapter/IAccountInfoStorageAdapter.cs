using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

public interface IAccountInfoStorageAdapter
{
    Task<AccountInfoRModel?> AccountInfoActiveGetByAccountId(string accountId); // Guid
    Task<List<AccountInfoRModel>?> AccountInfoActiveGetByAccountIds(string[] accountIds); // Guids
    Task<List<AccountAddressRModel>?> AccountAddressesGetByIds(string[] accountIds); // Guid
    Task<List<AccountAddressRModel>?> AccountAddressesDefaultGetByAccountIds(string[] accountIds); // Guids
}