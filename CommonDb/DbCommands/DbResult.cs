using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonDb.DbCommands;

public interface IDbResult<T> where T : class 
{
    void SetSuccess();
    void SetFail();
    void SetFail(Exception ex);
    void SetFail(string errorMessage);
    bool? IsHasResult { get; }
    EDb DataBaseType { get; }
    EDbLanguage? DatabaseLanguage { get; }
    void SetResult(T output);
    T? Result { get; }
}

public class DbResult<T> : IDbResult<T> where T : class 
{
    private bool? _isHasResult;
    private Exception? _exception;
    private readonly EDb _eDb; 
    private readonly EDbLanguage _dbLanguage; 
    private readonly ILogger<DbResult<T>> _logger = new Logger<DbResult<T>>(new NullLoggerFactory());
    private T? _result = null;

    public DbResult(EDb eDb)
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

    public void SetSuccess() => _isHasResult = true;
    public void SetFail() => _isHasResult = false;

    public void SetFail(Exception ex)
    {
        _isHasResult = false;
        _exception = ex;
        _logger.LogError(ex.Message);
    }

    public void SetFail(string errorMessage)
    {
        _isHasResult = false;
        _logger.LogError(errorMessage);
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