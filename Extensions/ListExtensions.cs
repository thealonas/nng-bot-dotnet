namespace nng_bot.Extensions;

public static class ListExtensions
{
    private static readonly Random Random = new();

    public static T GetRandom<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        return list[Random.Next(list.Count)];
    }

    public static void RemoveIfExists<T>(this HashSet<T> source, T target)
    {
        if (!source.Contains(target)) return;
        source.Remove(target);
    }
}
