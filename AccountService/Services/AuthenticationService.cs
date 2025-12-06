using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountServiceObject;
using AccountServiceObject.BusinessObjects;
using AccountServiceObject.CommandObjects;
using AccountServiceObject.QueryObjects;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonMode;
using CommonObject;
using CommonWebApplication.Services;
using DbFactory.Redis;
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
    IRedisFactoryWrapper redisCachingFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IAuthenticationService
{
    public async Task<CommonResponse<AccountLoginInfoObject>> Signup(AccountSignupCommand command)
    {
        return await CommonProcess<AccountLoginInfoObject>(async (response) =>
        {
            var checkExistExecution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountGetByUsernameOrPhoneNumberOrEmail,
                ExecParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "phone_number",
                        StringValue = command.PhoneNumber
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "email",
                        StringValue = command.Email
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "username",
                        StringValue = command.UserName
                    },
                ]
            };
            var existAccountResponse = await fldMasterPgService.Execute(checkExistExecution);
            if (!existAccountResponse.Status)
            {
                response.SetFail("Could not check Email/PhoneNumber/UserName",
                    existAccountResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            if (existAccountResponse.Data != null) // existed
            {
                List<AccountObject>? accountObjects = existAccountResponse.Data?
                    .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountObject))?
                    .Cast<AccountObject>().ToList();
                // 
                if (accountObjects?.Any(x => x.PhoneNumber == command.PhoneNumber) ?? false)
                    response.SetFail("NumberPhone is Existed", existAccountResponse.ErrorCode ?? EErrorCode.NotFound);
                if (accountObjects?.Any(x => x.UserName == command.UserName) ?? false)
                    response.SetFail("UserName is Existed", existAccountResponse.ErrorCode ?? EErrorCode.NotFound);
                if (accountObjects?.Any(x => x.Email == command.Email) ?? false)
                    response.SetFail("UserName is Existed", existAccountResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountSignup,
                ExecParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "phone_number",
                        StringValue = command.PhoneNumber
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "email",
                        StringValue = command.Email
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "username",
                        StringValue = command.UserName
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "password",
                        StringValue = command.Password
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "full_name",
                        StringValue = command.FullName
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "avatar_url",
                        StringValue = command.AvatarUrl
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "permission",
                        StringValue = 0.AsDefaultEnum<EPermission>().AsDefaultString()
                    }
                ]
            };
            var execSigninResponse = await fldMasterPgService.Execute(execution);
            if (!execSigninResponse.Status)
            {
                response.SetFail("Could not Find ExecString", execSigninResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            var loginResponse = await Login(new AccountLoginQuery
            {
                UserName = command.UserName,
                Password = command.Password,
                AuthType = command.AuthType,
                ClientId = command.ClientId,
            });

            if (!loginResponse.Status)
            {
                response.SetFail("Save accountLoginInfo fail after create account");
                return;
            }

            response.Data = loginResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountLoginInfoObject>> Login(AccountLoginQuery query)
    {
        return await CommonProcess<AccountLoginInfoObject>(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountGetByUsernameAndPassword,
                ExecParams =
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
            var execAccountResponse = await fldMasterPgService.Execute(execution);
            if (!execAccountResponse.Status || execAccountResponse.Data == null)
            {
                response.SetFail("Could not Find ExecString", execAccountResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            AccountObject? accountObject = execAccountResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountObject))?
                .Cast<AccountObject>().FirstOrDefault();

            if (accountObject == null)
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountInfoAliasTestObject>'");
                return;
            }

            AccountLoginInfoObject accountLoginInfoObject = await CreateToken(
                accountObject, query.AuthType /*, ELoginType.Default*/);

            if (query.IsEnableMultiDevice)
            {
            }

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
                Code = AuthenServiceQueryCode.AccountLoginInfoGetByRefreshToken,
                ExecParams =
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

            List<AccountLoginInfoObject>? accountInfoObjects = execStringResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountLoginInfoObject))?
                .Cast<AccountLoginInfoObject>()
                .ToList();

            if (accountInfoObjects is not { Count: > 0 })
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountInfoAliasTestObject>'");
                return;
            }

            AccountLoginInfoObject accountInfoObject = accountInfoObjects.First();
            if (accountInfoObject.SessionId != query.SessionId || accountInfoObject.TokenStatus != ETokenStatus.Active)
            {
                response.SetFail("SessionId does not match");
                return;
            }

            // logout for old session
            if (!(await SaveLogoutWhenLogoutOrRefreshToken(accountInfoObject)).Status)
            {
                response.SetFail("AccountLogout save failure");
                return;
            }

            // get account to sync for new session 
            var accountExecution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountGetById,
                ExecParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "id",
                        StringValue = accountInfoObject.AccountId.AsDefaultString(),
                    },
                ]
            };
            var accountExecutionResponse = await fldMasterPgService.Execute(accountExecution);
            if (!accountExecutionResponse.Status || accountExecutionResponse.Data == null)
            {
                response.SetFail("Could not Find ExecString",
                    accountExecutionResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            AccountObject? accountObject = accountExecutionResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountObject))?
                .Cast<AccountObject>().FirstOrDefault();

            if (accountObject == null)
            {
                response.SetFail("Incorrect data type of 'IEnumerable<AccountObject>'");
                return;
            }

            AccountLoginInfoObject accountLoginInfoObject = await CreateToken(
                accountObject, query.AuthType, ELoginType.Default, query.SessionId);
            if (!(await SaveLogin(accountLoginInfoObject)).Status)
            {
                response.SetFail("AccountLogin save failure");
                return;
            }

            response.Data = accountLoginInfoObject;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public async Task<CommonResponse<AccountLoginInfoObject>> GetLogonTokenBySessionId(
        AccountGetLogonInfoBySessionIdQuery query)
    {
        return await CommonProcess<AccountLoginInfoObject>(
            async (response) =>
            {
                if (EApplicationConfiguration.IsUseRedisCache.GetAppSettingConfig().AsDefaultBool())
                {
                    var accountInfoCache =
                        await redisCachingFactoryWrapper.GetStringAsync(query.SessionIdWithPrefixCode);
                    if (accountInfoCache != null)
                    {
                        var cacheValue = accountInfoCache.Result.Values.FirstOrDefault()?.ValueAsObject.AsEmptyString();
                        if (string.IsNullOrEmpty(cacheValue))
                        {
                            response.SetSuccess();
                            return (response, EControlFlow.Continue);
                        }

                        if (JsonConverter.TryFromJson<AccountLoginInfoObject>(cacheValue, out var accountInfoObject))
                        {
                            if (accountInfoObject != null)
                            {
                                response.Data = accountInfoObject;
                                response.SetSuccess();
                                return (response, EControlFlow.Break); // cache OK => break;
                            }
                        }
                    }
                }

                response.SetSuccess(); // avoid breaking case
                return (response, EControlFlow.Continue); // cache fail / unuse cache => continue;
            },
            async (response) =>
            {
                var logoutExecution = new TblFldExecution
                {
                    Code = AuthenServiceQueryCode.AccountLoginInfoGetBySessionId,
                    ExecParams =
                    [
                        new TblFldExecutionParam
                        {
                            ParamName = "session_id",
                            StringValue = query.SessionId.AsDefaultString(),
                        },
                    ]
                };

                var logoutExecutionResponse = await fldMasterPgService.Execute(logoutExecution);
                if (!logoutExecutionResponse.Status || logoutExecutionResponse.Data == null)
                {
                    response.SetFail("Could not found data", logoutExecutionResponse.ErrorCode ?? EErrorCode.NotFound);
                    return (response, EControlFlow.Break);
                }

                AccountLoginInfoObject? accountInfoObject = logoutExecutionResponse.Data?
                    .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountLoginInfoObject))?
                    .Cast<AccountLoginInfoObject>().FirstOrDefault();

                if (accountInfoObject == null)
                {
                    response.SetFail("Incorrect data type of AccountInfoAliasTestObject");
                    return (response, EControlFlow.Break);
                }

                response.Data = accountInfoObject;
                response.SetSuccess();
                return (response, EControlFlow.Break);
            }
        );
    }

    public async Task<CommonResponse<string>> LogOut(AccountLogoutQuery query)
    {
        return await CommonProcess<string>(async (response) =>
        {
            var logoutExecution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountLoginInfoGetBySessionId,
                ExecParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "session_id",
                        StringValue = query.SessionId.AsDefaultString(),
                    },
                ]
            };
            var logoutExecutionResponse = await fldMasterPgService.Execute(logoutExecution);
            if (!logoutExecutionResponse.Status || logoutExecutionResponse.Data == null)
            {
                response.SetFail("Could not Find ExecString", logoutExecutionResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            AccountLoginInfoObject? accountInfoObject = logoutExecutionResponse.Data?
                .ConverterToChildOfBaseAccountObjectFromGrpcTable(typeof(AccountLoginInfoObject))?
                .Cast<AccountLoginInfoObject>().FirstOrDefault();

            if (accountInfoObject == null)
            {
                response.SetFail("Incorrect data type of AccountInfoAliasTestObject");
                return;
            }

            await SaveLogoutWhenLogoutOrRefreshToken(accountInfoObject);
            await DeleteCachingToken(query.SessionIdWithPrefixCode); // delete token key from caching db
            response.Data = "Logout successful";
            response.SetSuccess();
        });
    }

    #region Private Method

    private async Task<CommonResponse> SaveLogin(AccountLoginInfoObject accountLoginInfo)
    {
        return await CommonProcess(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountLoginInfoSaveLogin,
                ExecParams =
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
                        StringValue = accountLoginInfo.Permission.EnumAsInt().AsDefaultString()
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
                    new TblFldExecutionParam()
                    {
                        ParamName = "email",
                        StringValue = accountLoginInfo.Email
                    },
                    new TblFldExecutionParam()
                    {
                        ParamName = "avatar_url",
                        StringValue = accountLoginInfo.AvatarUrl
                    },
                ]
            };
            var execStringResponse = await fldMasterPgService.Execute(execution);
            if (!execStringResponse.Status)
            {
                response.SetFail($"Could not Find ExecString {AuthenServiceQueryCode.AccountLoginInfoSaveLogin}",
                    execStringResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.SetSuccess();
        });
    }

    private async Task<CommonResponse> SaveLogoutWhenLogoutOrRefreshToken(AccountLoginInfoObject accountLoginInfo)
    {
        return await CommonProcess(async (response) =>
        {
            var execution = new TblFldExecution
            {
                Code = AuthenServiceQueryCode.AccountLoginInfoSaveLogOut,
                ExecParams =
                [
                    new TblFldExecutionParam
                    {
                        ParamName = "token_status",
                        StringValue = ETokenStatus.Inactive.EnumAsInt().AsDefaultString(),
                    },
                    new TblFldExecutionParam
                    {
                        ParamName = "session_id",
                        StringValue = accountLoginInfo.SessionId.AsDefaultString()
                    },
                ]
            };
            var execStringResponse = await fldMasterPgService.Execute(execution);
            if (!execStringResponse.Status)
            {
                response.SetFail($"Could not Find ExecString {AuthenServiceQueryCode.AccountLoginInfoSaveLogOut}",
                    execStringResponse.ErrorCode ?? EErrorCode.NotFound);
                return;
            }

            response.SetSuccess();
        });
    }

    private async Task<AccountLoginInfoObject> CreateToken(
        AccountObject account,
        EAuthentication authType,
        ELoginType loginType = ELoginType.Default,
        string? oldSessionKey = null,
        int expireMinutes = 0
    )
    {
        string sessionKey = $"{ContextService.SessionIdPrefix}-{account.UserName}-{CommonUtilityMode.GenerateGuidAsString()}";
        int minuteExpire = expireMinutes == 0
            ? EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt()
            : expireMinutes;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(EApplicationConfiguration.JwtTokensKey.GetAppSettingConfig()
            .AsDefaultString());
        List<Claim> claims =
        [
            new(ContextService.SessionCode, sessionKey),
            new(ContextService.TokenCreatedUtc, DateTime.UtcNow.AddMinutes(minuteExpire).ToIso8601()),
            new(ContextService.Permission, account.Permission.EnumAsInt().AsDefaultString()),
            new($"{ContextService.MinuteExpirePrefix}", minuteExpire.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, account.UserName),
            new(ContextService.LoginTypeEnumCode, loginType.EnumAsInt().AsDefaultString()),
            new(JwtRegisteredClaimNames.Sid, account.Id.AsDefaultString()),
            new(JwtHeaderParameterNames.Typ, loginType.GetDisplayName())
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
                Email = account.Email,
                AvatarUrl = account.AvatarUrl,
                AuthType = authType,
                LoginType = loginType,
                SessionId = sessionKey,
                MinuteExpire = minuteExpire,
                RefreshToken = CommonUtilityMode.GenerateGuidAsString(),
                Permission = account.Permission,
                TokenStatus = ETokenStatus.Active,
                Token = tokenValue,
            };

        if (EApplicationConfiguration.IsUseRedisCache.GetAppSettingConfig().AsDefaultBool())
        {
            if (!string.IsNullOrEmpty(oldSessionKey))
                await DeleteCachingToken($"{AccountCachingCode.PrefixCachingAccountToken}{oldSessionKey}");
            await redisCachingFactoryWrapper.SetAsync($"{AccountCachingCode.PrefixCachingAccountToken}{sessionKey}",
                JsonConverter.ToJson(accountLoginInfo),
                TimeSpan.FromMinutes(minuteExpire));
        }

        return accountLoginInfo;
    }

    private async Task DeleteCachingToken(string cachingTokenKey)
    {
        await redisCachingFactoryWrapper.DeleteAsync(cachingTokenKey);
    }

    #endregion Private Method
}