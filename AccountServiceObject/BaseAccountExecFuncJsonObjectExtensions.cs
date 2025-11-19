using CommonDb.DbResults;
using CommonObject;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;

namespace AccountServiceObject;

public static class BaseAccountExecFuncJsonObjectExtensions
{
    public static IEnumerable<BaseCtrl>? GenericConverterForBaseAccountJson(this INpOnWrapperResult result, Type ctrlType, string? jsonColumnName=null)
    {
        if (!ctrlType.IsSubclassOf(typeof(BaseAccountExecFuncJsonObject)) && ctrlType != typeof(BaseAccountExecFuncJsonObject))
            return null; 

        if (result is not INpOnTableWrapper tableWrapper)
            return null; 

        var ctrlList = new List<BaseCtrl>();
        foreach (var row in tableWrapper.RowWrappers)
        {
            // Tìm ô chứa dữ liệu JSON bằng tên cột đã cung cấp
            if (row.Value?.GetRowWrapper().TryGetValue(jsonColumnName.AsDefaultString(), out var cell) ?? false)
            {
                if (cell.ValueAsObject is string jsonValue)
                {
                    BaseAccountExecFuncJsonObject? newCtrl;
                    if (jsonColumnName != null)
                    {
                        newCtrl = (BaseAccountExecFuncJsonObject?)Activator.CreateInstance(ctrlType, jsonColumnName);
                    }
                    else
                    {
                        newCtrl = (BaseAccountExecFuncJsonObject?)Activator.CreateInstance(ctrlType);
                    }
                    if (newCtrl != null)
                    {
                        newCtrl.Json = jsonValue;
                        ctrlList.Add(newCtrl);
                    }
                }
            }
        }

        return ctrlList;
    }
}