using CommonDb.DbResults;
using CommonGrpcObject;
using CommonWebApplication.Services;
using DbFactory;
using DbFactory.Generics;
using HandleFlow.ResultConverters;
using IQuestionService;
using QuestionServiceObject.BusinessObjects;
using QuestionServiceObject.QueryObjects;

namespace QuestionService.Services;

public class FaqService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), IFaqService
{
    public async Task<CommonResponse<FaqObject>> GetAll(FaqQuery query)
    {
        return await CommonProcess<FaqObject>(async (response) =>
        {
            string pgQuery = "SELECT * FROM patient limit 1";

            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.ExecuteAsync(pgQuery);
            List<FaqObject>? patientObjs = resultOfQuery?
                .GenericConverter(typeof(FaqObject))?
                .Cast<FaqObject>()
                .ToList();

            if (patientObjs is not { Count: > 0 })
            {
                response.SetFail("Incorrect data type of 'IEnumerable<FaqObject>'");
                return;
            }
            
            FaqObject accountObject = patientObjs.First();
            response.Data = null;
            response.SetSuccess();
        });
    }
}