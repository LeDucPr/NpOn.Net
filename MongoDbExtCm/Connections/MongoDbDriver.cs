using CommonDb.Connections;
using CommonDb.DbCommands;
using CommonDb.DbResults;
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

    public override bool IsValidSession => _client != null && _collection != null;

    public MongoDbDriver(INpOnConnectOptions options) : base(options)
    {
        if (options is not MongoDbConnectOptions)
        {
            throw new ArgumentException("The provided options are not valid for the MongoDB driver.", nameof(options));
        }
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

        var mongoOptions = (MongoDbConnectOptions)Options;

        try
        {
            var settings = MongoClientSettings.FromConnectionString(mongoOptions.ConnectionString);
            // Recommended setting for modern applications
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            _client = new MongoClient(settings);

            // Ping the server to confirm a successful connection.
            var database = _client.GetDatabase(mongoOptions.DatabaseName);
            await database.RunCommandAsync((Command<BsonDocument>)"{ping: 1}", cancellationToken: cancellationToken);

            _collection = database.GetCollection<BsonDocument>(mongoOptions.CollectionName);

            // Get server version for display
            var buildInfoCommand = new BsonDocument("buildInfo", 1);
            var buildInfo = await database.RunCommandAsync<BsonDocument>(buildInfoCommand, cancellationToken: cancellationToken);
            Version = buildInfo["version"].AsString;
            Name = $"MongoDB {Version}";
        }
        catch (Exception)
        {
            // Ensure cleanup on connection failure
            _client = null;
            _collection = null;
            throw; // Re-throw the exception to the caller
        }
    }

    /// <summary>
    /// Disposes of the MongoDB client resources.
    /// </summary>
    public override Task DisconnectAsync()
    {
        // MongoClient is designed to be a long-lived object and manages its own connection pool.
        // There is no explicit "Disconnect" or "Shutdown" method.
        // We simply release our references to let the GC handle it when the driver is disposed.
        _client = null;
        _collection = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes a query (a BSON filter) against the configured collection.
    /// </summary>
    public override async Task<INpOnWrapperResult> Query(INpOnDbCommand? command, CancellationToken cancellationToken = default)
    {
        if (!IsValidSession || _collection == null)
        {
            return new InternalErrorResult("Session is not valid or not connected. Call ConnectAsync first.");
        }

        if (command == null)
        {
            return new InternalErrorResult("Command cannot be null.");
        }

        // For MongoDB, an empty CommandText can mean "find all documents".
        var filterText = string.IsNullOrWhiteSpace(command.CommandText) ? "{}" : command.CommandText;

        try
        {
            var filter = BsonDocument.Parse(filterText);
            var documents = await _collection.Find(filter).ToListAsync(cancellationToken);

            // Return the successful result wrapped in our custom class
            return new MongoResultSetWrapper(documents);
        }
        catch (Exception ex)
        {
            // Use the common error factory to create a detailed error result
            return NpOnErrorResult.Create($"Failed to execute MongoDB query: {filterText}", ex);
        }
    }

    /// <summary>
    /// Cleans up resources when the driver is disposed.
    /// </summary>
    protected override async ValueTask DisposeAsyncCore()
    {
        await DisconnectAsync();
    }
}