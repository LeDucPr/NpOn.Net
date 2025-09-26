using CommonMode;

namespace CommonDb.DbResults;

public interface INpOnWrapperResult
{
    void SetSuccess();
    INpOnWrapperResult SetFail(EDbError error);
    INpOnWrapperResult SetFail(string errorString);
    INpOnWrapperResult SetFail(Exception ex);
    bool Status { get; }
}

public interface INpOnWrapperResult<out TParent, out TChild> : INpOnWrapperResult where TParent : class
{
    TParent Parent { get; }
    TChild Result { get; }
}

public abstract class NpOnWrapperResult : INpOnWrapperResult
{
    private bool _isSuccess = false;
    private string? _errorString = null;
    
    public void SetSuccess()
    {
        _isSuccess = true;
    }

    public INpOnWrapperResult SetFail(EDbError error)
    {
        _errorString = error.GetDisplayName();
        _isSuccess = false;
        return this;
    }

    public INpOnWrapperResult SetFail(Exception ex)
    {
        _errorString = ex.Message;
        _isSuccess = false;
        return this;
    }

    public INpOnWrapperResult SetFail(string errorString)
    {
        _errorString = errorString;
        _isSuccess = false;
        return this;
    }

    public bool Status => _isSuccess;
}

/// <summary>
/// Lớp bọc chính nhận đối tượng truy vấn 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public abstract class NpOnWrapperResult<TParent, TChild>
    : INpOnWrapperResult<TParent, TChild> where TParent : class
{
    public TParent Parent { get; }
    private readonly Lazy<TChild> _lazyResult;
    public TChild Result => _lazyResult.Value;

    private bool _isSuccess = false;
    private string? _errorString = null;
    protected NpOnWrapperResult(TParent parent)
    {
        Parent = parent;
        _lazyResult = new Lazy<TChild>(CreateResult);
    }

    protected abstract TChild CreateResult();
    public void SetSuccess()
    {
        _isSuccess = true;
    }

    public INpOnWrapperResult SetFail(EDbError error)
    {
        _errorString = error.GetDisplayName();
        _isSuccess = false;
        return this;
    }

    public INpOnWrapperResult SetFail(Exception ex)
    {
        _errorString = ex.Message;
        _isSuccess = false;
        return this;
    }

    public INpOnWrapperResult SetFail(string errorString)
    {
        _errorString = errorString;
        _isSuccess = false;
        return this;
    }

    public bool Status => _isSuccess;
}