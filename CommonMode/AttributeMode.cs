using System.Reflection;

namespace CommonMode;

public static class AttributeMode
{
    #region Get Attributes from Object Instance

    /// <summary>
    /// Lấy toàn bộ các attribute mà một đối tượng đang sở hữu (thông qua Type của nó).
    /// Đây là câu trả lời trực tiếp cho yêu cầu của bạn.
    /// </summary>
    /// <param name="source">Đối tượng lấy attribute.</param>
    /// <param name="inherit">True để tìm kiếm trong cả các lớp cha của đối tượng.</param>
    /// <returns>Một danh sách các attribute.</returns>
    public static IEnumerable<Attribute> GetAttributes(this object? source, bool inherit = true)
    {
        if (source == null)
            return []; // Enumerable.Empty<Attribute>();
        return source.GetType().GetCustomAttributes(inherit).Cast<Attribute>();
    }

    /// <summary>
    /// Lấy tất cả các attribute của một kiểu cụ thể từ một đối tượng.
    /// </summary>
    /// <typeparam name="TAttribute">Kiểu attribute cần lấy.</typeparam>
    /// <param name="source">Đối tượng nguồn.</param>
    /// <param name="inherit">True để tìm kiếm trong cả các lớp cha.</param>
    /// <returns>Một danh sách các attribute thuộc kiểu TAttribute.</returns>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this object? source, bool inherit = true)
        where TAttribute : Attribute
    {
        if (source == null)
            return []; // Enumerable.Empty<TAttribute>();
        return source.GetType().GetCustomAttributes<TAttribute>(inherit);
    }

    /// <summary>
    /// Lấy một attribute cụ thể từ một đối tượng (lấy cái đầu tiên nếu có nhiều).
    /// </summary>
    /// <typeparam name="TAttribute">Kiểu attribute cần lấy.</typeparam>
    /// <param name="source">Đối tượng nguồn.</param>
    /// <param name="inherit">True để tìm kiếm trong cả các lớp cha.</param>
    /// <returns>Attribute được tìm thấy hoặc null nếu không có.</returns>
    public static TAttribute? GetAttribute<TAttribute>(this object? source, bool inherit = true)
        where TAttribute : Attribute
    {
        if (source == null)
        {
            return null;
        }

        return source.GetType().GetCustomAttribute<TAttribute>(inherit);
    }

    #endregion


    #region Get Attributes from Type or MemberInfo

    /// <summary>
    /// Lấy một attribute cụ thể từ một MemberInfo (như PropertyInfo, MethodInfo, FieldInfo).
    /// </summary>
    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo? member, bool inherit = true)
        where TAttribute : Attribute
    {
        return member?.GetCustomAttribute<TAttribute>(inherit);
    }

    /// <summary>
    /// Lấy tất cả các attribute của một kiểu cụ thể từ một MemberInfo.
    /// </summary>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo? member, bool inherit = true)
        where TAttribute : Attribute
    {
        return member?.GetCustomAttributes<TAttribute>(inherit) ?? []; // Enumerable.Empty<TAttribute>();
    }

    #endregion


    #region Generic Mode

    public static IEnumerable<(PropertyInfo propertyInfo, Attribute attribute, Type propertyType)>
        GetPropertiesWithAttribute<TAttribute>(this object? source, bool inherit = true)
        where TAttribute : Attribute
    {
        if (source == null)
            return [];

        var type = source is Type t ? t : source.GetType();

        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Select(prop => new
            {
                PropertyInfo = prop,
                Attribute = prop.GetCustomAttribute<TAttribute>(inherit)
            })
            .Where(item => item.Attribute != null)
            .Select(item => (
                propertyInfo: item.PropertyInfo,
                attribute: (Attribute)item.Attribute!,
                propertyType: item.PropertyInfo.PropertyType
            ));
    }


    public static IEnumerable<(PropertyInfo propertyInfo, Attribute attribute, Type propertyType)>
        GetPropertiesWithGenericAttribute(this object? source, Type openGenericAttributeType, bool inherit = true)
    {
        if (source == null)
            yield break;

        if (!openGenericAttributeType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                "The provided type must be an open generic type definition, e.g., typeof(FkAttribute<>).",
                nameof(openGenericAttributeType));
        }

        var type = source is Type t ? t : source.GetType();

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach (var prop in props)
        {
            var attributes = prop.GetCustomAttributes(inherit);
            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                if (attrType.IsGenericType && attrType.GetGenericTypeDefinition() == openGenericAttributeType)
                {
                    yield return (prop, (Attribute)attr, prop.PropertyType);
                }
            }
        }
    }

    /// <summary>
    /// Validate Attribute that attached
    /// </summary>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool HasClassAttribute<T>(Type? type) where T : Attribute
        => type?.GetCustomAttribute<T>(inherit: true) != null;

    public static Type? GetPropertyTypeFromAttribute(this Attribute attr, string propertyName)
    {
        Type attrType = attr.GetType();
        PropertyInfo? relatedTypeProp = attrType.GetProperty(propertyName);
        if (relatedTypeProp == null)
            return null;
        return (Type)relatedTypeProp.GetValue(attr)!;
    }
    
    #endregion Generic Mode
}