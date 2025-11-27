using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountServiceObject;
using AccountServiceObject.BusinessObjects;
using AccountServiceObject.QueryObjects;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonMode;
using CommonObject;
using CommonWebApplication.Services;
using DbFactory;
using Enums;
using GeneralServiceObject.QueryObjects;
using HandleFlow.ResultConverters;
using IAccountService;
using IGeneralService;
using Microsoft.IdentityModel.Tokens;
using ProjectEntry.AccountEntries;
using ProjectEnums.AccountEnums;

namespace AccountService.Services;

public class AuthenticationService(
    IDbFactoryWrapper dbFactoryWrapper,
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), IAuthenticationService
{
    public async Task<CommonResponse<AccountLoginInfoObject>> Login(AccountLoginQuery query)
    {
        return await CommonProcess<AccountLoginInfoObject>(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountGetByUsernameAndPassword,
                QueryParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "username",
                        StringValue = query.UserName
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "password",
                        StringValue = query.Password
                    }
                ]
            };
            var execStringResponse = await fldMasterPgService.Execute(execution);
            if (!execStringResponse.Status || execStringResponse.Data == null)
            {
                response.SetFail("Could not Find ExecString", execStringResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            List<AccountLoginInfoObject>? accountObjects = execStringResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountLoginInfoObject))?
                .Cast<AccountLoginInfoObject>()
                .ToList();

            if (accountObjects is not { Count: > 0 })
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountInfoAliasTestObject>'");
                return;
            }

            AccountLoginInfoObject accountObject = accountObjects.First();
            string userName = accountObject.UserName;
            if (userName is not { Length: > 0 })
            {
                response.SetFail("Invalid username");
                return;
            }

            accountObject.SessionId = $"SESSIONID-{accountObject.UserName}-{CommonUtilityMode.GenerateGuid()}";
            (string token, int minuteExpire) = await CreateToken(userName, accountObject, oldRefreshToken: null, expireMinutes: 0,
                loginType: query.LoginType ?? ELoginType.Default);
            
            // set 
            accountObject.Token = token;
            accountObject.MinuteExpire = minuteExpire;
            response.Data = accountObject;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountInfoAliasTestObject>> LoginJ(CommonJsonQuery query)
    {
        return await CommonProcess<AccountInfoAliasTestObject>(async (response) =>
        {
            AccountLoginQuery? accountLogin = CommonObject.JsonConverter.FromJson<AccountLoginQuery?>(query.Json);
            // catch Json convert failure
            if (accountLogin == null)
            {
                response.SetFail("CommonJsonQuery -> AccountLoginQuery convert null");
                return;
            }

            // response.Data = (await Login(accountLogin)).Data;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> RefreshToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse<INpOnGrpcObject>> Info()
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse> LogOut()
    {
        throw new NotImplementedException();
    }

    public string GenRefreshToken()
    {
        return CommonUtilityMode.GenerateGuid();
    }

    private async Task<(string Token, int MinuteExpire)> CreateToken(
        string userName,
        AccountLoginInfoObject accountLoginInfo,
        string? oldRefreshToken = null,
        int expireMinutes = 0,
        ELoginType loginType = ELoginType.Default)
    {
        if (!string.IsNullOrEmpty(oldRefreshToken))
        {
            // await authenService.RemoveLoginInfo(oldRefreshToken);
        }

        string sessionKey = accountLoginInfo.SessionId;
        int minuteExpire = expireMinutes == 0
            ? EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt()
            : expireMinutes;

        accountLoginInfo.MinuteExpire = minuteExpire;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig()
            .AsDefaultString());
        string uniqueNameKey = JwtRegisteredClaimNames.UniqueName;
        List<Claim> claims =
        [
            new(ContextService.SessionCode, sessionKey),
            new("MinuteExpire", minuteExpire.ToString()),
            new(uniqueNameKey, userName),
            new(JwtRegisteredClaimNames.Sid, accountLoginInfo.AccountId.AsDefaultString()),
            new Claim(JwtHeaderParameterNames.Typ, loginType.GetDisplayName())
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(minuteExpire),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenValue = tokenHandler.WriteToken(token);
        accountLoginInfo.Token = tokenValue;
        // await authenService.SetLoginInfo(sessionKey, accountLoginInfo, accountLoginInfo.MinuteExpire);
        return (tokenValue, minuteExpire);
    }
}