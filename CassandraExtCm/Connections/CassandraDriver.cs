using Cassandra;
using Cassandra.Mapping;
using CassandraExtCm.Cql;
using CassandraExtCm.Results;
using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CassandraExtCm.Connections;

public class CassandraDriver : NpOnDbDriver
{
    // DRIVER 
    private ICluster? _cluster;
    private ISession? _session;
    private IMapper? _mapper;
    public sealed override string Name { get; set; }
    public sealed override string Version { get; set; }

    public override bool IsValidSession => _session is { IsDisposed: false };

    public CassandraDriver(CassandraConnectOption option) : base(option)
    {
    }

    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsValidSession)
        {
            if (Option.IsWaitNextTransaction)
            {
                return; // Đã có session hợp lệ và option yêu cầu chờ.
            }

            await DisconnectAsync().ConfigureAwait(false);
        }

        if (_cluster == null)
        {
            var cassandraBuilder = Cluster.Builder();
            if (Option.ContactAddresses is { Length: > 0 })
            {
                cassandraBuilder.AddContactPoints(Option.ContactAddresses);
            }

            _cluster = cassandraBuilder.Build();
            Name = cassandraBuilder.ApplicationName;
            Version = cassandraBuilder.ApplicationVersion;
        }
        
        _session = await _cluster.ConnectAsync(Option.Keyspace).ConfigureAwait(false);
        _mapper = new Mapper(_session);
    }
    
    public override async Task DisconnectAsync()
    {
        if (!Option.IsShutdownImmediate)
        {
            if (_session != null)
                await _session.ShutdownAsync().ConfigureAwait(false); // chờ transaction hoàn tất
            if (_cluster != null)
                await _cluster.ShutdownAsync().ConfigureAwait(false);
        }
        else
        {
            _session?.Dispose();
            _cluster?.Dispose();
        }

        _session = null;
        _cluster = null;
        _mapper = null;
    }

    public override async Task<INpOnWrapperResult> Query(INpOnDbCommand? command)
    {
        // 1. Guard Clauses: Kiểm tra trạng thái hợp lệ và đầu vào
        if (!IsValidSession || _session == null)
            return new CassandraResultSetWrapper().SetFail(EDbError.Session);
        if (command == null)
            return new CassandraResultSetWrapper().SetFail(EDbError.Command);
        if (string.IsNullOrWhiteSpace(command.CommandText))
            return new CassandraResultSetWrapper().SetFail(EDbError.CommandText);
        try
        {
            var statement = new SimpleStatement(command.CommandText);
            RowSet rowSet = await _session.ExecuteAsync(statement)
                .ConfigureAwait(false);
            return new CassandraResultSetWrapper(rowSet);
        }
        catch (Exception)
        {
            return new CassandraResultSetWrapper().SetFail(EDbError.CommandTextSyntax);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}