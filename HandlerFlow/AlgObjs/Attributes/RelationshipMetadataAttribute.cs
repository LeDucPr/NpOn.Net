using System.Reflection;
using CommonMode;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;

namespace HandlerFlow.AlgObjs.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class RelationshipMetadataAttribute<T> : Attribute where T : BaseCtrl
{
    /// <summary>
    /// Type (T) BaseCtrl
    /// </summary>
    public Type RelatedCtrlType => typeof(T);

    /// <summary>
    /// Thuộc tính khóa ngoại
    /// </summary>
    public string? ForeignKeyPropertyName { get; }

    // public PropertyInfo? ForeignKeyProperty { get; }

    public RelationshipMetadataAttribute(string foreignKeyPropertyName)
    {
        // 1. Kiểm tra tên thuộc tính không được rỗng
        if (string.IsNullOrWhiteSpace(foreignKeyPropertyName))
            throw new ArgumentNullException(nameof(foreignKeyPropertyName));

        ForeignKeyPropertyName = foreignKeyPropertyName;

        // // info of property 
        // var propertyInfo = typeof(T).GetProperty(
        //     foreignKeyPropertyName,
        //     BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        // if (propertyInfo == null)
        //     throw new ArgumentException(
        //         EHandlerError.DependentProperty.GetDisplayName() +
        //         $" - Property '{foreignKeyPropertyName}' not found on type '{typeof(T).Name}'.");
        //
        // ForeignKeyProperty = propertyInfo;
    }

    // public RelationshipMetadataAttribute(PropertyInfo propertyInfo)
    // {
    //     if (typeof(BaseCtrl) != RelatedCtrlType)
    //         throw new ArgumentException(EHandlerError.RelatedType + $" - {nameof(propertyInfo)}");
    //     ForeignKeyProperty = propertyInfo;
    //     ForeignKeyPropertyName = propertyInfo.Name;
    // }
}