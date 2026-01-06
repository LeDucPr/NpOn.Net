using Common.Extensions.NpOn.CommonInternalCache;
using Common.Extensions.NpOn.CommonWebApplication.Controllers;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.NpOn.SSO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[EnableCors(Constant.CorsPolicy)]
[Produces("application/json")]
[Route("api/[controller]/[action]")]
public class BaseSsoController(
    ILogger<BaseSsoController> logger,
    ContextService contextService) : CommonController(
    logger, contextService)
{
}