using CommonDb.DbResults.Grpc;
using CommonGrpcObject;
using CommonWebApplication.Services;
using GeneralServiceObject.QueryObjects;
using IGeneralService;
using IQuestionService;
using ProjectEntry.QuestionEntries;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class SurveyService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), ISurveyService
{
    public async Task<CommonResponse<INpOnGrpcObject>> GetQuestionsBySurveyId(SurveyGetAllQuery query)
    {
        return await CommonProcess<INpOnGrpcObject>(async (response) =>
        {
            var surveyGetBy = await fldMasterPgService.Query(new TblFldQuery()
            {
                Code = QuestionServiceQueryCode.QuestionsBySurveyId,
                QueryParams =
                [
                    new TblFldQueryParam()
                    {
                        ParamName = "survey_id",
                        StringValue = query.SurveyIdAsString
                    }
                ],
            });

            INpOnGrpcObject? questionGrpTable = surveyGetBy.Data;
            if (!surveyGetBy.Status)
            {
                response.SetFail(surveyGetBy.ErrorMessages);
                return;
            }

            response.Data = questionGrpTable;
            response.SetSuccess();
        });
    }
}