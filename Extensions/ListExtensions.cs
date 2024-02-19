namespace nng_bot.Extensions;

public static class ListExtensions
{
    private static readonly Random Random = new();

    public static T GetRandom<T>(this IEnumerable<T> source)
    {
        var openList = source.ToList();
        var random = Random.Next(openList.Count);
        return openList[random];
    }

    public static void RemoveIfExists<T>(this HashSet<T> source, T target)
    {
        if (!source.Contains(target)) return;
        source.Remove(target);
    }
}
