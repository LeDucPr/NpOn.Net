using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.Connections;

public interface IConnectOptions
{
    bool IsValid();

    IConnectOptions SetConnectionString(string connectionString);
    string? ConnectionString { get; }

    IConnectOptions? SetKeyspace<T>(string keyspace) where T : IDbDriver;
    string? Keyspace { get; }

    IConnectOptions? SetContactAddresses<T>(string[]? contactAddresses) where T : IDbDriver;
    string[]? ContactAddresses { get; }

    IConnectOptions SetShutdownImmediate(bool isShutdownImmediate);
    bool IsShutdownImmediate { get; }

    IConnectOptions SetWaitNextTransaction(bool isWaitNextTransaction);
    bool IsWaitNextTransaction { get; }

    IConnectOptions SetSessionTimeout(long secondsTimeout);
    void ResetSessionTimeout();
    long ConnectionTimeoutSessions { get; }
}

public class DbConnectOptions<T> : IConnectOptions
{
    // private bool _isUseMultiSessions = false;
    private bool _isShutdownImmediate = false;
    private bool _isWaitNextTransaction = true;
    private long _secondsTimeout = 30; //2592000; // 30 days 
    private DateTime _currentConnectionTime;
    private DateTime _expiredConnectionTime;
    private string? _connectionString;
    protected readonly ILogger<DbConnectOptions<T>> _logger = new Logger<DbConnectOptions<T>>(new NullLoggerFactory());

    public virtual bool IsValid()
    {
        try
        {
            if (GetType() == typeof(IConnectOptions))
                throw new NotImplementedException("request Validator configuration from inherited class.");
            return true;
        }
        catch (NotImplementedException)
        {
            return false;
        }
    }

    #region ConnectionString

    public IConnectOptions SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public string? ConnectionString => _connectionString;

    #endregion ConnectionString

    // for databases

    #region Keyspace

    private string? _keyspace = string.Empty; // cassandra, scyllaDb

    [Obsolete("Obsolete")]
    public virtual IConnectOptions SetKeyspace<T>(string keyspace) where T : IDbDriver
    {
        try
        {
            if (IsValid())
            {
                _logger.LogError($"ConnectOptions is not valid for {typeof(IDbDriver)}");
                throw new ExecutionEngineException($"ConnectOptions is not valid for {typeof(IDbDriver)}");
            }

            _keyspace = keyspace;
        }
        catch (ExecutionEngineException)
        {
            _keyspace = null;
        }

        return this;
    }

    public string? Keyspace => _keyspace;

    #endregion Keyspace

    #region ContactAddresses

    private string[]? _contactAddresses;

    [Obsolete("Obsolete")]
    public virtual IConnectOptions? SetContactAddresses<T>(string[]? contactAddresses) where T : IDbDriver
    {
        try
        {
            if (IsValid())
            {
                throw new ExecutionEngineException($"Keyspace is not valid for {typeof(IDbDriver)}");
            }

            _contactAddresses = contactAddresses;
        }
        catch (ExecutionEngineException)
        {
            _contactAddresses = null;
        }

        return this;
    }

    public string[]? ContactAddresses => _contactAddresses;

    #endregion ContactAddresses

    // generic 

    #region SetShutdownImmediate

    public IConnectOptions SetShutdownImmediate(bool isShutdownImmediate = false)
    {
        _isShutdownImmediate = isShutdownImmediate;
        return this;
    }

    public bool IsShutdownImmediate => _isShutdownImmediate;

    #endregion SetShutdownImmediate

    #region WaitNextTransaction

    public IConnectOptions SetWaitNextTransaction(bool isWaitNextTransaction = true)
    {
        _isWaitNextTransaction = isWaitNextTransaction;
        return this;
    }

    public bool IsWaitNextTransaction => _isWaitNextTransaction;

    #endregion WaitNextTransaction

    #region UseMultiSessions

    public IConnectOptions SetSessionTimeout(long secondsTimeout = 30)
    {
        _secondsTimeout = secondsTimeout;
        _currentConnectionTime = DateTime.Now;
        _expiredConnectionTime = DateTime.Now + TimeSpan.FromSeconds(_secondsTimeout);
        return this;
    }

    public void ResetSessionTimeout()
    {
        _currentConnectionTime = DateTime.Now;
        _expiredConnectionTime = DateTime.Now + TimeSpan.FromSeconds(_secondsTimeout);
    }

    public long ConnectionTimeoutSessions => _secondsTimeout;

    #endregion UseMultiSessions
}