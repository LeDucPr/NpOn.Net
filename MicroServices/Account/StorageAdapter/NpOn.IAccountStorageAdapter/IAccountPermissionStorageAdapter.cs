using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

public interface IAccountPermissionStorageAdapter
{
    Task<List<AccountPermissionExceptionRModel>?> AccountPermissionExceptionGetByAccountId(string accountId);
    Task<bool> AccountPermissionExceptionDeleteOldVersionByHostCode(string hostCode, string versionId);

    Task<List<AccountPermissionExceptionRModel>?> AccountPermissionExceptionGetByAccountIdAndControllerCodes(
        string accountId, string[] controllerCodes);

    Task<List<AccountPermissionControllerRModel>?> AccountPermissionControllerGetByCodes(string[]? codes);
}