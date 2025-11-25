using CommonGrpcObject;
using CommonWebApplication.Services;
using IGeneralService;
using IQuestionService;
using QuestionServiceObject.CommandObjects;

namespace QuestionService.Services;

public class QuestionAndAnswerService(
    IFldMasterPgService fldMasterPgService,
    ILogger<CommonService> logger
) : CommonService(logger), IQuestionAndAnswerService
{
    public async Task<CommonResponse<string>> AddOrUpdateQuestionAndAnswer(QuestionAndAnswerAddOrUpdateCommand addOrUpdateCommand)
    {
        return await CommonProcess<string>(async (response) =>
        {
            // Original empty implementation
        });
    }
}