using CommonMode;
using Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.Connections;

public interface INpOnConnectOption
{
    bool IsConnectValid(); // validate when initialize 
    bool IsValid([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null);
    bool IsValidRequireFromBase([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null);
    
    INpOnConnectOption SetConnectionString(string connectionString);
    string? ConnectionString { get; }

    INpOnConnectOption? SetKeyspace<T>(string keyspace) where T : INpOnDbDriver;
    string? Keyspace { get; }

    INpOnConnectOption? SetDatabaseName(string databaseName);
    string? DatabaseName { get; }

    INpOnConnectOption? SetCollectionName<T>(string keyspace) where T : INpOnDbDriver;
    string? CollectionName { get; }

    INpOnConnectOption? SetContactAddresses<T>(string[]? contactAddresses) where T : INpOnDbDriver;
    string[]? ContactAddresses { get; }

    INpOnConnectOption SetShutdownImmediate(bool isShutdownImmediate);
    bool IsShutdownImmediate { get; }

    INpOnConnectOption SetWaitNextTransaction(bool isWaitNextTransaction);
    bool IsWaitNextTransaction { get; }

    INpOnConnectOption SetSessionTimeout(long secondsTimeout);
    void ResetSessionTimeout();
    long ConnectionTimeoutSessions { get; }
}

public abstract class DbNpOnConnectOption<T> : INpOnConnectOption
{
    // private bool _isUseMultiSessions = false;
    private bool _isShutdownImmediate = false;
    private bool _isWaitNextTransaction = true;
    private long _secondsTimeout = 30; //2592000; // 30 days 
    private DateTime _currentConnectionTime;
    private DateTime _expiredConnectionTime;
    private string? _connectionString;

    protected readonly ILogger<DbNpOnConnectOption<T>> _logger =
        new Logger<DbNpOnConnectOption<T>>(new NullLoggerFactory());

    #region Validate

    public abstract bool IsConnectValid();

    public virtual bool IsValid(string? propertyName = null)
    {
        try
        {
            if (GetType() == typeof(INpOnConnectOption))
                throw new NotImplementedException("request Validator configuration from inherited class.");
            return true;
        }
        catch (NotImplementedException)
        {
            return false;
        }
    }


    public bool IsValidRequireFromBase(string? propertyName)
    {
        var validPropertyNames = new HashSet<string>
        {
            EConnectLink.SelfValidateConnection.GetDisplayName(),
        };
        if (propertyName == null)
            return false;
        return validPropertyNames.Contains(propertyName);
    }
    #endregion Validate


    #region ConnectionString

    public INpOnConnectOption SetConnectionString(string connectionString)
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
    public virtual INpOnConnectOption SetKeyspace<T>(string keyspace) where T : INpOnDbDriver
    {
        try
        {
            if (!IsValid())
                throw new ExecutionEngineException($"ConnectOptions is not valid for {typeof(INpOnDbDriver)}");
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


    #region Collection

    private string? _collection = string.Empty; // mongoDb

    [Obsolete("Obsolete")]
    public virtual INpOnConnectOption SetCollectionName<T>(string collection) where T : INpOnDbDriver
    {
        try
        {
            if (!IsValid())
                throw new ExecutionEngineException($"ConnectOptions is not valid for {typeof(INpOnDbDriver)}");
            _collection = collection;
        }
        catch (ExecutionEngineException)
        {
            _collection = null;
        }

        return this;
    }

    public string? CollectionName => _collection;

    #endregion Keyspace


    #region ContactAddresses

    private string[]? _contactAddresses;

    [Obsolete("Obsolete")]
    public virtual INpOnConnectOption? SetContactAddresses<T>(string[]? contactAddresses) where T : INpOnDbDriver
    {
        try
        {
            if (!IsValid())
                throw new ExecutionEngineException($"Keyspace is not valid for {typeof(INpOnDbDriver)}");
            if (contactAddresses is not { Length: > 0 })
            {
                _contactAddresses = contactAddresses;
                return this;
            }
            HashSet<string> contactAddressesHashSet = new HashSet<string>(_contactAddresses ?? []);
            foreach (string contactAddress in contactAddresses)
                contactAddressesHashSet.Add(contactAddress);
            _contactAddresses = contactAddressesHashSet.ToArray();
        }
        catch (ExecutionEngineException)
        {
            _contactAddresses = null;
        }

        return this;
    }

    public string[]? ContactAddresses => _contactAddresses;

    #endregion ContactAddresses


    #region Database name

    private string? _databaseName = string.Empty; // postgres

    [Obsolete("Obsolete")]
    public virtual INpOnConnectOption? SetDatabaseName(string databaseName)
    {
        try
        {
            if (!IsValid())
                throw new ExecutionEngineException($"ConnectOptions is not valid for {typeof(INpOnDbDriver)}");
            _databaseName = databaseName;
        }
        catch (ExecutionEngineException)
        {
            _databaseName = null;
        }

        return this;
    }

    public string? DatabaseName => _databaseName;

    #endregion Database name


    // generic 

    #region SetShutdownImmediate

    public INpOnConnectOption SetShutdownImmediate(bool isShutdownImmediate = false)
    {
        _isShutdownImmediate = isShutdownImmediate;
        return this;
    }

    public bool IsShutdownImmediate => _isShutdownImmediate;

    #endregion SetShutdownImmediate


    #region WaitNextTransaction

    public INpOnConnectOption SetWaitNextTransaction(bool isWaitNextTransaction = true)
    {
        _isWaitNextTransaction = isWaitNextTransaction;
        return this;
    }

    public bool IsWaitNextTransaction => _isWaitNextTransaction;

    #endregion WaitNextTransaction


    #region UseMultiSessions

    public INpOnConnectOption SetSessionTimeout(long secondsTimeout = 30)
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