using System.Diagnostics;
using CommonMode;
using Enums;

namespace CommonDb.DbResults;

public interface INpOnWrapperResult
{
    void SetSuccess();
    INpOnWrapperResult SetFail(EDbError error);
    INpOnWrapperResult SetFail(string errorString);
    INpOnWrapperResult SetFail(Exception ex);
    long QueryTimeMilliseconds { get; }
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

    // Stopwatch 
    private readonly Stopwatch _stopwatch;
    private long _queryTimeMilliseconds = 0;
    private bool _isStopped = false;

    protected NpOnWrapperResult()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    private void SetStopExecute()
    {
        if (_isStopped)
            return;
        _stopwatch.Stop();
        _queryTimeMilliseconds = _stopwatch.ElapsedMilliseconds;
        _isStopped = !_isStopped;
    }

    public void SetSuccess()
    {
        _isSuccess = true;
        SetStopExecute();
    }

    public INpOnWrapperResult SetFail(EDbError error)
    {
        _errorString = error.GetDisplayName();
        _isSuccess = false;
        SetStopExecute();
        return this;
    }

    public INpOnWrapperResult SetFail(Exception ex)
    {
        _errorString = ex.Message;
        _isSuccess = false;
        SetStopExecute();
        return this;
    }

    public INpOnWrapperResult SetFail(string errorString)
    {
        _errorString = errorString;
        _isSuccess = false;
        SetStopExecute();
        return this;
    }

    public long QueryTimeMilliseconds => _queryTimeMilliseconds;

    public bool Status => _isSuccess;
}

/// <summary>
/// Lớp bọc chính nhận đối tượng truy vấn 
/// </summary>
/// <typeparam name="TParent"></typeparam>
/// <typeparam name="TChild"></typeparam>
public abstract class NpOnWrapperResult<TParent, TChild>
    : NpOnWrapperResult, INpOnWrapperResult<TParent, TChild> where TParent : class
{
    public TParent Parent { get; }
    private readonly Lazy<TChild> _lazyResult;
    public TChild Result => _lazyResult.Value;

    protected NpOnWrapperResult(TParent parent) : base()
    {
        Parent = parent;
        _lazyResult = new Lazy<TChild>(CreateResult);
    }

    protected abstract TChild CreateResult();
}