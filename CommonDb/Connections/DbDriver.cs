using System.Data.Common;
using System.Threading.Tasks;
using IsolationLevel = System.Data.IsolationLevel;

namespace CommonDb.Connections;

public abstract class DbDriver : IDbDriver, IAsyncDisposable
{
    private bool _disposed = false;
    public abstract string Name { get; set; }
    public abstract string Version { get; set; }
    protected bool IsShutdownImmediate { get; private set; } = false;
    protected bool IsWaitNextTransaction { get; private set; } = true;
    public void SetShutdownImmediate(bool isShutdownImmediate = false)
    {
        IsShutdownImmediate = isShutdownImmediate;
    }
    public void SetWaitNextTransaction(bool isWaitNextTransaction = true)
    {
        IsWaitNextTransaction = isWaitNextTransaction;
    }

    public ConnectOptions Options { get; }
    public abstract Task ConnectAsync(CancellationToken cancellationToken);
    public abstract Task DisconnectAsync();
    public abstract Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
    public abstract DbCommand CreateDbCommand();

    protected DbDriver(ConnectOptions options)
    {
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
}