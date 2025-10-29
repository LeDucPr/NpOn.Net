using ProtoBuf;

namespace CommonDb.DbResults;

[ProtoContract]
public interface INpOnRowWrapper
{
    IReadOnlyDictionary<string, INpOnCell> GetRowWrapper();
}

[ProtoContract]
public interface INpOnColumnWrapper
{
    IReadOnlyDictionary<int, INpOnCell> GetColumnWrapper();
}

[ProtoContract]
public interface INpOnCollectionWrapper
{
    IReadOnlyDictionary<int, INpOnColumnWrapper?> GetColumnWrapperByIndexes(int[] indexes);
    IReadOnlyDictionary<string, INpOnColumnWrapper?> GetColumnWrapperByColumnNames(string[]? columnNames = null);
    [ProtoMember(1)] IEnumerable<string> Keys { get; }
}

[ProtoContract]
public interface INpOnTableWrapper
{
    [ProtoMember(1)] IReadOnlyDictionary<int, INpOnRowWrapper?> RowWrappers { get; }
    [ProtoMember(2)] INpOnCollectionWrapper CollectionWrappers { get; }
}

[ProtoContract]
public interface INpOnSuperTableWrapper : INpOnTableWrapper, INpOnWrapperResult
{
}