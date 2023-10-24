/// <summary>
/// An interface for defining components that need to be notified when their attached GameObject is retrieved/released
/// from a prefab pool.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// A method called when the GameObject the implementing component is attached to is retrieved from a prefab pool.
    /// </summary>
    void OnRetrievedFromPool();

    /// <summary>
    /// A method called when the GameObject the implementing component is attached to is released to a prefab pool.
    /// </summary>
    void OnReleasedToPool();
}
