using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.DbCommands;

public interface INpOnDbResult
{
    INpOnDbResult SetSuccess();
    INpOnDbResult SetFail();
    INpOnDbResult SetFail(Exception ex);
    INpOnDbResult SetFail(string errorMessage);
    bool? IsHasResult { get; }
    EDb DataBaseType { get; }
    EDbLanguage? DatabaseLanguage { get; }
}

public interface INpOnDbResult<T> : INpOnDbResult where T : class
{
    Type ResultType { get; }
    void SetResult(T output);

    T? Result { get; }
}

public interface INpOnDbResult<T, TRow> : INpOnDbResult where T : class
{
    // Special
    [NpOnDbSupport(EDb.Cassandra, EDb.Postgres)]
    TRow[]? GetRows();
}

public abstract class NpOnDbResult<T> : INpOnDbResult<T> where T : class
{
    private bool? _isHasResult;
    private Exception? _exception;
    private readonly EDb _eDb;
    private readonly EDbLanguage _dbLanguage;
    private readonly ILogger<NpOnDbResult<T>> _logger = new Logger<NpOnDbResult<T>>(new NullLoggerFactory());
    private T? _result = null;
    private INpOnDbResult<T> _npOnDbResultImplementation;

    public Type ResultType => typeof(T);

    public NpOnDbResult(EDb eDb)
    {
        try
        {
            _eDb = eDb;
            _dbLanguage = _eDb.ChooseLanguage();
            _exception = null;
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    public INpOnDbResult SetSuccess()
    {
        _isHasResult = true;
        return this;
    }

    public INpOnDbResult SetFail()
    {
        _isHasResult = false;
        return this;
    }

    public INpOnDbResult SetFail(Exception ex)
    {
        _isHasResult = false;
        _exception = ex;
        _logger.LogError(ex.Message);
        return this;
    }

    public INpOnDbResult SetFail(string errorMessage)
    {
        _isHasResult = false;
        _logger.LogError(errorMessage);
        return this;
    }

    public bool? IsHasResult => _isHasResult;
    public EDb DataBaseType => _eDb;
    public EDbLanguage? DatabaseLanguage => _dbLanguage;

    protected void ValidateMethodDbSupport([System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
    {
        MethodInfo? method = GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new InvalidOperationException(
                $"Method '{methodName}' not found for validation in type '{GetType().FullName}'.");
        }

        var attribute = method.GetCustomAttribute<NpOnDbSupportAttribute>();
        if (attribute != null)
        {
            if (!attribute.IsSupported(DataBaseType))
            {
                throw new NotSupportedException(
                    $"Method '{methodName}' is not supported for database type '{DataBaseType}'. " +
                    $"Supported types are: {string.Join(", ", attribute.SupportedDatabases?.ToArray() ?? [])}.");
            }
        }
        // pass
    }

    public void SetResult(T? output)
    {
        _result = output;
        _isHasResult = true;
        _exception = null;
    }

    public T? Result => _result;
}

public abstract class NpOnDbResult<T, TRow> : NpOnDbResult<T>, INpOnDbResult<T, TRow>
    where T : class
    where TRow : INpOnRow
{
    protected NpOnDbResult(EDb eDb) : base(eDb)
    {
    }

    public virtual TRow[]? GetRows()
    {
        ValidateMethodDbSupport();
        if (GetType() == typeof(INpOnDbResult) || GetType() == typeof(INpOnDbResult<T>) ||
            GetType() == typeof(INpOnDbResult<T, TRow>))
        {
            throw new NotSupportedException(
                $"Method '{nameof(GetRows)}' not found for validation in type '{GetType().FullName}'.");
        }

        return null;
    }
}