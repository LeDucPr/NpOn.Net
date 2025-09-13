using System.Data.Common;
using System.Threading.Tasks;
using IsolationLevel = System.Data.IsolationLevel;

namespace CommonDb.Connections;

public interface IDbDriver
{
    string Name { get; }
    string Version { get; }
    public int ValidSessions { get; set; }
    public IConnectOptions Options { get; }

    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
    DbCommand CreateDbCommand();
}

public abstract class DbDriver : IDbDriver, IAsyncDisposable
{
    private bool _disposed = false;
    public abstract string Name { get; set; }
    public abstract string Version { get; set; }
    public abstract int ValidSessions { get; set; }

    public virtual IConnectOptions Options { get; }
    public abstract Task ConnectAsync(CancellationToken cancellationToken);
    public abstract Task DisconnectAsync();

    public abstract Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
        CancellationToken cancellationToken);

    public abstract DbCommand CreateDbCommand();

    protected DbDriver(IConnectOptions options)
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
}