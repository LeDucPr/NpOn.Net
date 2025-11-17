using CommonObject;
using CommonWebApplication.Controllers;
using CommonWebApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[EnableCors(Constant.CorsPolicy)]
[Produces("application/json")]
[Route("api/[controller]/[action]")]
public class BaseSsoController(
    ILogger<CommonController> logger,
    ContextService contextService) : CommonController(
    logger, contextService)
{
}