using System.Collections.Generic;

/// <summary>
/// A static ObjectPool of T typed lists for reusing list of T.
/// </summary>
public static class ListPool<T>
{
    /// <summary>
    /// Retrieves a pooled list.
    /// </summary>
    public static List<T> Retrieve()
    {
        return UnityEngine.Pool.ListPool<T>.Get();
    }

    /// <summary>
    /// Convenience method that retrieves a list and add an element.
    /// </summary>
    public static List<T> Retrieve(T t)
    {
        var list = Retrieve();
        list.Add(t);
        return list;
    }

    /// <summary>
    /// Convenience method that retrieves a list and add 2 elements.
    /// </summary>
    public static List<T> Retrieve(T t0, T t1)
    {
        var list = Retrieve();
        list.Add(t0);
        list.Add(t1);
        return list;
    }

    /// <summary>
    /// Convenience method that retrieves a list and add 3 elements.
    /// </summary>
    public static List<T> Retrieve(T t0, T t1, T t2)
    {
        var list = Retrieve();
        list.Add(t0);
        list.Add(t1);
        list.Add(t2);
        return list;
    }

    /// <summary>
    /// Convenience method that retrieves a list and add 4 elements.
    /// </summary>
    public static List<T> Retrieve(T t0, T t1, T t2, T t3)
    {
        var list = Retrieve();
        list.Add(t0);
        list.Add(t1);
        list.Add(t2);
        list.Add(t3);
        return list;
    }

    /// <summary>
    /// Convenience method that retrieves a list and add 5 elements.
    /// </summary>
    public static List<T> Retrieve(T t0, T t1, T t2, T t3, T t4)
    {
        var list = Retrieve();
        list.Add(t0);
        list.Add(t1);
        list.Add(t2);
        list.Add(t3);
        list.Add(t4);
        return list;
    }

    /// <summary>
    /// Retrieves a pooled populated with the elements of an input list.
    /// </summary>
    public static List<T> Retrieve(IEnumerable<T> list)
    {
        var pooledList = Retrieve();
        pooledList.AddRange(list);
        return pooledList;
    }

    /// <summary>
    /// Releases a list to the pool.
    /// </summary>
    public static void Release(List<T> toRelease)
    {
        UnityEngine.Pool.ListPool<T>.Release(toRelease);
    }
}

/// <summary>
/// Extension methods for ListPool.
/// </summary>
public static class ListPool
{
    /// <summary>
    /// Extension method variant of release.
    /// </summary>
    public static void ReleaseToPool<T>(this List<T> toRelease)
    {
        UnityEngine.Pool.ListPool<T>.Release(toRelease);
    }
}
