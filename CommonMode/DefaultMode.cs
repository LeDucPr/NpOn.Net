namespace CommonMode;

public static class DefaultMode
{
    public static T ToDefault<T>() where T : class
    {
        return default;
    }
}