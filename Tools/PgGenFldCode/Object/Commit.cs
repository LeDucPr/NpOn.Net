using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Grpc.Net.Client;
using MicroServices.General.Contract.GeneralServiceContract.Commands;
using MicroServices.General.Service.NpOn.IGeneralService;
using ProtoBuf.Grpc.Client;

namespace Tools.PgGenFldCode.Object;

public class Commit
{
    private readonly string _serviceUrl = "http://localhost:40000";
    private const int _waitTime = 1000;

    public Commit(string? serviceUrl)
    {
        _serviceUrl = serviceUrl ?? _serviceUrl;
    }

    public async Task<(string Message, bool Status)> ExecuteAsync<T>(T data,
        ERepositoryAction action /*= ERepositoryAction.Add*/)
    {
        using var ms = new MemoryStream();
        var payload = ProtoBufMode.ProtoBufSerialize(data);
        var command = new DomainActionCommand
        {
            ActionType = action,
            DomainType = typeof(T),
            Payload = payload
        };
        using var channel = GrpcChannel.ForAddress(_serviceUrl);
        var service = channel.CreateGrpcService<IFldMasterPgService>();

        try
        {
            var response = await service.ExecuteDomainAction(command).WaitAsync(TimeSpan.FromSeconds(_waitTime));
            if (!response.Status)
                return ($"Commit thất bại: {response?.Message ?? "Lỗi không xác định"}", false);
        }
        catch (TimeoutException)
        {
            return ("Kết nối quá thời gian quy định (10s). Vui lòng kiểm tra lại Service.", false);
        }

        return ("commit Success", true);
    }
}