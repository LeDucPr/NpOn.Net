using CommonDb.DbResults;
using DbFactory;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;
using SystemController.ResultConverters;


namespace SystemController;

public class InitialConnection
{
    private readonly IDbFactoryWrapper _factory;
    public BaseCtrl InitializationObject { get; private set; }
    public string? FirstInitializationSessionId { get; private set; }

    public InitialConnection(IDbFactoryWrapper factory, BaseCtrl decoyObject)
    {
        _factory = factory;
        InitializationObject = decoyObject;
        GetDataOfFirstConnection().GetAwaiter().GetResult();
    }

    private async Task GetDataOfFirstConnection()
    {
        (string? sessionId, BaseCtrl? ctrl) =
            (await InitializationObject.JoiningData(
                ((ctrl) =>
                {
                    BaseQueryCreatorWithKey queryCreator = new BaseQueryCreatorWithKey(ctrl);
                    return Task.FromResult(queryCreator.CreateQueryWithId());
                }),
                (async (query, type) =>
                {
                    INpOnWrapperResult? result = await _factory.QueryAsync(query);
                    var ctrl = result?.Converter(type);
                    return ctrl?.FirstOrDefault();
                }),
                true, -1));
        if (sessionId != null)
            FirstInitializationSessionId = sessionId;
        if (ctrl != null)
            InitializationObject = ctrl;
    }
}