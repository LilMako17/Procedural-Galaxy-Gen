using System.Text;

/// <summary>
/// A static ObjectPool of StringBuilders for reuse.
/// </summary>
public static class StringBuilderPool
{
    /// <summary>
    /// The ObjectPool for the builders. Clears the builder on retrieval.
    /// </summary>
    private static readonly ObjectPool<StringBuilder> s_ListPool
        = new ObjectPool<StringBuilder>(() => new StringBuilder(), l => l.Clear());

    /// <summary>
    /// Retrieves a pooled StringBuilder.
    /// </summary>
    public static StringBuilder Retrieve()
    {
        return s_ListPool.Retrieve();
    }

    /// <summary>
    /// Releases a StringBuilder to the pool.
    /// </summary>
    public static void Release(StringBuilder toRelease)
    {
        s_ListPool.Release(toRelease);
    }
}
