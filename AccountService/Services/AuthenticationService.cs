using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountServiceObject;
using AccountServiceObject.BusinessObjects;
using AccountServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonMode;
using CommonObject;
using CommonWebApplication.Services;
// using DbFactory;
using Enums;
using GeneralServiceObject.QueryObjects;
using IAccountService;
using IGeneralService;
using Microsoft.IdentityModel.Tokens;
using ProjectEntry.AccountEntries;
using ProjectEnums.AccountEnums;

namespace AccountService.Services;

public class AuthenticationService(
    // IDbFactoryWrapper dbFactoryWrapper,
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), IAuthenticationService
{
    private const string SessionIdPrefix = "SESSIONID";
    private const string MinuteExpirePrefix = "MinuteExpire";

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

            AccountObject? accountObject = execStringResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountObject))?
                .Cast<AccountObject>().FirstOrDefault();

            if (accountObject == null)
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountInfoAliasTestObject>'");
                return;
            }

            AccountLoginInfoObject accountLoginInfoObject = await CreateToken(
                accountObject, query.AuthType /*, ELoginType.Default*/);

            if (!(await SaveLogin(accountLoginInfoObject)).Status)
            {
                response.SetFail("AccountLogin save failure");
                return;
            }
            // set 
            response.Data = accountLoginInfoObject;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountLoginInfoObject>> RefreshToken(AccountRefreshTokenQuery query)
    {
        return await CommonProcess<AccountLoginInfoObject>(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountLoginInfoGetByUsernameAndPassword,
                QueryParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "refresh_token",
                        StringValue = query.RefreshToken
                    },
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

            response.Data = accountObject;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse<AccountLoginInfoObject>> Info()
    {
        throw new NotImplementedException();
    }

    public Task<CommonResponse> LogOut()
    {
        throw new NotImplementedException();
    }

    private async Task<CommonResponse> SaveLogin(AccountLoginInfoObject accountLoginInfo)
    {
        return await CommonProcess(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountLoginInfoSaveLogin,
                QueryParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "account_id",
                        StringValue = accountLoginInfo.AccountId.AsDefaultString()
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "username",
                        StringValue = accountLoginInfo.UserName
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "password",
                        StringValue = accountLoginInfo.Password
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "auth_type",
                        StringValue = accountLoginInfo.AuthType.EnumAsInt().AsDefaultString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "login_type",
                        StringValue = accountLoginInfo.LoginType.EnumAsInt().AsDefaultString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "full_name",
                        StringValue = accountLoginInfo.FullName
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "phone_number",
                        StringValue = accountLoginInfo.PhoneNumber
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "device_id",
                        StringValue = accountLoginInfo.DeviceId
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "token",
                        StringValue = accountLoginInfo.Token
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "refresh_token",
                        StringValue = accountLoginInfo.RefreshToken
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "session_id",
                        StringValue = accountLoginInfo.SessionId
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "permission",
                        StringValue = accountLoginInfo.Permission.AsDefaultString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "minute_expire",
                        StringValue = accountLoginInfo.MinuteExpire.AsDefaultString()
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "token_status",
                        StringValue = accountLoginInfo.TokenStatus.EnumAsInt().AsDefaultString()
                    },
                ]
            };
            var execStringResponse = await fldMasterPgService.Execute(execution);
            if (!execStringResponse.Status || execStringResponse.Data == null)
            {
                response.SetFail($"Could not Find ExecString {AuthenServiceQueryCode.AccountLoginInfoSaveLogin}",
                    execStringResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }
            response.SetSuccess();
        });
    }

    private Task<AccountLoginInfoObject> CreateToken(
        AccountObject account,
        EAuthentication authType,
        ELoginType loginType = ELoginType.Default,
        string? oldRefreshToken = null,
        int expireMinutes = 0
    )
    {
        if (!string.IsNullOrEmpty(oldRefreshToken))
        {
            // await authenService.RemoveLoginInfo(oldRefreshToken);
        }

        string sessionKey = $"{SessionIdPrefix}-{account.UserName}-{CommonUtilityMode.GenerateGuid()}";
        int minuteExpire = expireMinutes == 0
            ? EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt()
            : expireMinutes;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig()
            .AsDefaultString());
        string uniqueNameKey = JwtRegisteredClaimNames.UniqueName;
        List<Claim> claims =
        [
            new(ContextService.SessionCode, sessionKey),
            new($"{MinuteExpirePrefix}", minuteExpire.ToString()),
            new(uniqueNameKey, account.UserName),
            new(JwtRegisteredClaimNames.Sid, account.Id.AsDefaultString()),
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

        // set
        AccountLoginInfoObject accountLoginInfo =
            new AccountLoginInfoObject
            {
                // Id = default,
                AccountId = account.Id,
                UserName = account.UserName,
                Password = account.Password,
                FullName = account.FullName,
                PhoneNumber = account.PhoneNumber,
                AuthType = authType,
                LoginType = loginType,
                SessionId = sessionKey,
                MinuteExpire = minuteExpire,
                RefreshToken = CommonUtilityMode.GenerateGuid(),
                TokenStatus = ETokenStatus.Active,
                Token = tokenValue,
            };

        // await authenService.SetLoginInfo(sessionKey, accountLoginInfo, accountLoginInfo.MinuteExpire);
        return Task.FromResult(accountLoginInfo);
    }
}