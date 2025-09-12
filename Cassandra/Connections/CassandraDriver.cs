using System.Data;
using System.Data.Common;
using Cassandra;
using CommonDb.Connections; 
using IsolationLevel = System.Data.IsolationLevel;

public class CassandraDriver : DbDriver
{
    private readonly string[] _contactPoints;
    private readonly string _keyspace;
    private ICluster? _cluster;
    private ISession? _session;

    public override string Name { get; set; }
    public override string Version { get; set; } //> _cluster?.Metadata.ClusterName ?? "Unknown";

    public CassandraDriver(ConnectOptions options) : base(options)
    {
        if (string.IsNullOrWhiteSpace(options.KeySpace))
            throw new ArgumentNullException(nameof(options.KeySpace));
        _keyspace = options.KeySpace;
        _contactPoints = options.ContactAddresses ?? [];
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsWaitNextTransaction)
            if (_session is { IsDisposed: false })
                return; // có session thì chặn kết nối

        Builder cassandraBuilder = Cluster.Builder();
        if (_contactPoints is { Length : > 0 })
            cassandraBuilder = cassandraBuilder.AddContactPoints(_contactPoints);
        _cluster = cassandraBuilder.Build();
        _session = await _cluster.ConnectAsync(_keyspace).ConfigureAwait(false);
    }

    public override async Task DisconnectAsync()
    {
        if (!IsShutdownImmediate)
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
            "Cassandra does not support ACID transactions in the same way as relational databases. Consider using Batches for atomicity or Lightweight Transactions for compare-and-set operations.");
    }

    /// <summary>
    /// Tạo một đối tượng command tùy chỉnh để bọc các câu lệnh CQL.
    /// </summary>
    public override DbCommand CreateDbCommand()
    {
        return new CassandraDbCommand(_session);
    }
    
    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Lớp tùy chỉnh để ánh xạ khái niệm DbCommand sang Cassandra's IStatement.
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
        // Cassandra driver không hỗ trợ cancel một query đang chạy một cách trực tiếp
    }

    public override int ExecuteNonQuery()
    {
        return ExecuteNonQueryAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        var statement = new SimpleStatement(CommandText);
        RowSet result = await _session.ExecuteAsync(statement).ConfigureAwait(false);
        // ExecuteNonQuery thường trả về số dòng bị ảnh hưởng, nhưng Cassandra API không cung cấp thông tin này một cách trực tiếp.
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
        // Việc triển khai DbDataReader cho Cassandra khá phức tạp.
        // Để đơn giản, bạn có thể thực thi query và trả về một DataReader đã được đổ dữ liệu.
        // Tuy nhiên, điều này không tận dụng được streaming.
        throw new NotImplementedException(
            "Implementing a full DbDataReader for Cassandra is complex. Consider using the driver's native RowSet API for querying data.");
    }

    public override void Prepare()
    {
        // Có thể ánh xạ tới PreparedStatement của Cassandra, nhưng sẽ phức tạp hơn.
        throw new NotImplementedException();
    }
}