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

    public void SetResult(T? output)
    {
        _result = output;
        _isHasResult = true;
        _exception = null;
    }

    public T? Result => _result;
}