using System.Security.Cryptography;
using System.Text;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Attributes;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Controllers.NpOn.SSO.Mappings.Account;
using Controllers.NpOn.SSO.Requests;
using Controllers.NpOn.SSO.Validators;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Service.NpOn.IAccountService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers.NpOn.SSO.Controllers;

public class AccountController(
    ILogger<AccountController> logger,
    ContextService contextService,
    IAuthenticationService authenticationService,
    IAccountPermissionService accountPermissionService
) : BaseSsoController(logger, contextService)
{
    private readonly ContextService _contextService = contextService;

    [Obsolete("Obsolete")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> Signup([FromBody] AccountSignupRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = AccountSignupRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            var signupResponse = await authenticationService.Signup(new AccountSignupCommand
            {
                AuthType = request.AuthType,
                ClientId = _contextService.ClientId,
                Email = request.Email.AsEmptyString(),
                PhoneNumber = request.PhoneNumber.AsEmptyString(),
                UserName = request.UserName.AsEmptyString(),
                Password = CreateHashPassword(request.Password)
                    .AsEmptyString(),
                LoginType = ELoginType.Default,
                SignupIp = _contextService.GetIp(),
                DeviceSignupInfo = request.DeviceInfo,
                AuthenApplicationId = request.AppId,
                FullName = request.FullName.AsEmptyString(),
                AvatarUrl = request.AvatarUrl
            });

            if (!signupResponse.Status)
            {
                string errMessages = signupResponse.ErrorMessages.AsArrayJoin();
                response.SetFail(!string.IsNullOrWhiteSpace(errMessages) ? errMessages : "Signup fail");
                return;
            }

            response.Data = new
            {
                Model = signupResponse.Data?.ToModel(),
            };
            response.SetSuccess();
        });
    }


    [PermissionController(EPermission.Administrator)]
    [PermissionRequired]
    [HttpPost]
    public async Task<CommonApiResponse<string>> ChangeAccountStatus([FromBody] AccountChangeStatusRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var signupResponse = await authenticationService.ChangeAccountStatus(new AccountSetStatusCommand()
            {
                AccountId = request.AccountId,
                AccountStatus = request.AccountStatus,
            });

            if (!signupResponse.Status)
            {
                string errMessages = signupResponse.ErrorMessages.AsArrayJoin();
                response.SetFail(!string.IsNullOrWhiteSpace(errMessages) ? errMessages : "Change Account Status fail");
                return;
            }

            response.Data = "Change Account Status successful";
            response.SetSuccess();
        });
    }


    [PermissionController(EPermission.Administrator)]
    [PermissionRequired]
    [HttpPost]
    [Obsolete("Obsolete")]
    public async Task<CommonApiResponse<object>> ChangePassword([FromBody] ChangeAccountPasswordRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = ChangeAccountPasswordRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            string? newPassword = request.NewPassword;
            if (string.IsNullOrEmpty(newPassword))
                newPassword = GeneratePassword();
            var signupResponse = await authenticationService.ChangeAccountPassword(new AccountChangePasswordCommand()
            {
                AccountId = request.AccountId,
                NewPassword = CreateHashPassword(newPassword)
                    .AsEmptyString(),
            });

            if (!signupResponse.Status || signupResponse.Data)
            {
                response.SetFail("Change Account Password fail");
                return;
            }

            response.Data = request.NewPassword;
            response.SetSuccess();
        });
    }


    [Obsolete("Obsolete")]
    [AllowAnonymous]
    [HttpPost]
    public async Task<CommonApiResponse<object>> Login([FromBody] AccountLoginRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = AccountLoginRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            bool isUseMultiDevice = !string.IsNullOrWhiteSpace(request.DeviceInfo) &&
                                    request.AuthType == EAuthentication.WebApp;
            AccountLoginQuery inputQuery = new AccountLoginQuery
            {
                Email = request.Email,
                ClientId = _contextService.ClientId,
                PhoneNumber = request.PhoneNumber,
                UserName = request.UserName,
                Password = CreateHashPassword(request.Password),
                DeviceLoginInfo = request.DeviceInfo,
                LoginType = request.LoginType,
                Ip = _contextService.GetIp(),
                AuthenApplicationId = request.AppId,
                AuthType = request.AuthType,
                IsEnableMultiDevice = isUseMultiDevice
            };
            var accountLoginResponse = await authenticationService.Login(inputQuery);
            if (!accountLoginResponse.Status)
            {
                response.SetFail(accountLoginResponse.ErrorMessages);
                return;
            }

            if (accountLoginResponse.Data == null || string.IsNullOrEmpty(accountLoginResponse.Data.Token))
            {
                response.SetFail("Login fail");
                return;
            }

            // response.Data = await LoginProcess(tokenResult.Data);
            response.Data = new
            {
                Model = accountLoginResponse.Data.ToModel(),
            };
            response.SetSuccess();
        });
    }


    [HttpPost]
    public async Task<CommonApiResponse<object>> RefreshToken([FromBody] AccountRefreshTokenRequest request)
    {
        return await ProcessRequest<object>(async (response) =>
        {
            var validator = AccountRefreshTokenValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            AccountRefreshTokenQuery inputQuery = new AccountRefreshTokenQuery
            {
                RefreshToken = request.RefreshToken,
                DeviceInfo = request.DeviceInfo,
                LoginType = request.LoginType,
                AuthType = request.AuthType,
                SessionId = _contextService.GetSessionKey().AsDefaultString(),
                ProcessUId = _contextService.GetAccountIdAsString(),
            };
            var accountLoginResponse = await authenticationService.RefreshToken(inputQuery);
            if (!accountLoginResponse.Status)
            {
                response.SetFail(accountLoginResponse.ErrorMessages);
                return;
            }

            if (accountLoginResponse.Data == null || string.IsNullOrEmpty(accountLoginResponse.Data.Token))
            {
                response.SetFail("Login invalid");
                return;
            }

            // response.Data = await LoginProcess(tokenResult.Data);
            response.Data = new
            {
                Model = accountLoginResponse.Data.ToModel(),
            };
            response.SetSuccess();
        });
    }


    [HttpPost]
    public async Task<CommonApiResponse<string>> Logout([FromBody] AccountLogoutRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var logoutResponse = await authenticationService.LogOut(
                new AccountLogoutQuery
                {
                    SessionId = _contextService.GetSessionKey().AsDefaultString(),
                    ProcessUId = _contextService.GetAccountIdAsString(),
                });

            response.Data = logoutResponse.Status ? "Logout successful" : "Logout fail";
            if (!logoutResponse.Status)
            {
                response.SetFail(logoutResponse.ErrorMessages);
                return;
            }

            response.SetSuccess();
        });
    }


    [PermissionController(EPermission.Administrator)]
    [PermissionRequired("Add/Change Permission for user (API)", EPermission.Administrator)]
    [HttpPost]
    public async Task<CommonApiResponse<string>> ChangePermission(
        [FromBody] AccountPermissionAddOrChangeRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountPermissionRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            List<AccountPermissionExceptionAddOrChangeCommand> commands = [];
            foreach (var controller in request.Controllers)
            {
                commands.Add(new AccountPermissionExceptionAddOrChangeCommand
                {
                    AccountId = request.AccountId,
                    ControllerCode = controller.ControllerCode,
                    AccessPermission = controller.AccessPermission,
                });
            }

            var addOrChangePermissionResponse =
                await accountPermissionService.AddOrChangeAccountPermissionException(commands.ToArray());

            response.Data = addOrChangePermissionResponse.Status
                ? "Change Permission successful"
                : "Change Permission fail";
            if (!addOrChangePermissionResponse.Status)
            {
                response.SetFail(addOrChangePermissionResponse.ErrorMessages);
                return;
            }

            response.SetSuccess();
        });
    }

    [PermissionController(EPermission.Administrator)]
    [PermissionRequired]
    [HttpPost]
    public async Task<CommonApiResponse<string>> ChangePermissionMany(
        [FromBody] AccountPermissionExceptionAddOrChangeManyRequest request)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            var validator = AccountPermissionExceptionAddOrChangeManyRequestValidator.ValidateRequest(request);
            if (!validator.IsValid)
            {
                response.SetFail(validator.Errors.Select(p => p.ToString()));
                return;
            }

            AccountPermissionExceptionAddOrChangeManyCommand command =
                new AccountPermissionExceptionAddOrChangeManyCommand()
                {
                    AccountIds = request.AccountIds,
                    GroupIds = request.GroupIds,
                    ControllerComponents = request.Controllers?.Select(x =>
                        new AccountPermissionExceptionAddOrChangeManyControllerCodeCommand
                        {
                            ControllerCode = x.ControllerCode,
                            AccessPermission = x.AccessPermission,
                        }).ToArray(),
                };

            var addOrChangePermissionResponse =
                await accountPermissionService.AddOrChangeManyAccountPermissionException(command);

            response.Data = addOrChangePermissionResponse.Status
                ? "Change Permission successful"
                : "Change Permission fail";
            if (!addOrChangePermissionResponse.Status)
            {
                response.SetFail(addOrChangePermissionResponse.ErrorMessages);
                return;
            }

            response.SetSuccess();
        });
    }


    #region private func

    [Obsolete("Obsolete")]
    private string CreateHashPassword(string password, bool isAddMd5 = true)
    {
        if (string.IsNullOrWhiteSpace(password))
            return string.Empty;
        if (isAddMd5)
            password = CreateMd5(password).ToLower();
        byte[] salt = Encoding.UTF8.GetBytes(ContextService.DefaultSaltPassword);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 369);
        byte[] hash = pbkdf2.GetBytes(20); // 160 bit
        return Convert.ToBase64String(hash);
    }

    private static string GeneratePassword(int length = 20)
    {
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[]{};:,.<>?";
        string allChars = lowerChars + upperChars + digits + specialChars;
        int minChars = 8;
        if (length < minChars)
            throw new ArgumentException($"Password length must be at least {minChars} to include all character types.");

        var random = new Random();
        var sb = new StringBuilder();

        sb.Append(lowerChars[random.Next(lowerChars.Length)]);
        sb.Append(upperChars[random.Next(upperChars.Length)]);
        sb.Append(digits[random.Next(digits.Length)]);
        sb.Append(specialChars[random.Next(specialChars.Length)]);

        for (int i = sb.Length; i < length; i++) // random
            sb.Append(allChars[random.Next(allChars.Length)]);
        return new string(sb.ToString().OrderBy(_ => random.Next()).ToArray());
    }

    // for old system account
    private static string CreateMd5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }

    #endregion private func

    #region OldSystemCall

    // [PermissionController(EPermission.Administrator)]
    // [PermissionRequired]
    [AllowAnonymous]
    [HttpPost]
    [Obsolete("Obsolete")] // ??
    public async Task<CommonApiResponse<string>> SyncAccountFromOldSystem(
        [FromBody] AccountSyncFromOldSystemRequest[] requests)
    {
        return await ProcessRequest<string>(async (response) =>
        {
            List<AccountSyncFromOldSystemCommand> commands = [];
            foreach (var request in requests)
            {
                AccountSyncFromOldSystemCommand command = new AccountSyncFromOldSystemCommand
                {
                    AccountId = request.AccountId,
                    HashPassword = CreateHashPassword(request.Md5HashPassword, false),
                    UserName = request.UserName,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = request.CreatedAt,
                    AvatarUrl = request.AvatarUrl,
                    Permission = request.Permission,
                    Email = request.Email,
                    AccountStatus = request.AccountStatus,
                    CountryId = request.CountryId,
                    ProvinceId = request.ProvinceId,
                    DistrictId = request.DistrictId,
                    WardId = request.WardId,
                    AddressLine = request.AddressLine,
                    AddressType = request.AddressType ?? EAddressType.Default,
                    Gender = request.Gender,
                    Occupation = request.Occupation,
                    Marital = request.Marital,
                    Bio = request.Bio,
                    Website = request.Website,
                    SocialLinks = request.SocialLinks,
                    IdentificationNumber = request.IdentificationNumber,
                    PassportNumber = request.PassportNumber,
                    TaxCode = request.TaxCode,
                    CompanyName = request.CompanyName,
                    CompanyAddress = request.CompanyAddress,
                    DateOfBirth = request.DateOfBirth,
                };

                commands.Add(command);
            }

            var addOrChangePermissionResponse =
                await authenticationService.AccountSyncFromOldSystem(commands.ToArray());
            response.Data = addOrChangePermissionResponse.Status ? "Sync successful" : "Sync fail";
            if (!addOrChangePermissionResponse.Status)
            {
                response.SetFail(addOrChangePermissionResponse.ErrorMessages);
                return;
            }

            response.SetSuccess();
        });
    }

    #endregion OldSystemCall
}