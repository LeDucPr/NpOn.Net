using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;

namespace MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

public interface IAuthenticationStorageAdapter
{
    Task<List<AccountRModel>?> AccountGetByNumberPhoneOrEmailOrUsername(
        string phoneNumber, string email, string username);

    Task<AccountRModel?> AccountGetByUsernameAndPassword(string username, string password);
    Task<List<AccountLoginRModel>?> AccountLoginInfoGetByRefreshToken(string refreshToken);
    Task<AccountRModel?> AccountGetById(string accountId); // Guid
    Task<List<AccountRModel>?> AccountGetByIds(string[]? accountIds); //Guids
    Task<AccountLoginRModel?> AccountLoginInfoGetBySessionId(string sessionId); 
}