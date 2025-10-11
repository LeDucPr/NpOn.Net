namespace CommonMode;

public static class IndexerMode
{
    public static string CreateGuidWithStackTrace()
    {
        var st = new System.Diagnostics.StackTrace();
        var sf = st.GetFrame(1);
        var method = sf?.GetMethod();
        return $"{method?.DeclaringType?.FullName}.{method?.Name}_{Guid.NewGuid()}";
    }

    public static string CreateGuid()
    {
        return Guid.NewGuid().ToString();
    }
}