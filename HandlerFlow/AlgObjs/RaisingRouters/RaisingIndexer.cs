using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HandlerFlow.AlgObjs.CtrlObjs;
using CommonMode;
using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.RaisingRouters;

public static class RaisingIndexer
{
    public static void V1(this BaseCtrl ctrl)
    {
        var keyProperties = ctrl.GetPropertiesWithAttribute<KeyAttribute>().ToArray();
        // Case
        var idProperty = keyProperties.FirstOrDefault(x => x.propertyInfo.Name == nameof(BaseCtrl.Id));
        if (idProperty.propertyInfo != null)
        {
            string id = ctrl.Id;
        }

        keyProperties = keyProperties.Except([idProperty]).ToArray(); // các khóa khác nếu dùng cluster indexers
        var relationshipProperties = ctrl.GetPropertiesWithGenericAttribute(typeof(RelationshipAttribute<>)).ToArray();
        
        foreach (var keyProperty in keyProperties)
        {
            // keyProperty.propertyInfo
        }
        //
        // // 3. Kiểm tra xem có tìm thấy thuộc tính khóa không.
        // if (keyProperties.Count() != 0)
        // {
        //     // Nếu tìm thấy, bạn đã có được PropertyInfo của thuộc tính 'Id'.
        //     // Tên của thuộc tính khóa là:
        //     string keyPropertyName = keyProperty.Name;
        //
        //     Console.WriteLine($"Tìm thấy thuộc tính khóa [Key] trong lớp '{type.Name}': '{keyPropertyName}'");
        //     Console.WriteLine($"Kiểu dữ liệu của khóa: {keyProperty.PropertyType.Name}");
        //
        //     // Từ đây bạn có thể làm nhiều việc khác với keyProperty, ví dụ:
        //     // - Lấy giá trị của nó từ một đối tượng cụ thể: keyProperty.GetValue(myObject)
        // }
        // else
        // {
        //     // Điều này gần như không thể xảy ra vì T đã được ràng buộc kế thừa từ BaseCtrl
        //     Console.WriteLine($"Không tìm thấy thuộc tính nào được đánh dấu [Key] trong lớp '{type.Name}'.");
        // }
    }
}