using CommonDb.DbResults;
using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using GeneralServiceObject.BusinessObjects;
using GeneralServiceObject.QueryObjects;
using HandleFlow.ResultConverters;
using IGeneralService;
using ProjectEntry;

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

            var queryBuilder = new TblFldMasterQueryBuilder();
            if (query.Code != null)
                queryBuilder = queryBuilder.WhereCode(query.Code);
            else if (query.TblMaterId != null)
                queryBuilder = queryBuilder.WhereTblMasterId(query.TblMaterId);
            else if (query.ExecFunc != null)
                queryBuilder = queryBuilder.WhereExecFunc(query.ExecFunc);
            var (queryBuilderString, _) = queryBuilder.Build();

            INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.QueryAsync(queryBuilderString);
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

    // public async Task<CommonResponse<INpOnGrpcObject>> Query(TblFldQuery query)
    // {
    //     return await CommonProcess<INpOnGrpcObject>(async (response) =>
    //     {
    //         List<TblFldObject>? tblFldObjects = (await GetQuery(query)).Data;
    //         if (tblFldObjects is not { Count: > 0 })
    //         {
    //             response.SetFail("FldMasterObject not found");
    //             return;
    //         }
    //
    //         TblFldObject tblFldObjectFirst = tblFldObjects.First();
    //         INpOnWrapperResult? resultOfQuery;
    //         if (tblFldObjectFirst.ExecFunc != null)
    //         {
    //             string funcName = tblFldObjectFirst.ExecFunc;
    //             Dictionary<string, object> parameters = new Dictionary<string, object>();
    //             foreach (var param in tblFldObjects)
    //             {
    //                 parameters.Add(param);
    //             }
    //             
    //             resultOfQuery = await dbFactoryWrapper.ExecuteFunc(
    //                 funcName,
    //                 new Dictionary<string, object>
    //                 {
    //                     [""] =
    //                         @"{
    //                   ""full_name"": """",
    //                   ""username"": """",
    //                   ""from_date"": ""2025-11-07T00:00:00"",
    //                   ""to_date"": ""2025-11-14T23:59:59"",
    //                   ""mobile_phone"": """",
    //                   ""gender"": """",
    //                   ""province_rcd"": """",
    //                   ""district_rcd"": """",
    //                   ""commune_rcd"": """",
    //                   ""standard_account_id"": ""12fbd6a7-978b-4e7f-98bc-43c21684b371"",
    //                   ""master_account_id"": null,
    //                   ""province_account_rcd"": """",
    //                   ""rank_type"": null,
    //                   ""page"": 1,
    //                   ""pageSize"": 1
    //                 }"
    //                 }, true, isUseOutputJsonAsName: funcName
    //             
    //             
    //             response.SetFail("Invalid query");
    //             return;
    //         }
    //
    //         INpOnWrapperResult? wrapperResult = await dbFactoryWrapper.QueryAsync(queryBuilderString);
    //         if (wrapperResult == null)
    //         {
    //             response.SetFail("FldMaster not found");
    //             return;
    //         }
    //
    //         response.SetSuccess();
    //     });
    // }
}