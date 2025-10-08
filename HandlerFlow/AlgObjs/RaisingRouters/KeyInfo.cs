using System.Reflection;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public record KeyInfo(PropertyInfo Property, Attribute Attribute);

public record KeyMetadataInfo(IReadOnlyList<KeyInfo> PrimaryKeys, IReadOnlyList<KeyInfo> ForeignKeys);