using HandlerFlow.AlgObjs.Attributes;

namespace HandlerFlow.AlgObjs.CtrlObjs
{
    public abstract class BaseCtrl
    {
        #region Field Config

        [Pk(nameof(BaseCtrl.Id))] public required long Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // field mapper (initializer)
        public Dictionary<string, string>? FieldMap { get; protected set; }

        /// <summary>
        /// call in first requisition 
        /// </summary>
        public void CreateDefaultFieldMapper()
        {
            if (FieldMap is { Count: > 0 })
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
            newFieldMap.ToList().ForEach(x => FieldMap?.Add(x.Key, x.Value));
        }

        #endregion Field Config

        protected abstract void FieldMapper();
    }

    public static class BaseCtrlExtensions
    {
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