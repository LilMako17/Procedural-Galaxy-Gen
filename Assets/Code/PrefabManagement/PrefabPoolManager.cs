using System;
using System.Collections.Generic;
using UnityEngine;

public enum VFXPriority
{
    Default,
    High,
}

/// <summary>
/// A manager for easy prefab pooling.
/// </summary>
public class PrefabPoolManager : MonoBehaviour
{
    /// <summary>
    /// Dictionary mapping of source prefab instance ID to the pool for instance of that prefab.
    /// </summary>
    private readonly Dictionary<int, PrefabPool> _pools = new Dictionary<int, PrefabPool>();

    /// <summary>
    /// Dictionary mapping the pooled object's instance IDs to source prefab's instance ID.
    /// </summary>
    private readonly Dictionary<int, int> _instanceToSourcePool = new Dictionary<int, int>();

    /// <summary>
    /// Dictionary mapping the pooled object's instance IDs to their associated IPoolables list.
    /// </summary>
    private readonly Dictionary<int, IPoolable[]> _instanceToPoolables = new Dictionary<int, IPoolable[]>();

    /// <summary>
    /// Pool parent
    /// </summary>
    private GameObject _poolParent;

    /// <summary>
    /// Return true if can start pooling
    /// </summary>
    public bool IsIntialized => _poolParent != null;

    public int NumFX { get; private set; }

    public static int MaxFXCount { get; private set; } = 100;

    public static bool Debug_DisablePrefabPools { get; set; }

    /// <summary>
    /// Initialize pools
    /// </summary>
    public void InitializePoolParent()
    {
        ClearAllPools();
        _poolParent = new GameObject("InactivePrefabPoolObjects");
    }

    /// <summary>
    /// unitialized pools
    /// </summary>
    public void ClearAllPools()
    {
        NumFX = 0;
        if (_poolParent)
        {
            GameObject.Destroy(_poolParent);
            _poolParent = null;
        }
        _pools.Clear();
        _instanceToSourcePool.Clear();
        _instanceToPoolables.Clear();
    }

    public bool IncrementFXCount(VFXPriority vFXPriority = VFXPriority.Default)
    {
        if (!IsIntialized)
        {
            return true;
        }

        if (vFXPriority != VFXPriority.High && NumFX >= MaxFXCount)
        {
            //Log.Debug("FX at max " + NumFX);
            return false;
        }
        NumFX++;
        //Log.Debug(NumFX);
        return true;
    }

    public void DecrimentFXCount()
    {
        if (NumFX > 0)
        {
            NumFX--;
        }

        //Log.Debug(NumFX);
    }

    /// <summary>
    /// Retrieves an instance from a managed prefab pool, optionally under a new parent.
    /// </summary>
    public GameObject Retrieve(GameObject prefab, Transform newParent = null)
    {
        if (!IsIntialized)
        {
            Debug.LogError("prefab pool manager is not initialized");
            return null;
        }

#if DEBUG
        if (Debug_DisablePrefabPools)
        {
            if (newParent != null)
            {
                return GameObject.Instantiate(prefab, newParent);
            }
            else
            {
                return GameObject.Instantiate(prefab);
            }
        }
#endif

        var pool = GetPool(prefab);
        GameObject obj;

        obj = pool.Peek();
        while (obj == null)
        {
            pool.Retrieve();
            Debug.LogError("Found a destroyed object in pool for " + prefab);
            obj = pool.Peek();
        }

        if (newParent)
        {
            obj.transform.SetParent(newParent);
        }
        obj = pool.Retrieve();

        return obj;
    }

    /// <summary>
    /// Releases an instance to a managed prefab pool.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (!IsIntialized)
        {
            Debug.LogWarning("prefab pool manager is not initialized");
            GameObject.Destroy(instance);
            return;
        }

        try
        {
#if DEBUG
            if (Debug_DisablePrefabPools)
            {
                GameObject.Destroy(instance);
                return;
            }
#endif
            var instanceId = instance.GetInstanceID();
            if (!_instanceToSourcePool.ContainsKey(instanceId))
            {
                GameObject.Destroy(instance);
                Debug.LogWarning($"Attempting to return {instance} to PrefabPool, but it is not tracked by the pool.");
                return;
            }
            var sourcePool = _pools[_instanceToSourcePool[instanceId]];
            if (sourcePool.Contains(instance))
            {
                Debug.LogWarning($"Attempting to return {instance} to PrefabPool, but it is already in the pool.");
                return;
            }
            sourcePool.Release(instance);
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception thrown while trying to release \"{instance.name}\" to PrefabPoolManager: {e}");
        }
    }

    public bool TryCreatePool(GameObject prefab, bool customReleaseLogic = false)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab));
        }

        if (_pools.ContainsKey(prefab.GetInstanceID()))
        {
            //Log.Warn("already have pool for prefab " + prefab.name);
            return false;
        }

        PrefabPool pool;
        if (customReleaseLogic)
        {
            pool = new PrefabPool(() => DefaultPoolFactory(prefab), OnRetrieveNoSetActive, OnReleaseNoSetActive);
        }
        else
        {
            pool = new PrefabPool(() => DefaultPoolFactory(prefab), OnRetrieve, OnRelease);
        }

        //Log.Debug("added pool " + prefab.name);
        _pools.Add(prefab.GetInstanceID(), pool);
        return true;
    }

    /// <summary>
    /// The default factory method for automatically created prefab pools.
    /// </summary>
    private GameObject DefaultPoolFactory(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        var poolables = instance.GetComponentsInChildren<IPoolable>();
        _instanceToSourcePool.Add(instance.GetInstanceID(), prefab.GetInstanceID());
        _instanceToPoolables.Add(instance.GetInstanceID(), poolables);
        return instance;
    }

    /// <summary>
    /// Gets pool by look-up or by creating one with a default factory if none exist.
    /// </summary>
    private PrefabPool GetPool(GameObject prefab)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab));
        }

        if (!_pools.TryGetValue(prefab.GetInstanceID(), out var pool))
        {
            pool = new PrefabPool(() => DefaultPoolFactory(prefab), OnRetrieve, OnRelease);
            _pools.Add(prefab.GetInstanceID(), pool);
        }
        return pool;
    }

    /// <summary>
    /// Callback for when an instance is retrieved from a prefab pool.
    /// </summary>
    private void OnRetrieve(GameObject instance)
    {
        instance.SetActive(true);
        var poolables = _instanceToPoolables[instance.GetInstanceID()];
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnRetrievedFromPool();
        }
    }

    /// <summary>
    /// Callback for when an instance is released to a prefab pool.
    /// </summary>
    private void OnRelease(GameObject instance)
    {
        var poolables = _instanceToPoolables[instance.GetInstanceID()];
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnReleasedToPool();
        }
        instance.SetActive(false);
        instance.transform.SetParent(_poolParent.transform);
    }

    /// <summary>
    /// Callback for when an instance is retrieved from a prefab pool.
    /// </summary>
    private void OnRetrieveNoSetActive(GameObject instance)
    {
        //instance.SetActive(true);
        var poolables = _instanceToPoolables[instance.GetInstanceID()];
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnRetrievedFromPool();
        }
    }

    /// <summary>
    /// Callback for when an instance is released to a prefab pool.
    /// </summary>
    private void OnReleaseNoSetActive(GameObject instance)
    {
        var poolables = _instanceToPoolables[instance.GetInstanceID()];
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnReleasedToPool();
        }
        //instance.SetActive(false);
        //instance.transform.SetParent(_poolParent.transform);
    }
}
