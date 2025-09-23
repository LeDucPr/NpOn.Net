// using System.Collections.Concurrent;
// using System.Reflection;
//
// namespace CommonDb.DbCommands;
//
// public enum EDbValueType : byte
// {
//     [EDbValue(typeof(string), EDb.Postgres, EDb.Cassandra, EDb.ScyllaDb)]
//     Unknown = 0,
//     
//     [EDbValue(typeof(string), EDb.Postgres, EDb.Cassandra, EDb.ScyllaDb)]
//     Text = 1,
//     
//     [EDbValue(typeof(object), EDb.MongoDb)] 
//     Bson = 2,
//
//     [EDbValue(typeof(string), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres, EDb.MongoDb)]
//     Json = 3,
//
//     [EDbValue(typeof(Array), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres, EDb.MongoDb)]
//     Array = 4,
//
//     [EDbValue(typeof(DateTime), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
//     DateTime = 5,
//
//     [EDbValue(typeof(decimal), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
//     Number = 6,
//
//     [EDbValue(typeof(double), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
//     Float = 7,
//
//     [EDbValue(typeof(long), EDb.Cassandra, EDb.ScyllaDb, EDb.Postgres)]
//     Integer = 8,
// }
//
// public class EDbValueAttribute : Attribute
// {
//     public Type CorrespondingType { get; }
//     public IReadOnlyCollection<EDb> SupportedDbTypes { get; }
//     public EDbValueAttribute(Type correspondingType, params EDb[] supportedDbTypes)
//     {
//         if (supportedDbTypes == null || supportedDbTypes.Length == 0)
//         {
//             throw new ArgumentException("Database does not support this data type", nameof(supportedDbTypes));
//         }
//         CorrespondingType = correspondingType;
//         SupportedDbTypes = supportedDbTypes;
//     }
// }
//
// public static class EDbValueTypeExtensions
// {
//     // Cache cho việc kiểm tra DB hỗ trợ
//     private static readonly ConcurrentDictionary<EDbValueType, IReadOnlyCollection<EDb>> DbSupportCache = new();
//     
//     // Cache MỚI cho việc lấy kiểu C#
//     private static readonly ConcurrentDictionary<EDbValueType, Type> CSharpTypeCache = new();
//     
//     private static readonly ConcurrentDictionary<Type, EDbValueType> ReverseTypeCache = new();
//
//     public static bool IsValidFor(this EDbValueType valueType, EDb dbType)
//     {
//         var supportedDbs = DbSupportCache.GetOrAdd(valueType, (v) =>
//         {
//             var memberInfo = typeof(EDbValueType).GetMember(v.ToString()).FirstOrDefault();
//             if (memberInfo == null)
//                 return [];
//             var attribute = memberInfo.GetCustomAttribute<EDbValueAttribute>();
//             return attribute?.SupportedDbTypes ?? [];
//         });
//
//         return supportedDbs.Contains(dbType);
//     }
//
//     public static Type GetCSharpType(this EDbValueType valueType)
//     {
//         return CSharpTypeCache.GetOrAdd(valueType, v =>
//         {
//             var memberInfo = typeof(EDbValueType).GetMember(v.ToString()).FirstOrDefault();
//             if (memberInfo == null)
//             {
//                 return typeof(object);
//             }
//             var attribute = memberInfo.GetCustomAttribute<EDbValueAttribute>();
//             return attribute?.CorrespondingType ?? typeof(object);
//         });
//     }
//     
//     public static EDbValueType GetEDbValueType(this Type csharpType)
//     {
//         return ReverseTypeCache.GetOrAdd(csharpType, typeToFind =>
//         {
//             foreach (EDbValueType enumValue in Enum.GetValues(typeof(EDbValueType)))
//             {
//                 var definedCSharpType = enumValue.GetCSharpType(); //  using from caching
//                 if (definedCSharpType == typeToFind)
//                 {
//                     return enumValue;
//                 }
//             }
//             return EDbValueType.Unknown; 
//         });
//     }
// }