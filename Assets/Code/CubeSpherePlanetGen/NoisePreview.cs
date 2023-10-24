using UnityEngine;
using System.Collections;
using CubeSphere;

public class NoisePreview : MonoBehaviour
{
    private Texture2D _tex;
    public int Size = 256;

    // Use this for initialization
    void Start()
    {
        _tex = new Texture2D(Size, Size);
        _tex.filterMode = FilterMode.Point;
        _tex.wrapMode = TextureWrapMode.Repeat;
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = _tex;
    }

    public void UpdatePos(PlanetCursor cursor)
    {
        var center = cursor.CurrentPos;
        var xAxis = -cursor.GetXAxis(center);
        var yAxis = cursor.GetYAxis(center);

        var colors = new Color[Size * Size];
        for (int i = 0; i < colors.Length; i++)
        {
            var y = i / Size;
            var x = i % Size;
            var xNormalized = (x / (float)(Size - 1)) - 0.5f; // (-0.5 to 0.5)
            var yNormalized = (y / (float)(Size - 1)) - 0.5f; // (-0.5 to 0.5)

            var x3d = xNormalized * xAxis * cursor.ProjectionWidth;
            var y3d = yNormalized * yAxis * cursor.ProjectionHeight;
            var localPoint3D = x3d + y3d + center;

            colors[i] = cursor.Planet.EvaluateColor(localPoint3D.normalized);
        }

        _tex.SetPixels(colors);
        _tex.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = _tex;
    }
}
