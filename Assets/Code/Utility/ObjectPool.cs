using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// A generic, ObjectPool for reusing objects.
/// </summary>
public class ObjectPool<T> where T : class
{
    /// <summary>
    /// The function used to create new objects for the pool.
    /// </summary>
    private readonly Func<T> _factory;

    /// <summary>
    /// An optional action called on object when they are retrieved from the pool.
    /// </summary>
    private readonly Action<T> _retrieveAction;

    /// <summary>
    /// An optional action called on objects when they are released to the pool.
    /// </summary>
    private readonly Action<T> _releaseAction;

    /// <summary>
    /// A fast collection for storing pooled objects.
    /// </summary>
    private readonly List<T> _pool;

    /// <summary>
    /// A hash set used to O(1) check if an element is already in the pool.
    /// </summary>
    private readonly HashSet<T> _hashSet;

    /// <summary>
    /// Constructs the object pool.
    /// </summary>
    public ObjectPool(Func<T> factory,
        Action<T> retrieveAction = null,
        Action<T> releaseAction = null)
    {
        _factory = factory;
        _retrieveAction = retrieveAction;
        _releaseAction = releaseAction;
        _pool = new List<T>();
        _hashSet = new HashSet<T>();
    }

    /// <summary>
    /// Retrieve an object from the pool or by invoking the factory when the pool is empty.
    /// </summary>
    public T Retrieve()
    {
        if (_pool.TryTakeLast(out var obj))
        {
            _hashSet.Remove(obj);
        }
        else
        {
            obj = _factory.Invoke();
        }
        try
        {
            _retrieveAction?.Invoke(obj);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"ObjectPool retrieve action threw: {e}");
        }
        return obj;
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    public void Release(T obj)
    {
        if (Contains(obj))
        {
            UnityEngine.Debug.LogWarning($"Attempting to release duplicate object: {obj} to ObjectPool.");
            return;
        }
        try
        {
            _releaseAction?.Invoke(obj);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"ObjectPool release action threw: {e}");
        }
        _pool.Add(obj);
        _hashSet.Add(obj);
    }

    /// <summary>
    /// Returns the next object in the pool without actually removing it from the pool or invoking
    /// the pools retrieve Action on it. Will create a new object in the pool if none exist.
    /// </summary>
    public T Peek()
    {
        if (!_pool.TryPeekLast(out var obj))
        {
            obj = _factory.Invoke();
            _pool.Add(obj);
        }
        return obj;
    }

    /// <summary>
    /// Returns true if pool contains the object.
    /// </summary>
    public bool Contains(T obj)
    {
        return _hashSet.Contains(obj);
    }
}
