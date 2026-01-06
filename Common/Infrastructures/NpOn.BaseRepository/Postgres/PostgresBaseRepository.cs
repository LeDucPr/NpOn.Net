using Common.Extensions.NpOn.CommonBaseDomain;
using Common.Extensions.NpOn.CommonEnums;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Npgsql;
using NpgsqlTypes;

namespace Common.Infrastructures.NpOn.BaseRepository.Postgres;

public class PostgresBaseRepository(IPostgresFactoryWrapper postgresFactoryWrapper) : IPostgresBaseRepository
{
    private readonly EDb _dataBaseType = EDb.Postgres;

    public async Task<INpOnWrapperResult?>Add<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Add, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }
    public async Task<INpOnWrapperResult?>Update<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Update, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }
    public async Task<INpOnWrapperResult?>Merge<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Merge, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }
    public async Task<INpOnWrapperResult?>Delete<T>(List<T> domains, bool isUseDefaultWhenNull = false) where T : BaseDomain
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Delete);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }
    
    public async Task<INpOnWrapperResult?> Add(List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Add, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }

    public async Task<INpOnWrapperResult?> Update(List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Update, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }

    public async Task<INpOnWrapperResult?> Merge(List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Merge, isUseDefaultWhenNull);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }

    public async Task<INpOnWrapperResult?> Delete(List<BaseDomain> domains, bool isUseDefaultWhenNull = false)
    {
        (string commandText, List<NpOnDbCommandParam> npgsqlParameters) = GetParams(domains, ERepositoryAction.Delete);

        INpOnDbCommand dbCommand =
            new NpOnDbCommand(_dataBaseType, commandText, npgsqlParameters);
        INpOnWrapperResult? wrapperResult = await postgresFactoryWrapper.ExecuteAsync(dbCommand);
        return wrapperResult;
    }

    public async Task<INpOnWrapperResult?> Execute(RepositoryCommand repositoryCommand)
    {
        if (repositoryCommand.ExecType == EExecType.ExecFunc)
        {
            var typedParameters = repositoryCommand.Parameters?
                .OfType<INpOnDbCommandParam<NpgsqlDbType>>()
                .ToList();
            return await postgresFactoryWrapper.ExecuteFuncParams(
                repositoryCommand.CommandText, typedParameters);
        }

        if (repositoryCommand.Parameters is { Length: > 0 })
            return await postgresFactoryWrapper.ExecuteAsync(repositoryCommand.CommandText, repositoryCommand.Parameters.ToList());
        return await postgresFactoryWrapper.ExecuteAsync(repositoryCommand.CommandText);
    }

    private (string commandText, List<NpOnDbCommandParam> npgsqlParameters) GetParams(
        List<BaseDomain> domains, ERepositoryAction actionType, bool isUseDefaultWhenNull = false)
    {
        (string commandText, List<NpgsqlParameter> npgsqlParameters) = actionType switch
        {
            ERepositoryAction.Add => domains.ToPostgresParamsInsert(isUseDefaultWhenNull),
            ERepositoryAction.Update => domains.ToPostgresParamsUpdate(isUseDefaultWhenNull),
            ERepositoryAction.Delete => domains.ToPostgresParamsDelete(isUseDefaultWhenNull),
            ERepositoryAction.Merge => domains.ToPostgresParamsMerge(isUseDefaultWhenNull),
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };

        List<NpOnDbCommandParam> parameters = npgsqlParameters
            .Select(p => new NpOnDbCommandParam<NpgsqlDbType>
            {
                ParamName = p.ParameterName,
                ParamValue = p.Value ?? DBNull.Value,
                ParamType = p.NpgsqlDbType
            })
            .Cast<NpOnDbCommandParam>()
            .ToList();
        return (commandText, parameters);
    }
    
    private (string commandText, List<NpOnDbCommandParam> npgsqlParameters) GetParams<T>(
        List<T> domains, ERepositoryAction actionType, bool isUseDefaultWhenNull = false) where T : BaseDomain 
    {
        (string commandText, List<NpgsqlParameter> npgsqlParameters) = actionType switch
        {
            ERepositoryAction.Add => domains.Cast<BaseDomain>().ToList().ToPostgresParamsInsert(isUseDefaultWhenNull),
            ERepositoryAction.Update => domains.Cast<BaseDomain>().ToList().ToPostgresParamsUpdate(isUseDefaultWhenNull),
            ERepositoryAction.Delete => domains.Cast<BaseDomain>().ToList().ToPostgresParamsDelete(isUseDefaultWhenNull),
            ERepositoryAction.Merge => domains.Cast<BaseDomain>().ToList().ToPostgresParamsMerge(isUseDefaultWhenNull),
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };

        List<NpOnDbCommandParam> parameters = npgsqlParameters
            .Select(p => new NpOnDbCommandParam<NpgsqlDbType>
            {
                ParamName = p.ParameterName,
                ParamValue = p.Value ?? DBNull.Value,
                ParamType = p.NpgsqlDbType
            })
            .Cast<NpOnDbCommandParam>()
            .ToList();
        return (commandText, parameters);
    }
}