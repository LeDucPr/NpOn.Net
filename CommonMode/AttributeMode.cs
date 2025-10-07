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

    public static IEnumerable<(PropertyInfo propertyInfo, Type propertyType, object? value)> GetPropertiesWithAttribute<TAttribute>(this object? source, bool inherit = true)
        where TAttribute : Attribute
    {
        if (source == null)
        {
            return []; 
        }
        return source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.IsDefined(typeof(TAttribute), inherit))
            .Select(prop => (
                propertyInfo: prop,
                propertyType: prop.PropertyType,
                value: prop.GetValue(source) 
            ));
    }
    
    public static IEnumerable<(PropertyInfo propertyInfo, Attribute attribute)> GetPropertiesWithGenericAttribute(this object? source, Type openGenericAttributeType, bool inherit = true)
    {
        if (source == null)
        {
            yield break; 
        }

        if (!openGenericAttributeType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The provided type must be an open generic type definition, e.g., typeof(RelationshipAttribute<>).", nameof(openGenericAttributeType));
        }

        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attributes = prop.GetCustomAttributes(inherit);
            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                if (attrType.IsGenericType && attrType.GetGenericTypeDefinition() == openGenericAttributeType)
                {
                    yield return (prop, (Attribute)attr);
                }
            }
        }
    }
}