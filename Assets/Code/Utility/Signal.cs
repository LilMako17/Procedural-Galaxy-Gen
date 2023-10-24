using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// The base for all signal implementations.
/// </summary>
public class BaseSignal<TAction>
{
    /// <summary>
    /// The list of actions to invoke when the signal is invoked.
    /// </summary>
    protected List<TAction> _listeners = new List<TAction>();

    /// <summary>
    /// The list of actions to invoke when the signal is next invoked, then the list is cleared.
    /// </summary>
    protected List<TAction> _oneShotListeners = new List<TAction>();

    /// <summary>
    /// A lock object used for thread safety.
    /// </summary>
    protected object _threadLock = new object();

    /// <summary>
    /// Adds a listener to the signal.
    /// </summary>
    public void AddListener(TAction listener)
    {
        lock (_threadLock)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }
    }

    /// <summary>
    /// Adds a listener to the signal that is only called once on the next invocation.
    /// </summary>
    public void AddListenerOneShot(TAction listener)
    {
        lock (_threadLock)
        {
            if (!_oneShotListeners.Contains(listener))
            {
                _oneShotListeners.Add(listener);
            }
        }
    }

    /// <summary>
    /// Removes a listner (even one that is only added as oneshot).
    /// </summary>
    public void RemoveListener(TAction listener)
    {
        lock (_threadLock)
        {
            _listeners.Remove(listener);
            _oneShotListeners.Remove(listener);
        }
    }

    public void ClearListeners()
    {
        _listeners.Clear();
    }

    /// <summary>
    /// Returns number of listeners
    /// </summary>
    public int GetNumberOfListeners()
    {
        return _listeners.Count;
    }
}

/// <summary>
/// An object that provides thread-safe Delegate-like functionality with less allocation.
/// </summary>
public class Signal : BaseSignal<Action>
{
    /// <summary>
    /// Invokes all the listners of the singal.
    /// </summary>
    public void Invoke()
    {
        lock (_threadLock)
        {
            var copy = ListPool<Action>.Retrieve(_listeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            copy.Clear();
            copy.AddRange(_oneShotListeners);
            _oneShotListeners.Clear(); //Clear one shot listeners first or else if new one shots are added they will be cleared
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke();
                }
                catch(Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }

            ListPool<Action>.Release(copy);
        }
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit delegate signatures.
    /// </summary>
    public static implicit operator Action(Signal signal)
    {
        return signal.Invoke;
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit Unity's delegate signatures.
    /// </summary>
    public static implicit operator UnityAction(Signal signal)
    {
        return signal.Invoke;
    }
}

/// <summary>
/// A generic version of Signal that takes a single parameter.
/// </summary>
public class Signal<T0> : BaseSignal<Action<T0>>
{
    /// <summary>
    /// Invokes all the listners of the singal.
    /// </summary>
    public void Invoke(T0 t0)
    {
        lock (_threadLock)
        {
            var copy = ListPool<Action<T0>>.Retrieve(_listeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            copy.Clear();
            copy.AddRange(_oneShotListeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            _oneShotListeners.Clear();
            ListPool<Action<T0>>.Release(copy);
        }
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit delegate signatures.
    /// </summary>
    public static implicit operator Action<T0>(Signal<T0> signal)
    {
        return signal.Invoke;
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit Unity's delegate signatures.
    /// </summary>
    public static implicit operator UnityAction<T0>(Signal<T0> signal)
    {
        return signal.Invoke;
    }
}

/// <summary>
/// A generic version of Signal that takes two parameters.
/// </summary>
public class Signal<T0, T1> : BaseSignal<Action<T0, T1>>
{
    /// <summary>
    /// Invokes all the listners of the singal.
    /// </summary>
    public void Invoke(T0 t0, T1 t1)
    {
        lock (_threadLock)
        {
            var copy = ListPool<Action<T0, T1>>.Retrieve(_listeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0, t1);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            copy.Clear();
            copy.AddRange(_oneShotListeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0, t1);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            _oneShotListeners.Clear();
            ListPool<Action<T0, T1>>.Release(copy);
        }
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit delegate signatures.
    /// </summary>
    public static implicit operator Action<T0, T1>(Signal<T0, T1> signal)
    {
        return signal.Invoke;
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit Unity's delegate signatures.
    /// </summary>
    public static implicit operator UnityAction<T0, T1>(Signal<T0, T1> signal)
    {
        return signal.Invoke;
    }
}

/// <summary>
/// A generic version of Signal that takes three parameters.
/// </summary>
public class Signal<T0, T1, T2> : BaseSignal<Action<T0, T1, T2>>
{
    /// <summary>
    /// Invokes all the listners of the singal.
    /// </summary>
    public void Invoke(T0 t0, T1 t1, T2 t2)
    {
        lock (_threadLock)
        {
            var copy = ListPool<Action<T0, T1, T2>>.Retrieve(_listeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0, t1, t2);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            copy.Clear();
            copy.AddRange(_oneShotListeners);
            foreach (var listener in copy)
            {
                try
                {
                    listener.Invoke(t0, t1, t2);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception in signal listener: " + ex.ToString());
                }
            }
            _oneShotListeners.Clear();
            ListPool<Action<T0, T1, T2>>.Release(copy);
        }
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit delegate signatures.
    /// </summary>
    public static implicit operator Action<T0, T1, T2>(Signal<T0, T1, T2> signal)
    {
        return signal.Invoke;
    }

    /// <summary>
    /// Implicit cast so Signals can more naturally fit Unity's delegate signatures.
    /// </summary>
    public static implicit operator UnityAction<T0, T1, T2>(Signal<T0, T1, T2> signal)
    {
        return signal.Invoke;
    }
}
