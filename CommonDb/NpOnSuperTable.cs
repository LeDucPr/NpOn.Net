using CommonDb.DbResults;
using Enums;

namespace CommonDb;

public class NpOnSuperTableWrapper : INpOnSuperTableWrapper
{
    public NpOnSuperTableWrapper(List<INpOnWrapperResult>? concatResults)
    {
        if (concatResults is not { Count: > 0 })
            return;
    }

    public IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers { get; }
    public INpOnCollectionWrapper CollectionWrappers { get; }

    public void SetSuccess()
    {
        throw new NotImplementedException();
    }

    public INpOnWrapperResult SetFail(EDbError error)
    {
        throw new NotImplementedException();
    }

    public INpOnWrapperResult SetFail(string errorString)
    {
        throw new NotImplementedException();
    }

    public INpOnWrapperResult SetFail(Exception ex)
    {
        throw new NotImplementedException();
    }

    public long QueryTimeMilliseconds { get; }
    public bool Status { get; }
}