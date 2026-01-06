namespace Common.Extensions.NpOn.HandleFlow.Raising;

public record DataLookup(BaseCtrl Ctrl, Exception? Exception);

public record JoinListLookup(string SessionId)
{
    public IReadOnlyList<DataLookup> Data { get; private set; } = new List<DataLookup>();

    public void Merge(DataLookup dataLookup)
    {
        if (Data is List<DataLookup> mutableData)
            mutableData.Add(dataLookup);
        else
            Data = new List<DataLookup>(Data) { dataLookup };
    }
}