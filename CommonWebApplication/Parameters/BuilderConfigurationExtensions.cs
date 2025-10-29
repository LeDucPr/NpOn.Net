using Enums;

namespace CommonWebApplication.Parameters;

public static class BuilderConfigurationExtensions
{
    public static object? TryGetConfig(this IConfiguration configuration, EApplicationConfiguration key)
    {
        var section = configuration.GetSection(key.ToString());
        if (section.Exists())
            return section.Value;
        return null;
    } 
}