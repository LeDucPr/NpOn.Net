namespace CommonDb.DbResults;

public interface INpOnWrapperResult
{
}

public interface INpOnWrapperResult<out TParent, out TChild> : INpOnWrapperResult where TParent : class
{
    TParent Parent { get; }
    TChild Result { get; }
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

    protected NpOnWrapperResult(TParent parent)
    {
        Parent = parent;
        _lazyResult = new Lazy<TChild>(CreateResult);
    }

    protected abstract TChild CreateResult();
}