using Newtonsoft.Json.Linq;

namespace Common.Infrastructures.NpOn.CommonDb.DbCommands.Extensions
{
    public static class NpOnDbExecCommandExtensions
    {
        public static INpOnDbExecCommand AsFullJsonBlock(this INpOnDbExecCommand execCommand)
        {
            if (execCommand.Params is not { Count: > 0 })
                return execCommand;
            if (execCommand.Params.Count == 1)
            {
                var singleParamValue = execCommand.Params.Values.First();
                if (singleParamValue is JToken or System.Text.Json.JsonDocument)
                    return execCommand;
            }

            var jsonObject = JObject.FromObject(execCommand.Params);
            var firstValue = jsonObject.Properties().First().Value.ToString();
            var innerObject = JObject.Parse(firstValue);
            var newParams = new Dictionary<string, object>
            {
                { string.Empty, innerObject }
            };

            return new NpOnDbExecCommand(
                execCommand.DataBaseType,
                execCommand.FuncName,
                newParams,
                execCommand.AliasForSingleColumnOutput
            );
        }
    }
}