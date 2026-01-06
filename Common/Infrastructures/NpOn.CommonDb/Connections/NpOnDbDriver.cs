using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Common.Infrastructures.NpOn.CommonDb.DbCommands;
using Common.Infrastructures.NpOn.CommonDb.DbResults;

namespace Common.Infrastructures.NpOn.CommonDb.Connections;

public interface INpOnDbDriver
{
    string Name { get; }
    string Version { get; }
    public bool IsValidSession { get; }
    public INpOnConnectOption Option { get; }
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task<INpOnWrapperResult> Execute(INpOnDbCommand? command);
    Task<INpOnWrapperResult> ExecuteFunc(INpOnDbExecCommand? execCommand);

    Task<INpOnWrapperResult> ExecuteFuncParams<TEnum>(INpOnDbExecCommand? execCommand,
        List<INpOnDbCommandParam<TEnum>> parameters) where TEnum : Enum;
    Task<bool> IsAliveAsync(CancellationToken cancellationToken = default);
}

public abstract class NpOnDbDriver : INpOnDbDriver, IAsyncDisposable
{
    private bool _disposed = false;
    public abstract string Name { get; set; }
    public abstract string Version { get; set; }
    public abstract bool IsValidSession { get; }
    public virtual INpOnConnectOption Option { get; }
    public abstract Task ConnectAsync(CancellationToken cancellationToken);
    public abstract Task DisconnectAsync();

    public virtual Task<INpOnWrapperResult> Execute(INpOnDbCommand? command)
    {
        throw new NotImplementedException("Need to override this method");
    }

    public virtual Task<INpOnWrapperResult> ExecuteFunc(INpOnDbExecCommand? execCommand)
    {
        throw new NotImplementedException("Need to override this method");
    }

    public virtual Task<INpOnWrapperResult> ExecuteFuncParams<TEnum>(INpOnDbExecCommand? execCommand,
        List<INpOnDbCommandParam<TEnum>> parameters) where TEnum : Enum
    {
        throw new NotImplementedException("Need to override this method");
    }

    protected NpOnDbDriver(INpOnConnectOption option)
    {
        if (!option.IsValid() && !option.IsValidRequireFromBase(EConnectLink.SelfValidateConnection.GetDisplayName()))
            return;
        Option = option;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync();
        _disposed = true;
    }

    public virtual Task<bool> IsAliveAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsValidSession && !_disposed);
}