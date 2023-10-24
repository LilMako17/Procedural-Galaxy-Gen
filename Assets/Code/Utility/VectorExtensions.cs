using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    /// <summary>
    /// Returns the same vector with no Y value
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 x0z(this Vector3 vec)
    {
        return new Vector3(vec.x, 0, vec.z);
    }

    public static Vector2 xz(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    /// <summary>
    /// Rotate a point around a pivot
    /// </summary>
    /// <param name="point"></param>
    /// <param name="pivot"></param>
    /// <param name="angles"></param>
    /// <returns></returns>
    public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
}
