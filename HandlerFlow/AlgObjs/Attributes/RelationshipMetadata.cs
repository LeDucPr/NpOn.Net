using System.Collections.Concurrent;
using System.Reflection;
using Enums;

namespace HandlerFlow.AlgObjs.Attributes;

public record RelationshipInfo(
    PropertyInfo NavigationProperty,
    PropertyInfo ForeignKeyProperty
);

public static class RelationshipMetadataCache
{
    private static readonly ConcurrentDictionary<Type, List<RelationshipInfo>> Cache = new();

    /// <summary>
    /// Thông tin lưu trữ về Properties Info
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<RelationshipInfo> GetRelationships(Type type)
    {
        // Cache
        return Cache.GetOrAdd(type, t =>
        {
            var results = new List<RelationshipInfo>();
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var navProperty in properties)
            {
                var attr = navProperty.GetCustomAttribute<NavigationPropertyAttribute>();
                if (attr == null)
                    continue;
                // NavigationPropertyAttribute.ForeignKeyPropertyName
                var fkProperty = t.GetProperty(attr.ForeignKeyPropertyName);
                if (fkProperty == null)
                {
                    throw new InvalidOperationException
                    (EHandlerError.NavigationProperty +
                     $" - '{navProperty.Name}' " +
                     $"but the specified foreign key property '{attr.ForeignKeyPropertyName}' was not found.");
                }

                results.Add(new RelationshipInfo(navProperty, fkProperty));
            }

            return results;
        });
    }
}