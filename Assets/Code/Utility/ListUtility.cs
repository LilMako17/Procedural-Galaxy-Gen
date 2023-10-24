using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility methods for the generic List collection.
/// </summary>
public static class ListUtility
{
    private static Random rng = new Random();

    /// <summary>
    /// Returns a clone of the list, references will be the same (not a deep copy).
    /// </summary>
    public static List<T> Clone<T>(this List<T> list)
    {
        return list.AsEnumerable().ToList();
    }

    /// <summary>
    /// Removes and returns the first element of the list.
    /// </summary>
    public static T TakeFirst<T>(this List<T> list)
    {
        var element = list[0];
        list.RemoveAt(0);
        return element;
    }

    /// <summary>
    /// Removes and returns the last element of the list.
    /// </summary>
    public static T TakeLast<T>(this List<T> list)
    {
        var element = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return element;
    }


    /// <summary>
    /// Removes and returns the last element of the list.
    /// </summary>
    public static bool TryTakeLast<T>(this List<T> list, out T element)
    {
        if (list.Count > 0)
        {
            element = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return true;
        }
        element = default(T);
        return false;
    }

    /// <summary>
    /// Attempts to return the last element of a list if it has any elements.
    /// </summary>
    public static bool TryPeekLast<T>(this List<T> list, out T element)
    {
        if (list.Count > 0)
        {
            element = list[list.Count - 1];
            return true;
        }
        element = default(T);
        return false;
    }

    /// <summary>
    /// Removes and returns the nth element of the list.
    /// </summary>
    public static T TakeAt<T>(this List<T> list, int index)
    {
        var element = list[index];
        list.RemoveAt(index);
        return element;
    }

    public static T TakeRandom<T>(this List<T> list)
    {
        if(IsNullOrEmpty<T>(list))
        {
            return default(T);
        }
        var randomIndex = rng.Next(list.Count);
        return list[randomIndex];
    }

    public static T TakeRandom<T>(this List<T> list, Random random)
    {
        if (IsNullOrEmpty<T>(list))
        {
            return default(T);
        }
        var randomIndex = random.Next(list.Count);
        return list[randomIndex];
    }

    public static IEnumerable<T> TakeRandom<T>(this List<T> list, int num)
    {
        list.Shuffle();
        var randomList = list.Take(num);
        return randomList;
    }

    /// <summary>
    /// Returns true if the list is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return list == null || list.Count == 0;
    }

    /// <summary>
    /// Randomize the order of a list
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Gets a random element from a list.
    /// </summary>
    public static T GetRandom<T>(this IList<T> list)
    {
        return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count - 1)] : default(T);
    }
}
