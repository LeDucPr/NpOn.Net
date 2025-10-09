namespace HandlerFlow.WrapperProcessors;

public static class WrapperProcessers
{
    public static async Task<TOut?> Processer<TOut>(Func<object[]?, Task<TOut>>? processer,
        params object[]? args
    ) where TOut : notnull
    {
        try
        {
            if (processer == null)
                return default;
            TOut result = await processer(args);
            return result;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public static async Task<TOut?> Processer<T, TOut>(Func<T?, Task<TOut>>? processer, T? arg) where TOut : notnull
    {
        try
        {
            if (processer == null)
                return default;
            TOut result = await processer(arg);
            return result;
        }
        catch (Exception)
        {
            return default;
        }
    }
}