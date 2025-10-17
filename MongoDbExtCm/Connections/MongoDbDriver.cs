using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
using Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbExtCm.Results;

namespace MongoDbExtCm.Connections;

/// <summary>
/// Implements the database driver for MongoDB.
/// </summary>
public class MongoDbDriver : NpOnDbDriver
{
    private MongoClient? _client;
    private IMongoCollection<BsonDocument>? _collection;

    public sealed override string Name { get; set; } = "MongoDB";
    public sealed override string Version { get; set; } = "Unknown";

    public override bool IsValidSession => _client != null; //&& _collection != null;

    public MongoDbDriver(MongoDbConnectOption option) : base(option)
    {
    }

    /// <summary>
    /// Establishes a connection to the MongoDB server and gets the specified collection.
    /// </summary>
    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsValidSession)
        {
            return;
        }

        await DisconnectAsync();

        try
        {
            var settings = MongoClientSettings.FromConnectionString(Option.ConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            _client = new MongoClient(settings);
            var database = _client.GetDatabase(Option.DatabaseName); // database
            await database.RunCommandAsync((Command<BsonDocument>)"{ping: 1}", cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(Option.CollectionName))  
                _collection = database.GetCollection<BsonDocument>(Option.CollectionName);

            // Get server version for display
            var buildInfoCommand = new BsonDocument("buildInfo", 1);
            var buildInfo =
                await database.RunCommandAsync<BsonDocument>(buildInfoCommand, cancellationToken: cancellationToken);
            Version = buildInfo["version"].AsString;
            Name = $"MongoDB {Version}";
        }
        catch (Exception ex)
        {
            _client = null;
            _collection = null;
            // throw; 
        }
    }

    public override Task DisconnectAsync()
    {
        _client = null;
        _collection = null;
        return Task.CompletedTask;
    }

    public override async Task<INpOnWrapperResult> Query(INpOnDbCommand? command)
    {
        if (!IsValidSession || _collection == null)
        {
            return new MongoResultSetWrapper().SetFail(EDbError.Session);
        }

        if (command == null)
        {
            return new MongoResultSetWrapper().SetFail(EDbError.CommandText);
        }

        var filterText = string.IsNullOrWhiteSpace(command.CommandText) ? "{}" : command.CommandText;
        try
        {
            CancellationToken cancellationToken = CancellationToken.None;
            var filter = BsonDocument.Parse(filterText);
            var documents = await _collection.Find(filter).ToListAsync(cancellationToken);
            return new MongoResultSetWrapper(documents);
        }
        catch (Exception ex)
        {
            return new MongoResultSetWrapper().SetFail(EDbError.CommandText);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync();
    }
}