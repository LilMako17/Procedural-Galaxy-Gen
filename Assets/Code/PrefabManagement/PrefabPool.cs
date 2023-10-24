using System;
using UnityEngine;

/// <summary>
/// An Object pool specifically for GameObjects.
/// </summary>
internal class PrefabPool : ObjectPool<GameObject>
{
    /// <summary>
    /// Ctor that just calls the base class for now.
    /// </summary>
    public PrefabPool(Func<GameObject> factory, Action<GameObject> retrieveAction, Action<GameObject> releaseAction)
        : base(factory, retrieveAction, releaseAction) { }
}