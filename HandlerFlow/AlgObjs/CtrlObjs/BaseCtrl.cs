using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs
{
    public abstract class BaseCtrl
    {
        #region Field Config

        [Pk(nameof(BaseCtrl.Id))] public required int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // field mapper (initializer)
        public Dictionary<string, string>? FieldMap { get; protected set; }

        /// <summary>
        /// call in first requisition 
        /// </summary>
        public void CreateDefaultFieldMapper()
        {
            if (FieldMap is {Count: > 0})
                throw new ArgumentNullException(nameof(FieldMap) + "is created"); 
            BaseFieldMapper();
        }

        private void BaseFieldMapper()
        {
            FieldMap ??= new();
            FieldMap.Add(nameof(Id), "id");
            FieldMap.Add(nameof(CreatedAt), "created_at");
            FieldMap.Add(nameof(UpdatedAt), "updated_at");
            FieldMapper();
        }

        public void FieldMapperCustom(Dictionary<string, string> newFieldMap, bool isOverrideMapperFieldInfo = true)
        {
            if (isOverrideMapperFieldInfo)
                FieldMap = new Dictionary<string, string>();
            newFieldMap.ToList().ForEach(x => FieldMap.Add(x.Key, x.Value));
        }

        #endregion Field Config


        protected abstract void FieldMapper();

        /// <summary>
        /// fill data from others -> object
        /// </summary>
        /// <param name="convertObjectMethod"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<T> ToObject<T>(
            Func<T, Task<string>>? convertObjectMethod) where T : BaseCtrl, new()
        {
            if (convertObjectMethod == null)
                throw new ArgumentNullException(nameof(convertObjectMethod));
            var obj = new T();
            var result = await convertObjectMethod(obj);
            return obj;
        }
    }

    public static class BaseCtrlExtensions
    {
        /// <summary>
        /// Fill data from others -> object (extension method)
        /// </summary>
        /// <param name="obj">Object inherit from BaseCtrl</param>
        /// <param name="convertObjectMethod">Convert Method</param>
        /// <typeparam name="T">BaseCtrl inherit Type</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<T> ToObjectAsync<T>(
            this T obj,
            Func<T, Task<string>>? convertObjectMethod) where T : BaseCtrl
        {
            if (convertObjectMethod == null)
                throw new ArgumentNullException(nameof(convertObjectMethod));

            var result = await convertObjectMethod(obj);
            // có thể xử lý result nếu cần
            return obj;
        }

        /// <summary>
        /// Is Inherit class from BaseCtrl?
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        public static bool IsChildOfBaseCtrl(this Type ctrlType)
        {
            Type baseType = typeof(BaseCtrl);
            return ctrlType != baseType && ctrlType.IsSubclassOf(baseType);
        }

        /// <summary>
        /// used in caching when retrieving the first object,
        /// the parameter from FieldMap will be loaded into the cache and reused for objects of the same Type
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        public static BaseCtrl? CreateDefaultFieldMapperWithEmptyObject(this Type ctrlType)
        {
            var emptyCtrl = (BaseCtrl?)Activator.CreateInstance(ctrlType);
            emptyCtrl?.CreateDefaultFieldMapper();
            return emptyCtrl;
        }
    }
    
}