using System.Data;
using System.Data.Common;
using CommonDb.Connections;
using IsolationLevel = System.Data.IsolationLevel;

namespace Cassandra.Connections;

public class CassandraDriver : DbDriver
{
    // DRIVER 
    private Builder? _cassandraBuilder;
    private ICluster? _cluster;
    private ISession? _session;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }
    public override int ValidSessions { get; set; }

    private ISession? NewSession =>
        _cluster?.ConnectAsync(Options.Keyspace).ConfigureAwait(false).GetAwaiter().GetResult();

    public CassandraDriver(CassandraDbConnectOptions options, bool isOpenConnectImmediate = false) : base(options)
    {
        _cassandraBuilder = Cluster.Builder();
        _cluster = _cassandraBuilder.Build();
        Name = _cassandraBuilder.ApplicationName;
        Version = _cassandraBuilder.ApplicationVersion;
        if (isOpenConnectImmediate)
        {
            _session = NewSession;
        }
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (Options.IsWaitNextTransaction)
            if (_session is { IsDisposed: false })
                return; // có session thì chặn kết nối


        if (Options.ContactAddresses is { Length : > 0 })
            _cassandraBuilder = _cassandraBuilder?.AddContactPoints(Options.ContactAddresses);

        _session = NewSession;
    }

    public override async Task DisconnectAsync()
    {
        if (!Options.IsShutdownImmediate)
        {
            if (_session != null)
                await _session.ShutdownAsync().ConfigureAwait(false); // chờ transaction hoàn tất
            if (_cluster != null)
                await _cluster.ShutdownAsync().ConfigureAwait(false);
        }

        _session = null;
        _cluster = null;
        await base.DisposeAsync();
    }

    public override Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        // Ném lỗi rõ ràng để người dùng biết tính năng này không được hỗ trợ.
        throw new NotSupportedException(
            "CassandraCM does not support ACID transactions in the same way as relational databases. Consider using Batches for atomicity or Lightweight Transactions for compare-and-set operations.");
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Lớp tùy chỉnh để ánh xạ khái niệm DbCommand sang CassandraCM's IStatement.
/// </summary>
public class CassandraDbCommand : DbCommand
{
    private readonly ISession _session;

    public CassandraDbCommand(ISession session)
    {
        _session = session;
    }

    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection =>
        throw new NotSupportedException(
            "Use query parameter binding in CQL directly, e.g., 'SELECT * FROM users WHERE id = ?'.");

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
        // CassandraCM driver không hỗ trợ cancel một query đang chạy một cách trực tiếp
    }

    public override int ExecuteNonQuery()
    {
        return ExecuteNonQueryAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        var statement = new SimpleStatement(CommandText);
        RowSet result = await _session.ExecuteAsync(statement).ConfigureAwait(false);
        // ExecuteNonQuery thường trả về số dòng bị ảnh hưởng, nhưng CassandraCM API không cung cấp thông tin này một cách trực tiếp.
        // Trả về 1 nếu thành công, 0 nếu không.
        return result.IsFullyFetched ? 1 : 0;
    }

    public override object? ExecuteScalar()
    {
        return ExecuteScalarAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        var statement = new SimpleStatement(CommandText);
        RowSet result = await _session.ExecuteAsync(statement).ConfigureAwait(false);
        var firstRow = result.FirstOrDefault();
        // Trả về giá trị của cột đầu tiên trong dòng đầu tiên
        return firstRow?[0];
    }

    protected override DbParameter CreateDbParameter()
    {
        throw new NotSupportedException("Parameters are not managed via DbParameter objects.");
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return ExecuteDbDataReaderAsync(behavior, CancellationToken.None).GetAwaiter().GetResult();
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior,
        CancellationToken cancellationToken)
    {
        // Việc triển khai DbDataReader cho CassandraCM khá phức tạp.
        // Để đơn giản, bạn có thể thực thi query và trả về một DataReader đã được đổ dữ liệu.
        // Tuy nhiên, điều này không tận dụng được streaming.
        throw new NotImplementedException(
            "Implementing a full DbDataReader for CassandraCM is complex. Consider using the driver's native RowSet API for querying data.");
    }

    public override void Prepare()
    {
        // Có thể ánh xạ tới PreparedStatement của CassandraCM, nhưng sẽ phức tạp hơn.
        throw new NotImplementedException();
    }
}