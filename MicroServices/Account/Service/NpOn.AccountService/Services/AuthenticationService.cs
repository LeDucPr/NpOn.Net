using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Events;
using Common.Infrastructures.NpOn.RabbitMqExtMs.Senders;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;
using Microsoft.IdentityModel.Tokens;
// using Common.Infrastructures.NpOn.KafkaExtCm.Events;
// using Common.Infrastructures.NpOn.KafkaExtCm.Senders;

namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class AuthenticationService(
    IPostgresBaseRepository baseRepository,
    IAuthenticationStorageAdapter authenticationStorageAdapter,
    IAccountInfoStorageAdapter accountInfoStorageAdapter,
    IAccountPermissionService accountPermissionService,
    IAccountTokenAndPermissionRedisRepository redisRepository,
    IRabbitMqProducer rabbitMqProducer,
    // IKafkaProducer kafkaProducer,
    ILogger<CommonService> logger
) : CommonService(logger), IAuthenticationService
{
    private readonly int _expireTokenMinutes =
        EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt() != 0
            ? EApplicationConfiguration.LoginExpiresTime.GetAppSettingConfig().AsDefaultInt()
            : 480;

    #region Signup And Accept

    public async Task<CommonResponse<AccountLoginRModel>> Signup(AccountSignupCommand command)
    {
        return await CommonProcess<AccountLoginRModel>(async (response) =>
        {
            List<AccountRModel>? existAccounts =
                await authenticationStorageAdapter.AccountGetByNumberPhoneOrEmailOrUsername(command.PhoneNumber,
                    command.Email,
                    command.UserName);

            if (existAccounts is { Count: > 0 }) // existed
            {
                if (existAccounts.Any(x => x.PhoneNumber == command.PhoneNumber))
                    response.SetFail("NumberPhone is Existed");
                if (existAccounts.Any(x => x.UserName == command.UserName))
                    response.SetFail("UserName is Existed");
                if (existAccounts.Any(x => x.Email == command.Email))
                    response.SetFail("UserName is Existed");
                return;
            }

            Contracts.NpOn.AccountServiceContract.Domains.Account account =
                new Contracts.NpOn.AccountServiceContract.Domains.Account(command);
            if (!(await baseRepository.Add([account]))?.Status ?? false)
            {
                response.SetFail("Add new Account fail");
                return;
            }

            CommonResponse<AccountLoginRModel> loginResponse = await Login(new AccountLoginQuery
            {
                UserName = command.UserName,
                Password = command.Password,
                AuthType = command.AuthType,
                ClientId = command.ClientId,
            });

            if (!loginResponse.Status)
            {
                response.SetFail("Save Account Login fail after create account");
                return;
            }

            response.Data = loginResponse.Data;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> ChangeAccountStatus(AccountSetStatusCommand command)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            string accountIdAsString = command.AccountId.AsDefaultString();
            AccountRModel? existAccounts =
                await authenticationStorageAdapter.AccountGetById(accountIdAsString);
            if (existAccounts == null)
            {
                response.SetFail("Account not found");
                response.Data = false;
                return;
            }

            if (existAccounts.Status == command.AccountStatus)
            {
                response.Data = true;
                response.SetSuccess();
                return;
            }

            Contracts.NpOn.AccountServiceContract.Domains.Account accountChangeStatus =
                new Contracts.NpOn.AccountServiceContract.Domains.Account(existAccounts);
            accountChangeStatus.ChangeStatus(command);
            if (!(await baseRepository.Update([accountChangeStatus]))?.Status ?? false)
            {
                response.SetFail($"Account Change Status {command.AccountStatus.AsDefaultString()} fail");
                response.Data = false;
                return;
            }

            if (command.AccountStatus != EAccountStatus.Active)
            {
                var isDeleteCachingTokenStorage =
                    await redisRepository.DeleteCachingTokenStorageAndTokensByAccountId(accountIdAsString);
                var isDeleteCachePermissionCache =
                    await redisRepository.DeleteCachingPermissionExceptionsByAccountId(accountIdAsString);
                if (!isDeleteCachingTokenStorage || !isDeleteCachePermissionCache)
                {
                    response.SetFail($"Account Change Status Cache {command.AccountStatus.AsDefaultString()} fail");
                    response.Data = false;
                    return;
                }
            }

            response.Data = true;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<bool>> ChangeAccountPassword(AccountChangePasswordCommand command)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            string accountIdAsString = command.AccountId.AsDefaultString();
            AccountRModel? existAccounts =
                await authenticationStorageAdapter.AccountGetById(accountIdAsString);
            if (existAccounts == null)
            {
                response.SetFail("Account not found");
                response.Data = false;
                return;
            }

            Contracts.NpOn.AccountServiceContract.Domains.Account accountChangeStatus =
                new Contracts.NpOn.AccountServiceContract.Domains.Account(existAccounts);
            accountChangeStatus.ChangeNewPassword(command);
            if (!(await baseRepository.Update([accountChangeStatus]))?.Status ?? false)
            {
                response.SetFail($"Account Change Password fail");
                response.Data = false;
                return;
            }

            response.Data = true;
            response.SetSuccess();
        });
    }

    #endregion Signup And Accept

    public async Task<CommonResponse<AccountLoginRModel>> Login(AccountLoginQuery query)
    {
        return await CommonProcess<AccountLoginRModel>(async (response) =>
        {
            AccountRModel? accountRModel =
                await authenticationStorageAdapter.AccountGetByUsernameAndPassword(
                    query.UserName.AsDefaultString(), query.Password.AsDefaultString());
            if (accountRModel == null || accountRModel.Status != EAccountStatus.Active)
            {
                response.SetFail("Account not found");
                return;
            }

            AccountLoginRModel accountLoginRModel = CreateToken(
                accountRModel, query.AuthType /*, ELoginType.Default*/);

            if (query.IsEnableMultiDevice)
            {
            }

            // kafkaProducer.AddEvent(new KafkaEvent<AccountSaveLoginEvent>()
            // {
            //     MessageContent = accountLoginRModel.ToLoginEvent()
            // });

            rabbitMqProducer.AddEvent(new RabbitMqEvent<AccountSaveLoginEvent>()
            {
                MessageContent = accountLoginRModel.ToLoginEvent()
            });
            response.Data = accountLoginRModel;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountLoginRModel>> RefreshToken(AccountRefreshTokenQuery query)
    {
        return await CommonProcess<AccountLoginRModel>(async (response) =>
        {
            List<AccountLoginRModel>?
                accountInfoObjects =
                    await authenticationStorageAdapter.AccountLoginInfoGetByRefreshToken(query.RefreshToken);

            if (accountInfoObjects is not { Count: > 0 })
            {
                response.SetFail("RefreshToken not found");
                return;
            }

            AccountLoginRModel accountRModel = accountInfoObjects.First();
            if (accountRModel.SessionId != query.SessionId || accountRModel.TokenStatus != ETokenStatus.Active)
            {
                response.SetFail("SessionId does not match");
                return;
            }

            // logout for old session
            rabbitMqProducer.AddEvent(new RabbitMqEvent<AccountSaveLogoutEvent>()
            {
                MessageContent = accountRModel.ToLogoutEvent(query.SessionId),
            });

            // get account to sync for new session 

            string accountId = accountRModel.AccountId.AsDefaultString();
            AccountRModel? accountObject =
                await authenticationStorageAdapter.AccountGetById(accountId);
            if (accountObject == null)
            {
                response.SetFail("Account not found");
                return;
            }

            AccountLoginRModel accountLoginRModel = CreateToken(
                accountObject, query.AuthType /*, ELoginType.Default*/);

            rabbitMqProducer.AddEvent(new RabbitMqEvent<AccountSaveLoginEvent>()
            {
                MessageContent = accountLoginRModel.ToLoginEvent(query.SessionId)
            });

            response.Data = accountLoginRModel;
            response.SetSuccess();
        });
    }

    public Task<CommonResponse<INpOnGrpcObject>> LoginToken(CommonJsonQuery query)
    {
        throw new NotImplementedException();
    }

    public async Task<CommonResponse<AccountLoginRModel>> GetLogonTokenBySessionId(
        AccountGetLogonInfoBySessionIdQuery query)
    {
        return await CommonProcess<AccountLoginRModel>(
            async (response) =>
            {
                bool isUseCachingDb = EApplicationConfiguration.IsUseRedisCache.GetAppSettingConfig().AsDefaultBool();
                if (!isUseCachingDb)
                {
                    response.SetSuccess(); // avoid breaking case
                    return (response, EControlFlow.Continue);
                }

                AccountLoginRModel? accountLoginRModel =
                    await redisRepository.GetAccountLogonBySessionIdWithPrefixCode(query.SessionId);
                if (accountLoginRModel != null)
                {
                    response.Data = accountLoginRModel;
                    response.SetSuccess();
                    return (response, EControlFlow.Break); // cache OK => break;
                }

                response.SetSuccess(); // avoid breaking case
                if (!isUseCachingDb)
                    return (response, EControlFlow.Continue); // cache fail / unuse cache => continue;
                return (response, EControlFlow.Break);
            },
            async (response) =>
            {
                AccountLoginRModel? accountLoginInfoRModel =
                    await authenticationStorageAdapter.AccountLoginInfoGetBySessionId(query.SessionId);
                if (accountLoginInfoRModel == null)
                {
                    response.SetFail("AccountLoginInfo not found");
                    return (response, EControlFlow.Break);
                }

                response.Data = accountLoginInfoRModel;
                response.SetSuccess();
                return (response, EControlFlow.Break);
            }
        );
    }

    public async Task<CommonResponse<bool>> IsValidLogonPermissionExceptionControllers(
        AccountPermissionExceptionCachingCheckValidQuery query)
    {
        return await CommonProcess<bool>(async (response) =>
        {
            bool isValid = false;
            var existedCachingControllers =
                await redisRepository.GetCachingPermissionException(query.AccountId);
            string? findControllerCode = existedCachingControllers?.FirstOrDefault();
            if (findControllerCode == null && query.IsHasBasePermission)
                isValid = true;
            else if (findControllerCode != null && !query.IsHasBasePermission)
                isValid = true;
            response.Data = isValid;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<string>> LogOut(AccountLogoutQuery query)
    {
        return await CommonProcess<string>(async (response) =>
        {
            var logonTokenResponse = await GetLogonTokenBySessionId(new AccountGetLogonInfoBySessionIdQuery()
            {
                SessionId = query.SessionId,
            });
            if (!logonTokenResponse.Status || logonTokenResponse.Data == null)
            {
                AccountLoginRModel? accountLoginInfoRModel =
                    await authenticationStorageAdapter.AccountLoginInfoGetBySessionId(query.SessionId);
                if (accountLoginInfoRModel == null)
                {
                    response.SetFail("Invalid Token");
                    return;
                }

                rabbitMqProducer.AddEvent(new RabbitMqEvent<AccountSaveLogoutEvent>()
                {
                    MessageContent = accountLoginInfoRModel.ToLogoutEvent(query.SessionId),
                });
                response.SetFail("Invalid Token");
                return;
            }

            response.Data = "Logout successful";
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> SaveLogin(AccountSaveLoginEvent @event)
    {
        return await CommonProcess(async (response) =>
        {
            AccountLoginRModel accountLoginRModel = @event.ToObject();
            AccountLogin domain = new AccountLogin(accountLoginRModel);
            if (!(await baseRepository.Add([domain]))?.Status ?? false)
            {
                response.SetFail($"AccountLoginInfo save login fail");
                return;
            }

            string accountId = @event.AccountId.AsDefaultString();
            string sessionId = @event.SessionId;

            await redisRepository.AddCachingToken(sessionId, accountLoginRModel);
            await redisRepository.AddToCachingTokenStorageByAccountId(accountId, sessionId);
            // permission exception cache
            var permissionExceptionResponse =
                await accountPermissionService.AccountPermissionExceptionQuickGetByAccountId(
                    new AccountPermissionExceptionGetByAccountIdQuery()
                    {
                        AccountId = accountId,
                    });
            await redisRepository.AddCachingPermissionException(accountId, permissionExceptionResponse.Data);

            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> SaveLogout(AccountSaveLogoutEvent @event)
    {
        return await CommonProcess(async (response) =>
        {
            AccountLoginRModel accountLogin = @event.ToObject();
            AccountLogin domain = new AccountLogin(accountLogin);
            domain.Logout();

            if (!(await baseRepository.Update([domain]))?.Status ?? false)
            {
                response.SetFail($"AccountLoginInfo save logout fail");
                return;
            }

            // delete token key from caching db
            await redisRepository.DeleteCachingToken(@event.SessionId);
            await redisRepository.DeleteCachingTokenStorageAndTokensByAccountId(
                accountLogin.AccountId.AsDefaultString(), [accountLogin.SessionId]);

            response.SetSuccess();
        });
    }


    #region OldSystem

    public async Task<CommonResponse> AccountSyncFromOldSystem(AccountSyncFromOldSystemCommand[] commands)
    {
        return await CommonProcess(async (response) =>
        {
            string[] accountIds = commands.Where(x => x.AccountId.AsDefaultGuid() != Guid.Empty)
                .Select(x => x.AccountId.AsDefaultString()).ToArray();
            if (accountIds.Length != commands.Length)
            {
                response.SetFail("Some AccountId is invalid");
                return;
            }

            List<Contracts.NpOn.AccountServiceContract.Domains.Account> accounts = [];
            List<AccountInfo> accountInfos = [];
            List<AccountAddress> addresses = [];
            var existedAccountInfos = await accountInfoStorageAdapter.AccountInfoActiveGetByAccountIds(accountIds);
            var existedAddresses = await accountInfoStorageAdapter.AccountAddressesDefaultGetByAccountIds(accountIds);
            foreach (AccountSyncFromOldSystemCommand command in commands)
            {
                // account
                var account = new Contracts.NpOn.AccountServiceContract.Domains.Account(command);
                accounts.Add(account);

                // account info
                AccountInfoRModel? existedAccountInfo =
                    existedAccountInfos?.FirstOrDefault(x => x.AccountId == command.AccountId);
                if (existedAccountInfo != null)
                {
                    AccountInfo accountInfo = new AccountInfo(existedAccountInfo);
                    accountInfo.Change(command);
                    accountInfos.Add(accountInfo);
                }
                else
                {
                    AccountInfo accountInfo = new AccountInfo(command);
                    accountInfos.Add(accountInfo);
                }

                // account address
                AccountAddressRModel? existedAddress =
                    existedAddresses?.FirstOrDefault(x => x.AccountId == command.AccountId);

                if (existedAddress != null)
                {
                    AccountAddress address = new AccountAddress(existedAddress);
                    address.Change(command);
                    addresses.Add(address);
                }
                else
                {
                    AccountAddress address = new AccountAddress(command);
                    addresses.Add(address);
                }
            }
            
            if (accounts.Count > 0 && (!(await baseRepository.Merge(accounts))?.Status ?? false))
            {
                response.SetFail("Sync Account from Old System fail");
                return;
            }

            if (accountInfos.Count > 0 && (!(await baseRepository.Merge(accountInfos))?.Status ?? false))
            {
                response.SetFail("Sync AccountInfo from Old System fail");
                return;
            }

            if (addresses.Count > 0 && (!(await baseRepository.Merge(addresses))?.Status ?? false))
            {
                response.SetFail("Sync AccountAddress from Old System fail");
                return;
            }

            response.SetSuccess();
        });
    }

    #endregion OldSystem

    #region Private Method

    private AccountLoginRModel CreateToken(
        AccountRModel account,
        EAuthentication authType,
        ELoginType loginType = ELoginType.Default
    )
    {
        string sessionKey =
            $"{ContextService.SessionIdPrefix}-{account.UserName}-{CommonUtilityMode.GenerateGuidAsString()}";
        int minuteExpire = _expireTokenMinutes;

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
        AccountLoginRModel accountLogin =
            new AccountLoginRModel
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

        return accountLogin;
    }

    #endregion Private Method
}