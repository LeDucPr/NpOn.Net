using CommonDb.DbResults;
using CommonWebApplication.Objects;
using CommonWebApplication.Services;
using DbFactory;
using ITZoneService;

namespace TZoneService.Services;

public class CfService(
    IDbFactoryWrapper dbFactoryWrapper,
    ILogger<CommonService> logger
) : CommonService(logger), ICfService
{
    public async Task<CommonResponse<INpOnWrapperResult>> TestC()
    {
        return await CommonProcess<INpOnWrapperResult>(async (response) =>
        {
            // Gán dữ liệu mặc định
           
            // Thực hiện query
            string msQuery = "Select * from Users where id = 'C000175'";
            INpOnWrapperResult? resultOfQuery = await dbFactoryWrapper.QueryAsync(msQuery);

            // Nếu muốn, bạn có thể xử lý kết quả query ở đây
            if (resultOfQuery != null)
            {
                response.Data = resultOfQuery;
            }
        });
    }
}
