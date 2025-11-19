using CommonDb.DbResults;
using CommonObject;
using ObjectHandlerFlow.AlgObjs.CtrlObjs;

namespace QuestionServiceObject;

public static class BaseQuestionExecFuncJsonObjectExtensions
{
    public static IEnumerable<BaseCtrl>? GenericConverterForJson(this INpOnWrapperResult result, Type ctrlType, string? jsonColumnName=null)
    {
        if (!ctrlType.IsSubclassOf(typeof(BaseQuestionExecFuncJsonObject)) && ctrlType != typeof(BaseQuestionExecFuncJsonObject))
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
                    BaseQuestionExecFuncJsonObject? newCtrl;
                    if (jsonColumnName != null)
                    {
                        newCtrl = (BaseQuestionExecFuncJsonObject?)Activator.CreateInstance(ctrlType, jsonColumnName);
                    }
                    else
                    {
                        newCtrl = (BaseQuestionExecFuncJsonObject?)Activator.CreateInstance(ctrlType);
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