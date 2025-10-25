namespace BaseWebApplication.Parameters;

public static class BuilderConfigurationExtensions
{
    public static object? TryGetConfig(this IConfiguration configuration, EConfiguration key)
    {
        var section = configuration.GetSection(key.ToString());
        if (section.Exists())
            return section.Value;
        return null;
    } 
}