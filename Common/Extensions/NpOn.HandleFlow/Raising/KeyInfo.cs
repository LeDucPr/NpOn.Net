using System.Reflection;

namespace Common.Extensions.NpOn.HandleFlow.Raising;

public record KeyInfo(PropertyInfo Property, Attribute Attribute);

public record KeyMetadataInfo(
    IReadOnlyList<KeyInfo> PrimaryKeys,
    IReadOnlyList<KeyInfo> ForeignKeys,
    IReadOnlyList<KeyInfo> ForeignKeyIds
);