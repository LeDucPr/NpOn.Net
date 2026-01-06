using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class AccountMenuPermissionService(
    IAccountMenuPermissionStorageAdapter accountMenuPermissionStorageAdapter,
    IPostgresBaseRepository baseRepository,
    ILogger<CommonService> logger
) : CommonService(logger), IAccountMenuPermissionService
{
    
}