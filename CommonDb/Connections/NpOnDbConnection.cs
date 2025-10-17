using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IsolationLevel = System.Data.IsolationLevel;

namespace CommonDb.Connections;

public abstract class NpOnDbConnection : DbConnection
{
    public abstract INpOnDbDriver Driver { get; }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }

    public override void Close()
    {
        throw new NotImplementedException();
    }

    public override void Open()
    {
        throw new NotImplementedException();
    }

    [AllowNull] public override string ConnectionString { get; set; }
    public override string Database { get; }
    public override ConnectionState State { get; }
    public override string DataSource { get; }
    public override string ServerVersion { get; }

    protected override DbCommand CreateDbCommand()
    {
        throw new NotImplementedException();
    }
}

public class NpOnDbConnection<T> : NpOnDbConnection where T : INpOnDbDriver
{
    private T _dbDriver;
    private readonly ILogger<NpOnDbConnection<T>> _logger = new Logger<NpOnDbConnection<T>>(new NullLoggerFactory());
    private ConnectionState _state = ConnectionState.Closed;

    // Properties từ lớp DbConnection
    public sealed override string Database => _dbDriver.Name;
    public sealed override string DataSource => _dbDriver.Name;
    public sealed override string ServerVersion => _dbDriver.Version;
    public sealed override ConnectionState State => _state;
    public override INpOnDbDriver Driver => _dbDriver;

    public NpOnDbConnection(INpOnConnectOption option)
    {
        _dbDriver = (T)Activator.CreateInstance(typeof(T), option)!;
    }

    public NpOnDbConnection(INpOnDbDriver driver)
    {
        _dbDriver = ((T?)driver)!;
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

    public async Task ChangeDriverAsync(T newDbDriver, string connectionString,
        CancellationToken? cancellationToken = default)
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
                _logger.LogError(
                    "Cannot change driver: Mismatched driver types. Current: {CurrentType}, New: {NewType}",
                    currentDriverType.FullName, newDriverType.FullName);
                throw new InvalidOperationException(
                    $"Cannot switch from driver type '{currentDriverType.Name}' to '{newDriverType.Name}'.");
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
    [AllowNull] public override string ConnectionString { get; set; }
    public override void Open() => OpenAsync(new CancellationToken(true)).GetAwaiter().GetResult();
    public override void Close() => CloseAsync().GetAwaiter().GetResult();

    public override void ChangeDatabase(string databaseName) =>
        ChangeDatabaseAsync(databaseName).GetAwaiter().GetResult();

    #region unUse

    protected override DbCommand CreateDbCommand()
    {
        throw new NotImplementedException();
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    #endregion unUse
}