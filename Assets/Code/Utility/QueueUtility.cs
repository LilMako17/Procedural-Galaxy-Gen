using System.Collections.Generic;

/// <summary>
/// Utility methods for the generic Queue collection.
/// </summary>
public static class QueueUtility
{
    /// <summary>
    /// Removes an element from a Queue by unwinding it, filtering out the value, and
    /// rebuilding the queue in the correct order.
    /// </summary>
    public static void Remove<T>(this Queue<T> queue, T element)
    {
        var list = ListPool<T>.Retrieve();
        while (queue.Count > 0)
        {
            var dequeued = queue.Dequeue();
            if (!EqualityComparer<T>.Default.Equals(dequeued, element))
            {
                list.Add(dequeued);
            }
        }
        for (var i = list.Count - 1; i >= 0; i--)
        {
            queue.Enqueue(list[i]);
        }
        ListPool<T>.Release(list);
    }
}
