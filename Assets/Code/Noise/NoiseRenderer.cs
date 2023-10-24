using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(NoiseRenderer))]
public class NoiseRenderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Redraw"))
        {
            var upcast = target as NoiseRenderer;
            if (upcast != null)
            {
                upcast.GenerateNoise();
            }
        }
    }
}
#endif

public class NoiseRenderer : MonoBehaviour
{
    public GameObject PreviewRenderer;
    public int Size;
    public int Seed;
    public float Frequency = 0.02f;
    public float ScaleX = 1f;
    public float ScaleY = 1f;
    public float OffsetX = 0f;
    public float OffsetY = 0f;

    public int Octaves = 1;
    public float Persistance = 1f;
    public float Lacunarity = 1f;
    public bool Use4D;

    private Texture2D _tex;

    public void GenerateNoise()
    {
        var rawNoise = GenerateRawNoise();
        var pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = rawNoise[i];

            pixels[i] = new Color(p, p, p, 1f);
        }

        CreatePreview(PreviewRenderer, Size, pixels);
    }

    private float[] GenerateRawNoise()
    {
        var min = 1f;
        var max = 0f;
        var noise = new FastNoise(Seed);
        noise.SetFrequency(Frequency);
        var pixels = new float[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
        {
            var y = i / Size;
            var x = i % Size;

            float p;
            if (Use4D)
            {
                var s = x / (float)Size;
                var t = y / (float)Size;
                var dx = Size * (ScaleX - OffsetX);
                var dy = Size * (ScaleY - OffsetY);
                var nx = (OffsetX * Size) + Mathf.Cos(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                var ny = (OffsetY * Size) + Mathf.Cos(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                var nz = (OffsetX * Size) + Mathf.Sin(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                var nw = (OffsetY * Size) + Mathf.Sin(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                p = noise.GetSimplex(nx, ny, nz, nw); // -1 to 1
                p = (p + 1f) / 2f;
            }
            else
            {
                p = noise.GetPerlin(x, y);
            }

            if (p < min)
            {
                min = p;
            }
            if (p > max)
            {
                max = p;
            }

            pixels[i] = p;
        }

        return pixels;
    }

    public void CreatePreview(GameObject plane, int size, Color[] pixels)
    {
        if (_tex != null)
        {
            DestroyImmediate(_tex);
        }
        _tex = new Texture2D(size, size);
        _tex.filterMode = FilterMode.Point;
        _tex.wrapMode = TextureWrapMode.Repeat;
        _tex.SetPixels(pixels);

        plane.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = _tex;
    }
}
