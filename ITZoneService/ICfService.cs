using CommonDb.DbResults;
using CommonWebApplication.Objects;

namespace ITZoneService;

public interface ICfService
{
    Task<CommonResponse<INpOnWrapperResult>> TestC();
}