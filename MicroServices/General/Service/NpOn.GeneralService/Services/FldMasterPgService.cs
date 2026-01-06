using System.Collections;
using Common.Extensions.NpOn.CommonBaseDomain;
using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonGrpcContract;
using Common.Extensions.NpOn.CommonMode;
using Common.Extensions.NpOn.CommonWebApplication.Services;
using Common.Extensions.NpOn.HandleFlow;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;
using Common.Infrastructures.NpOn.CommonDb.DbResults.Grpc;
using Common.Infrastructures.NpOn.DbFactory.Generics;
using Definitions.NpOn.ProjectConstant.GeneralConstant;
using MicroServices.General.Contract.GeneralServiceContract.Commands;
using MicroServices.General.Contract.GeneralServiceContract.Queries;
using MicroServices.General.Contract.GeneralServiceContract.ReadModels;
using MicroServices.General.Service.NpOn.IGeneralService;
using Npgsql;
using NpgsqlTypes;

namespace MicroServices.General.Service.NpOn.GeneralService.Services
{
    public class FldMasterPgService(
        IDbFactoryWrapper dbFactoryWrapper,
        ILogger<CommonService> logger
    ) : CommonService(logger), IFldMasterPgService
    {
        public async Task<CommonResponse> ExecuteDomainAction(DomainActionCommand command)
        {
            return await CommonProcess(async response =>
            {
                byte[]? body = command.Payload;
                if (body == null)
                {
                    response.SetFail("Payload not null.");
                    return;
                }

                Type domainType = command.DomainType;
                var domains = ProtoBufMode.ProtoBufDeserialize(body, domainType);
                List<BaseDomain> domainList;
                if (domains is IEnumerable enumerable)
                {
                    domainList = enumerable.Cast<BaseDomain>().ToList();
                }
                else if (domains is BaseDomain single)
                {
                    domainList = new List<BaseDomain> { single };
                }
                else
                {
                    response.SetFail("ProtoBufDeserialize Payload error.");
                    return;
                }

                (string commandText, List<NpgsqlParameter> npgsqlParameters) = command.ActionType switch
                {
                    ERepositoryAction.Add => domainList.ToPostgresParamsInsert(),
                    ERepositoryAction.Update => domainList.ToPostgresParamsUpdate(),
                    ERepositoryAction.Delete => domainList.ToPostgresParamsDelete(),
                    ERepositoryAction.Merge => domainList.ToPostgresParamsMerge(),
                    _ => throw new ArgumentOutOfRangeException()
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

                INpOnDbCommand dbCommand =
                    new NpOnDbCommand(dbFactoryWrapper.DbType, commandText, parameters);
                INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.ExecuteAsync(dbCommand);
                if (wrapperResult == null || !wrapperResult.Status)
                {
                    response.SetFail($"Failed to {command.ActionType} domains.");
                    return;
                }

                response.SetSuccess();
            });
        }

        public async Task<CommonResponse<List<TblFldRModel>>> GetExecution(TblFldExecution execution)
        {
            return await CommonProcess<List<TblFldRModel>>(async (response) =>
            {
                if (execution.ExecFunc == null && execution.Code == null && execution.TblMaterId == null)
                {
                    response.SetFail("Invalid query");
                    return;
                }

                List<NpOnDbCommandParam> parameters = new List<NpOnDbCommandParam>();
                var queryBuilder = new TblFldMasterQueryBuilder();
                if (execution.Code != null)
                {
                    queryBuilder = queryBuilder.WhereCode(execution.Code);
                    parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                    {
                        ParamName = nameof(execution.Code),
                        ParamValue = execution.Code,
                        ParamType = NpgsqlDbType.Varchar
                    });
                }
                else if (execution.TblMaterId != null)
                {
                    queryBuilder = queryBuilder.WhereTblMasterId(execution.TblMaterId);
                    parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                    {
                        ParamName = nameof(execution.TblMaterId),
                        ParamValue = execution.Code,
                        ParamType = NpgsqlDbType.Uuid
                    });
                }
                else if (execution.ExecFunc != null)
                {
                    queryBuilder = queryBuilder.WhereExecFunc(execution.ExecFunc);
                    parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                    {
                        ParamName = nameof(execution.ExecFunc),
                        ParamValue = execution.Code,
                        ParamType = NpgsqlDbType.Varchar
                    });
                }

                var (queryBuilderString, _) = queryBuilder.Build();

                INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.ExecuteAsync(queryBuilderString, parameters);
                if (wrapperResult == null)
                {
                    response.SetFail("FldMaster not found");
                    return;
                }

                List<TblFldRModel>? tblFldObjects = wrapperResult
                    .GenericConverter(typeof(TblFldRModel))?
                    .Cast<TblFldRModel>()
                    .ToList();

                if (tblFldObjects is not { Count: > 0 })
                {
                    response.SetFail("FldMasterObject not found");
                    return;
                }

                response.Data = tblFldObjects;
                response.SetSuccess();
            });
        }

        public async Task<CommonResponse<CommandRModel?>> GetExecCommand(TblFldExecution execution)
        {
            return await CommonProcess<CommandRModel?>(async (response) =>
            {
                List<TblFldRModel>? tblFldObjects = (await GetExecution(execution)).Data;
                if (tblFldObjects is not { Count: > 0 })
                {
                    response.SetFail("FldMasterObject not found");
                    return;
                }

                TblFldRModel tblFldRModelFirst = tblFldObjects.First();
                if (string.IsNullOrWhiteSpace(tblFldRModelFirst.Query) &&
                    string.IsNullOrWhiteSpace(tblFldRModelFirst.ExecFunc))
                {
                    response.SetFail("Query null");
                    return;
                }

                if (tblFldRModelFirst.ExecType == null)
                {
                    response.SetFail("ExecType null");
                    return;
                }

                if (tblFldRModelFirst.DataBaseType == null)
                {
                    response.SetFail("DataBaseType null");
                    return;
                }

                EDb databaseType = (EDb)tblFldRModelFirst.DataBaseType;
                Type? deserializeParamType = null;
                if (databaseType == EDb.Postgres)
                {
                    deserializeParamType = typeof(NpOnDbCommandParamGrpc<NpgsqlDbType>);
                }
                // else {} // other db type 

                List<NpOnDbCommandParamGrpc> parameters = [];
                foreach (var paramObj in tblFldObjects)
                {
                    if (string.IsNullOrEmpty(paramObj.FieldName))
                        break;
                    string? stringValue =
                        execution.ExecParams?.First(x => x.ParamName == paramObj.FieldName).StringValue;
                    NpOnDbCommandParamGrpc? commandParam = null;
                    if (databaseType == EDb.Postgres)
                        commandParam = new NpOnDbCommandParamGrpc<NpgsqlDbType>
                        {
                            ParamName = paramObj.FieldName,
                            ParamValue = stringValue.AsDefaultString(),
                            ParamType = paramObj.FieldType ?? paramObj.FieldDbType ?? NpgsqlDbType.Unknown,
                        };
                    // else {} // other db type

                    if (commandParam != null)
                        parameters.Add(commandParam);
                }

                EExecType execType = (EExecType)tblFldRModelFirst.ExecType;
                string commandText =
                    (execType == EExecType.Query ? tblFldRModelFirst.Query : tblFldRModelFirst.ExecFunc)!;
                var paramsList = new NpOnDbCommandParamGrpcList { Items = parameters };
                byte[] payload = ProtoBufMode.ProtoBufSerialize(paramsList);
                CommandRModel commandModel = new CommandRModel
                {
                    CommandText = commandText,
                    // Parameters = parameters.ToArray(),
                    ParamsPayload = payload,
                    ExecType = execType,
                    DatabaseType = databaseType,
                    DeserializeParamType = deserializeParamType ?? typeof(NpOnDbCommandParam),
                };
                response.Data = commandModel;
                response.SetSuccess();
            });
        }

        public async Task<CommonResponse<INpOnGrpcObject>> Execute(TblFldExecution execution)
        {
            return await CommonProcess<INpOnGrpcObject>(async (response) =>
            {
                List<TblFldRModel>? tblFldObjects = (await GetExecution(execution)).Data;
                if (tblFldObjects is not { Count: > 0 })
                {
                    response.SetFail("FldMasterObject not found");
                    return;
                }

                TblFldRModel tblFldRModelFirst = tblFldObjects.First();
                INpOnWrapperResult? wrapperResult = null;
                if (tblFldRModelFirst is { ExecFunc: not null, ExecType: EExecType.ExecFunc })
                {
                    string funcName = tblFldRModelFirst.ExecFunc;
                    List<INpOnDbCommandParam<NpgsqlDbType>> parameters = [];

                    foreach (var paramObj in tblFldObjects)
                    {
                        if (string.IsNullOrEmpty(paramObj.FieldName))
                            break;
                        string? stringValue =
                            execution.ExecParams?.First(x => x.ParamName == paramObj.FieldName).StringValue;
                        NpOnDbCommandParam<NpgsqlDbType> commandParam = new NpOnDbCommandParam<NpgsqlDbType>
                        {
                            ParamName = paramObj.FieldName,
                            ParamValue = stringValue.AsDefaultString(),
                            ParamType = paramObj.FieldDbType ?? NpgsqlDbType.Unknown,
                        };
                        parameters.Add(commandParam);
                    }

                    try
                    {
                        wrapperResult = await dbFactoryWrapper.ExecuteFuncParams(
                            funcName, parameters);
                    }
                    catch (Exception)
                    {
                        response.SetFail("Execute Error!");
                        return;
                    }
                }
                else if (tblFldRModelFirst is { Query: not null, ExecType: EExecType.Query })
                {
                    string execString = tblFldRModelFirst.Query;
                    List<NpOnDbCommandParam> parameters = new List<NpOnDbCommandParam>();
                    foreach (var paramObj in tblFldObjects)
                    {
                        if (string.IsNullOrEmpty(paramObj.FieldName))
                            break;
                        string? stringValue =
                            execution.ExecParams?.First(x => x.ParamName == paramObj.FieldName).StringValue;
                        NpOnDbCommandParam<NpgsqlDbType> commandParam = new NpOnDbCommandParam<NpgsqlDbType>
                        {
                            ParamName = paramObj.FieldName,
                            ParamValue = stringValue.AsDefaultString(),
                            ParamType = paramObj.FieldType ?? paramObj.FieldDbType ?? NpgsqlDbType.Unknown,
                        };
                        parameters.Add(commandParam);
                    }

                    try
                    {
                        if (parameters is { Count: > 0 })
                            wrapperResult = await dbFactoryWrapper.ExecuteAsync(execString, parameters);
                        else
                            wrapperResult = await dbFactoryWrapper.ExecuteAsync(execString);
                    }
                    catch (Exception)
                    {
                        response.SetFail("Query Error!");
                        return;
                    }
                }

                if (wrapperResult == null)
                {
                    response.SetFail("FldMaster not found");
                    return;
                }

                if (wrapperResult is not INpOnTableWrapper tableWrapperResult)
                {
                    response.SetFail("ValueFormat not found");
                    return;
                }

                if (tableWrapperResult.RowWrappers.Count == 0)
                    response.Data = null;
                else
                {
                    INpOnGrpcObject grpcObject = tableWrapperResult.ToGrpcTable();
                    response.Data = grpcObject;
                }

                response.SetSuccess();
            });
        }
    }
}