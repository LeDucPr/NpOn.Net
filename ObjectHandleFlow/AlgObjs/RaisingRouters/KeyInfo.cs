using System.Reflection;

namespace ObjectHandlerFlow.AlgObjs.RaisingRouters;

public record KeyInfo(PropertyInfo Property, Attribute Attribute);

public record KeyMetadataInfo(
    IReadOnlyList<KeyInfo> PrimaryKeys,
    IReadOnlyList<KeyInfo> ForeignKeys,
    IReadOnlyList<KeyInfo> ForeignKeyIds
);