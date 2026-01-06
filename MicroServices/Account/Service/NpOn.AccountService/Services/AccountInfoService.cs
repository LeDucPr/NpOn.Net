using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Infrastructures.NpOn.BaseRepository.Postgres;
using Definitions.NpOn.ProjectEnums.AccountEnums;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Commands;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Domains;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Queries;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.ReadModels;
using MicroServices.Account.Service.NpOn.IAccountService;
using MicroServices.Account.StorageAdapter.NpOn.IAccountStorageAdapter;

namespace MicroServices.Account.Service.NpOn.AccountService.Services;

public class AccountInfoService(
    IPostgresBaseRepository baseRepository,
    IAccountInfoStorageAdapter accountInfoStorageAdapter,
    ILogger<CommonService> logger
) : CommonService(logger), IAccountInfoService
{
    public async Task<CommonResponse<AccountInfoRModel[]?>> AccountInfoGetByAccountIds(
        AccountInfoGetByAccountIdsQuery query)
    {
        return await CommonProcess<AccountInfoRModel[]?>(async (response) =>
        {
            string[]? stringAccountIds = query.AccountIds
                ?.Select(x => x.AsDefaultString()).ToArray();
            if (stringAccountIds == null || !stringAccountIds.Any())
            {
                response.SetSuccess();
                return;
            }

            List<AccountInfoRModel>? existedAccountInfos =
                await accountInfoStorageAdapter.AccountInfoActiveGetByAccountIds(stringAccountIds);
            response.Data = existedAccountInfos?.ToArray();
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse<AccountInfoRModel?>> AccountInfoGetByAccountId(
        AccountInfoGetByAccountIdQuery query)
    {
        return await CommonProcess<AccountInfoRModel?>(async (response) =>
        {
            AccountInfoRModel? accountInfoObj =
                await accountInfoStorageAdapter.AccountInfoActiveGetByAccountId(query.AccountId);
            response.Data = accountInfoObj;
            response.SetSuccess();
        });
    }

    public async Task<CommonResponse> AccountInfoAddOrChange(AccountInfoAddOrChangeCommand command)
    {
        return await CommonProcess(async (response) =>
        {
            AccountInfoRModel? accountInfoObj =
                await accountInfoStorageAdapter.AccountInfoActiveGetByAccountId(command.AccountId);

            if (!(await baseRepository.Add([new AccountInfo(command)]))?.Status ?? false)
            {
                response.SetFail("AccountInfo Add fail");
                return;
            }

            if (accountInfoObj != null)
            {
                AccountInfo accountInfo = new AccountInfo(accountInfoObj);
                accountInfo.ChangeAccountInfoStatus();
                if (!(await baseRepository.Update([accountInfo]))?.Status ?? false)
                {
                    response.SetFail("AccountInfo Add fail");
                    return;
                }
            }

            // address
            if (command.CountryId == null && command.ProvinceId == null && command.DistrictId == null
                && command.WardId == null && command.AddressLine == null)
            {
                response.SetSuccess();
                return;
            }

            string accountId = command.AccountId.AsDefaultString();
            AccountAddressRModel? defaultAddressRModel =
                (await accountInfoStorageAdapter.AccountAddressesDefaultGetByAccountIds([accountId]))?.FirstOrDefault();
            if (!(await baseRepository.Add([new AccountAddress(command)]))?.Status ??
                false) // new Default Account Address
            {
                response.SetFail("AccountAddresses Add fail");
                return;
            }

            if (defaultAddressRModel != null)
            {
                AccountAddress accountAddress = new AccountAddress(defaultAddressRModel);
                accountAddress.ChangeAddressType();
                if (!(await baseRepository.Update([accountAddress]))?.Status ?? false)
                {
                    response.SetFail("Old AccountAddresses Change fail");
                    return;
                }
            }

            response.SetSuccess();
        });
    }

    /// <summary>
    /// An account can only have one DEFAULT address.
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public async Task<CommonResponse> AccountAddressesAddOrChange(AccountAddressAddOrChangeCommand[] commands)
    {
        return await CommonProcess(async (response) =>
        {
            var invalidAccounts = commands.GroupBy(c => c.AccountId)
                .Where(g => g.Count(c => c.AddressType == EAddressType.Default) > 1)
                .Select(g => g.Key).ToList();
            if (invalidAccounts.Any())
            {
                response.SetFail($"Accounts have more than one default address: {string.Join(", ", invalidAccounts)}");
                return;
            }

            string[] existedDefaultAddressesIds = commands.Select(x => x.AccountId.AsDefaultString()).ToArray();
            List<AccountAddressRModel>? addressRModels =
                await accountInfoStorageAdapter.AccountAddressesDefaultGetByAccountIds(existedDefaultAddressesIds);

            List<AccountAddress> accountAddressesAdd = [];
            foreach (var command in commands)
                accountAddressesAdd.Add(new AccountAddress(command));

            if (!(await baseRepository.Add(accountAddressesAdd))?.Status ?? false)
            {
                response.SetFail("AccountAddresses Add fail");
                return;
            }

            if (addressRModels is { Count: > 0 })
            {
                List<AccountAddress> accountAddressesChange = [];
                foreach (var addressRModel in addressRModels)
                {
                    var accountAddress = new AccountAddress(addressRModel);
                    accountAddress.ChangeAddressType();
                    accountAddressesChange.Add(accountAddress);
                }

                if (!(await baseRepository.Update(accountAddressesChange))?.Status ?? false)
                {
                    response.SetFail("AccountAddresses Change fail");
                    return;
                }
            }

            response.SetSuccess();
        });
    }
}