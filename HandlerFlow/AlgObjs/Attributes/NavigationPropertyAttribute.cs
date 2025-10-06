namespace HandlerFlow.AlgObjs.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NavigationPropertyAttribute : Attribute
{
    /// <summary> 
    /// ForeignKeyPropertyName 
    /// </summary>
    public string ForeignKeyPropertyName { get; }

    /// <summary>
    /// Điều hướng Metadata 
    /// </summary>
    /// <param name="foreignKeyPropertyName">Tên của thuộc tính khóa ngoại. Luôn sử dụng nameof() để đảm bảo an toàn.</param>
    public NavigationPropertyAttribute(string foreignKeyPropertyName)
    {
        ForeignKeyPropertyName = foreignKeyPropertyName;
    }
}