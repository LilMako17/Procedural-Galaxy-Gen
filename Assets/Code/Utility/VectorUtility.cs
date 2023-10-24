using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Utility methods for Unity's Vector objects.
/// </summary>
public static class VectorUtility
{
    /// <summary>
    /// Returns true if two vectors are approximately equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(Vector3 a, Vector3 b, float epsilon = 1.0e-4f)
    {
        return (a - b).sqrMagnitude < epsilon;
    }

    /// <summary>
    /// Remap01 for Unity's Vector2 class.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Remap01(Vector2 low, Vector2 high, Vector2 v)
    {
        return new Vector2(MathUtility.Remap01(low.x, high.x, v.x), MathUtility.Remap01(low.y, high.y, v.y));
    }

    /// <summary>
    /// Packs the x y coordinates of a Vector into a single float with 12 bits of precision.
    /// </summary>
    public static float EncodeXYToSingle(this Vector2 v)
    {
        const int PRECISION = (1 << 12) - 1;
        var x = Mathf.FloorToInt(v.x * PRECISION);
        var y = Mathf.FloorToInt(v.y * PRECISION) << 12;
        return x + y;
    }
}
