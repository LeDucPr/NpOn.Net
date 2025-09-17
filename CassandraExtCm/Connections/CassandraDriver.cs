using Cassandra;
using Cassandra.Mapping;
using CassandraExtCm.Cql;
using CommonDb.Connections;
using CommonDb.DbCommands;

namespace CassandraExtCm.Connections;

public class CassandraDriver : NpOnNpOnDbDriver
{
    // DRIVER 
    private ICluster? _cluster;
    private ISession? _session;
    private IMapper? _mapper;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }

    public override bool ValidSessions => _session is { IsDisposed: false };

    private ISession? NewSession =>
        _cluster?.ConnectAsync(Options.Keyspace).ConfigureAwait(false).GetAwaiter().GetResult();

    public CassandraDriver(CassandraNpOnConnectOptions options) : base(options)
    {
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_session is { IsDisposed: false })
        {
            if (Options.IsWaitNextTransaction)
            {
                return; // Đã có session hợp lệ và option yêu cầu chờ.
            }
            await DisconnectAsync();
        }
        var cassandraBuilder = Cluster.Builder();
        if (Options.ContactAddresses is { Length: > 0 })
        {
            cassandraBuilder.AddContactPoints(Options.ContactAddresses);
        }

        _cluster = cassandraBuilder.Build();
        _session = await _cluster.ConnectAsync(Options.Keyspace).ConfigureAwait(false);
        _mapper = new Mapper(_session);
        Name = cassandraBuilder.ApplicationName;
        Version = cassandraBuilder.ApplicationVersion;
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
        _mapper = null;
        await base.DisposeAsync();
    }

    public override async Task<INpOnDbResult> Query(INpOnDbCommand? command)
    {
        if (_mapper == null || command == null || string.IsNullOrWhiteSpace(command.CommandText))
        {
            return new CassandraResult();
        }

        try
        {
            IEnumerable<RowSet> results = await _mapper.FetchAsync<RowSet>(command.CommandText).ConfigureAwait(false);
            int count = results.Count();
            return new CassandraResult(results.FirstOrDefault());
        }
        catch (Exception ex)
        {
            return new CassandraResult().SetFail();
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}