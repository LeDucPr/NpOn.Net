namespace Common.Infrastructures.NpOn.CommonDb.DbCommands;

public interface INpOnDbCommandParam
{
    public string ParamName { get; set; }
    public object? ParamValue { get; set; }
}

public class NpOnDbCommandParam : INpOnDbCommandParam
{
    public required string ParamName { get; set; }
    public object? ParamValue { get; set; }
}

public interface INpOnDbCommandParam<TEnum> : INpOnDbCommandParam where TEnum : Enum
{
    public TEnum ParamType { get; set; }
}

public class NpOnDbCommandParam<TEnum> : NpOnDbCommandParam, INpOnDbCommandParam<TEnum> where TEnum : Enum
{
    public required TEnum ParamType { get; set; }
}