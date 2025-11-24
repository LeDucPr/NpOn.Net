using CommonDb.DbCommands;
using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonObject;
using CommonWebApplication.Services;
using DbFactory;
using GeneralServiceObject.BusinessObjects;
using GeneralServiceObject.QueryObjects;
using HandleFlow.ResultConverters;
using IGeneralService;
using NpgsqlTypes;
using ProjectEntry;
using ProjectEntry.GeneralEntries;
using ProjectEnums.GeneralEnums;

namespace GeneralService.Services;

public class FldMasterPgService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IFldMasterPgService
{
    public async Task<CommonResponse<List<TblFldObject>>> GetQuery(TblFldQuery query)
    {
        return await CommonProcess<List<TblFldObject>>(async (response) =>
        {
            if (query.ExecFunc == null && query.Code == null && query.TblMaterId == null)
            {
                response.SetFail("Invalid query");
                return;
            }

            List<NpOnDbCommandParam> parameters = new List<NpOnDbCommandParam>();
            var queryBuilder = new TblFldMasterQueryBuilder();
            if (query.Code != null)
            {
                queryBuilder = queryBuilder.WhereCode(query.Code);
                parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                {
                    ParamName = nameof(query.Code),
                    ParamValue = query.Code,
                    ParamType = NpgsqlDbType.Varchar
                });
            }
            else if (query.TblMaterId != null)
            {
                queryBuilder = queryBuilder.WhereTblMasterId(query.TblMaterId);
                parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                {
                    ParamName = nameof(query.TblMaterId),
                    ParamValue = query.Code,
                    ParamType = NpgsqlDbType.Uuid
                });
            }
            else if (query.ExecFunc != null)
            {
                queryBuilder = queryBuilder.WhereExecFunc(query.ExecFunc);
                parameters.Add(new NpOnDbCommandParam<NpgsqlDbType>
                {
                    ParamName = nameof(query.ExecFunc),
                    ParamValue = query.Code,
                    ParamType = NpgsqlDbType.Varchar
                });
            }
            var (queryBuilderString, _) = queryBuilder.Build();

            INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.QueryAsync(queryBuilderString, parameters);
            if (wrapperResult == null)
            {
                response.SetFail("FldMaster not found");
                return;
            }

            List<TblFldObject>? tblFldObjects = wrapperResult
                .GenericConverter(typeof(TblFldObject))?
                .Cast<TblFldObject>()
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

    public async Task<CommonResponse<INpOnGrpcObject>> Query(TblFldQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            List<TblFldObject>? tblFldObjects = (await GetQuery(query)).Data;
            if (tblFldObjects is not { Count: > 0 })
            {
                response.SetFail("FldMasterObject not found");
                return;
            }

            TblFldObject tblFldObjectFirst = tblFldObjects.First();
            INpOnWrapperResult? wrapperResult = null;
            if (tblFldObjectFirst is { ExecFunc: not null, ExecType: EExecType.ExecFunc })
            {
                string funcName = tblFldObjectFirst.ExecFunc;
                List<INpOnDbCommandParam<NpgsqlDbType>> parameters = [];
                
                foreach (var paramObj in tblFldObjects)
                {
                    if (string.IsNullOrEmpty(paramObj.FieldName))
                        break;
                    string? stringValue = query.QueryParams?.First(x => x.ParamName == paramObj.FieldName).StringValue;
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
            else if (tblFldObjectFirst is { Query: not null, ExecType: EExecType.Query })
            {
                string queryString = tblFldObjectFirst.Query;
                List<NpOnDbCommandParam> parameters = new List<NpOnDbCommandParam>();
                foreach (var paramObj in tblFldObjects)
                {
                    if (string.IsNullOrEmpty(paramObj.FieldName))
                        break;
                    string? stringValue = query.QueryParams?.First(x => x.ParamName == paramObj.FieldName).StringValue;
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
                        wrapperResult = await dbFactoryWrapper.QueryAsync(queryString, parameters);
                    else
                        wrapperResult = await dbFactoryWrapper.QueryAsync(queryString);
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

            INpOnGrpcObject grpcObject = tableWrapperResult.ToGrpcTable();
            response.Data = grpcObject;
            response.SetSuccess();
        });
    }
}