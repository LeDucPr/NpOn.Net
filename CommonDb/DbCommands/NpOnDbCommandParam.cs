namespace CommonDb.DbCommands;

public class NpOnDbCommandParam
{
    public required string ParamName { get; set; }
    public object? ParamValue { get; set; }
}

public class NpOnDbCommandParam<TEnum> : NpOnDbCommandParam where TEnum : Enum
{
    public required TEnum ParamType { get; set; }
}