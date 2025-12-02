using ProjectEnums.AccountEnums;

namespace CommonWebApplication.Services;

public class PermissionService
{
    public PermissionService(
        ILogger<PermissionService> logger,
        IHttpContextAccessor? httpContextAccessor,
        ContextService contextService,
        IServiceProvider serviceProvider,
        // AuthenService authenService,
        ILogAction logAction)
    {
        Init();
    }

    private void Init() 
    {
        // todo 
        // get from db
        EPermission.Administrator.AddPermission([]);
    }
 
    // public void 
}
