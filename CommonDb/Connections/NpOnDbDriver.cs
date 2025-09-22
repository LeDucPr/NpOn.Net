using System.Data.Common;
using CommonDb.DbCommands;
using IsolationLevel = System.Data.IsolationLevel;

namespace CommonDb.Connections;

public interface INpOnDbDriver
{
    string Name { get; }
    string Version { get; }
    public bool IsValidSession { get; }
    public INpOnConnectOptions Options { get; }
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task<INpOnDbResult> Query(INpOnDbCommand? command);
    Task<bool> IsAliveAsync(CancellationToken cancellationToken = default);
}

public abstract class NpOnDbDriver : INpOnDbDriver, IAsyncDisposable
{
    private bool _disposed = false;
    public abstract string Name { get; set; }
    public abstract string Version { get; set; }
    public abstract bool IsValidSession { get; }
    public virtual INpOnConnectOptions Options { get; }
    public abstract Task ConnectAsync(CancellationToken cancellationToken);
    public abstract Task DisconnectAsync();

    public virtual Task<INpOnDbResult> Query(INpOnDbCommand? command)
    {
        throw new NotImplementedException("Need to override this method");
    }

    protected NpOnDbDriver(INpOnConnectOptions options)
    {
        if (!options.IsValid())
            return;
        Options = options;
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