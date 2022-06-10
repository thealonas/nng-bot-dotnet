namespace nng_bot.Extensions;

public static class DictionaryExtensions
{
    public static void RemoveIfExists<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey target)
        where TKey : notnull
    {
        if (!source.ContainsKey(target)) return;
        source.Remove(target);
    }
}
