using UnityEngine;

/// <summary>
/// Utility methods for Unity's Color object.
/// </summary>
public static class ColorUtility
{
    /// <summary>
    /// Pack 4 low-precision [0-1] floats values to a float.
    /// Each value [0-1] has 64 steps(6 bits).
    /// </summary>
    public static float EncodeRGBAToSingle(this Color color)
    {
        const int PRECISION = (1 << 6) - 1;

        var r = Mathf.FloorToInt(color.r * PRECISION);
        var g = Mathf.FloorToInt(color.g * PRECISION) << 6;
        var b = Mathf.FloorToInt(color.b * PRECISION) << 12;
        var a = Mathf.FloorToInt(color.a * PRECISION) << 18;

        return r + g + b + a;
    }

    /// <summary>
	/// Pack 3 low-precision [0-1] floats values to a float.
	/// Each value [0-1] has 256 steps(8 bits).
	/// </summary>
	public static float EncodeRGBToSingle(this Color color)
    {
        const int PRECISION = (1 << 8) - 1;

        var r = Mathf.FloorToInt(color.r * PRECISION);
        var g = Mathf.FloorToInt(color.g * PRECISION) << 8;
        var b = Mathf.FloorToInt(color.b * PRECISION) << 16;

        return r + g + b;
    }

    /// <summary>
    /// Returns a new Vector4 from a Color (clamped to LDR unless opting for HDR).
    /// </summary>
    public static Vector4 ToVector4(this Color color)
    {
        color = color.ClampLDR();
        return new Vector4(color.r, color.g, color.b, color.a);
    }

    /// <summary>
    /// Returns a new color clamped to LDR.
    /// </summary>
    public static Color ClampLDR(this Color color)
    {
        return new Color(
            Mathf.Clamp01(color.r), Mathf.Clamp01(color.g),
            Mathf.Clamp01(color.b), Mathf.Clamp01(color.a));
    }
}

