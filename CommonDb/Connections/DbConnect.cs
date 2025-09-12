using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IsolationLevel = System.Data.IsolationLevel;

namespace CommonDb.Connections;

public class DbConnect<T> : DbConnection where T : IDbDriver
{
    private T _dbDriver;
    private readonly ILogger<DbConnect<T>> _logger = new Logger<DbConnect<T>>(new NullLoggerFactory());
    private ConnectionState _state = ConnectionState.Closed;

    // Properties từ lớp DbConnection
    public sealed override string ConnectionString { get; set; }
    public sealed override string Database => _dbDriver.Name;
    public sealed override string DataSource => _dbDriver.Name;
    public sealed override string ServerVersion => _dbDriver.Version;
    public sealed override ConnectionState State => _state;

    public DbConnect(string connectionString)
    {
        ConnectionString = connectionString;
        _dbDriver = (T)Activator.CreateInstance(typeof(T), ConnectionString)!;
    }
    
    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (_state != ConnectionState.Closed)
        {
            _logger.LogWarning("Attempted to open a connection that is not closed. Current state: {State}", _state);
            return;
        }

        try
        {
            _state = ConnectionState.Connecting;
            _logger.LogInformation("Opening database connection...");
            await _dbDriver.ConnectAsync(cancellationToken);
            _state = ConnectionState.Open;
            _logger.LogInformation("Database connection opened successfully.");
        }
        catch (Exception ex)
        {
            _state = ConnectionState.Closed;
            _logger.LogError(ex, "Failed to open database connection.");
            throw;
        }
    }

    public override async Task CloseAsync()
    {
        if (_state == ConnectionState.Closed)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Closing database connection...");
            await _dbDriver.DisconnectAsync();
            _logger.LogInformation("Database connection closed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while closing the database connection.");
        }
        finally
        {
            _state = ConnectionState.Closed;
        }
    }

    protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Beginning a new transaction with isolation level: {IsolationLevel}", isolationLevel);
        return await _dbDriver.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
    }

    public async Task ChangeDriverAsync(T newDbDriver, string connectionString, CancellationToken? cancellationToken = default)
    {
        try
        {
            if (newDbDriver == null)
            {
                throw new ArgumentNullException(nameof(newDbDriver));
            }
            var currentDriverType = _dbDriver.GetType();
            var newDriverType = newDbDriver.GetType();
            if (currentDriverType != newDriverType)
            {
                _logger.LogError("Cannot change driver: Mismatched driver types. Current: {CurrentType}, New: {NewType}",
                    currentDriverType.FullName, newDriverType.FullName);
                throw new InvalidOperationException($"Cannot switch from driver type '{currentDriverType.Name}' to '{newDriverType.Name}'.");
            }
            _logger.LogInformation("Changing database driver. Closing current connection...");
            await CloseAsync();
            // Replace driver
            ConnectionString = connectionString ?? ConnectionString;
            _dbDriver = newDbDriver;
            _logger.LogInformation("Driver changed. Opening new connection...");
            await OpenAsync(cancellationToken ?? CancellationToken.None);
        }
        catch (Exception)
        {
            _state = ConnectionState.Closed;
        }
    }
    
    // override
    public override void Open() => OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
    public override void Close() => CloseAsync().GetAwaiter().GetResult();
    public override void ChangeDatabase(string databaseName) => ChangeDatabaseAsync(databaseName).GetAwaiter().GetResult();
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginDbTransactionAsync(isolationLevel, CancellationToken.None).GetAwaiter().GetResult();

    protected override DbCommand CreateDbCommand()
    {
        _logger.LogDebug("Creating a new DbCommand.");
        var command = _dbDriver.CreateDbCommand();
        command.Connection = this; 
        return command;
    }
}