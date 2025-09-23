namespace CommonDb.DbCommands;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class NpOnDbSupportAttribute : Attribute 
{
    // Getter method for private fields
    public IReadOnlyCollection<EDb>? SupportedDatabases => _supportedDatabases;
    
    // Fields
    private readonly IReadOnlyCollection<EDb>? _supportedDatabases;
    private readonly Type[]? _resultTypes;
    public NpOnDbSupportAttribute(params EDb[]? supportedDbs)
    {
        if (supportedDbs == null || supportedDbs.Length == 0)
            return; // ?
        _supportedDatabases = supportedDbs;
        _resultTypes = supportedDbs.ToList().Select(x => x.GetType()).ToArray();
    }

    public bool Detect<T>()
    {
        if (_supportedDatabases == null)
            return false;
        return _resultTypes!.Contains(typeof(T));
    }

    public bool IsSupported(EDb dataBaseType)
    {
        if (_supportedDatabases == null)
            return false;
        return _supportedDatabases.Contains(dataBaseType);
    }
}