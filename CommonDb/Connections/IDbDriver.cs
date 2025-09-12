using System.Data.Common;
using CommonDb.Connections;
using IsolationLevel = System.Data.IsolationLevel;

public interface IDbDriver
{
    string Name { get; }
    string Version { get; }
    void SetShutdownImmediate(bool isShutdownImmediate = false);
    void SetWaitNextTransaction(bool isWaitNextTransaction = true);
    public ConnectOptions Options { get; }

    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken);
    DbCommand CreateDbCommand();
}