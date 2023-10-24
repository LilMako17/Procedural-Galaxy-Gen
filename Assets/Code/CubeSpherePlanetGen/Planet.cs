using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CubeSphere
{
    public class Planet : MonoBehaviour
    {
        [Range(2, 256)]
        public int Resolution = 25;
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [OnValueChanged("OnValidate", true)]
        public PlanetShapeSettings ShapeSettings;
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [OnValueChanged("OnValidate", true)]
        public PlanetColorSettings ColorSettings;
        public int Seed;
        public float Scale = 1f;
        public Action<Planet> OnClick { get; set; }
        public SolarSystemObjectData Data { get; set; }

        // shape stuff
        private MeshFilter[] _meshFilters;
        private TerrainFace[] _terrainFaces;
        private const int _numSides = 6;
        private ShapeGenerator _shapeGenerator;

        // color stuff
        private Texture2D _texture;
        private const int TEXTURE_RES = 50;
        private NoiseGenerator3D _colorNoise;

        private MaterialPropertyBlock[] _matProps;


        private void Awake()
        {
            Cleanup();
            if (Application.isPlaying)
            {
                GeneratePlanet();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += GeneratePlanet;
#endif
            }
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += GeneratePlanet;
            }
#endif
        }

        private void OnMouseUpAsButton()
        {
            if (OnClick != null)
            {
                OnClick(this);
            }
        }

        public void GeneratePlanet()
        {
            if (ShapeSettings != null && ColorSettings != null)
            {
                Initialize(Seed, Scale);
                GenerateMesh();
                GenerateColor();
            }
        }

        public float GetNormalizedHeight(Vector3 worldPos)
        {
            var pointOnUnitSphere = worldPos.normalized;
            var height = _shapeGenerator.GetHeight(pointOnUnitSphere);
            return (height - _shapeGenerator.Min) / (_shapeGenerator.Max - _shapeGenerator.Min);
        }

        public float GetHeight(Vector3 worldPos)
        {
            var pointOnUnitSphere = worldPos.normalized;
            return _shapeGenerator.GetHeight(pointOnUnitSphere);
        }

        public void Cleanup()
        {
            var meshFilters = transform.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in meshFilters)
            {
                DestroyAsset(filter.gameObject);
            }
            _meshFilters = null;
            if (_texture != null)
            {
                DestroyAsset(_texture);
                _texture = null;
            }
        }

        private void DestroyAsset(UnityEngine.Object o)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(o);
            }
            else
            {
                DestroyImmediate(o);
            }
        }

        [Button("Redraw")]
        private void Redraw()
        {
            this.Cleanup();
            this.GeneratePlanet();
        }

        public void Initialize(int seed, float scale)
        {
            var collider = gameObject.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
            }
            collider.radius = scale;

            // note 6 sides
            Vector3[] faceDirections = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            if (_meshFilters?.Length != _numSides)
            {
                _meshFilters = new MeshFilter[_numSides];
            }
            if (_matProps?.Length != _numSides)
            {
                _matProps = new MaterialPropertyBlock[_numSides];
            }

            _terrainFaces = new TerrainFace[_numSides];
            _shapeGenerator = new ShapeGenerator(ShapeSettings, seed + 1, scale);
            _colorNoise = new NoiseGenerator3D(ColorSettings.BiomeColorSettings.Noise, seed);

            for (int i = 0; i < _numSides; i++)
            {
                if (_meshFilters[i] == null)
                {
                    var meshObj = new GameObject("side_" + i);
                    meshObj.transform.SetParent(transform, false);
                    meshObj.AddComponent<MeshRenderer>();
                    _meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    _meshFilters[i].sharedMesh = new Mesh();
                }

                _matProps[i] = new MaterialPropertyBlock();
                var r = _meshFilters[i].GetComponent<MeshRenderer>();
                r.sharedMaterial = ColorSettings.Material;
                r.GetPropertyBlock(_matProps[i]);
                _terrainFaces[i] = new TerrainFace(_shapeGenerator, _meshFilters[i].sharedMesh, Resolution, faceDirections[i]);
            }
        }

        public void GenerateMesh()
        {
            //UnityEngine.Debug.Log("generate mesh");
            foreach (var face in _terrainFaces)
            {
                face.ConstructMesh();
            }
        }

        public void GenerateColor()
        {
            GenerateGradientStrip();

            foreach (var face in _terrainFaces)
            {
                face.UpdateUVs(this);
            }

            for (var i = 0; i < _matProps.Length; i++)
            {
                var p = _matProps[i];

                p.SetTexture("_MainTex", _texture);
                p.SetFloat("_ElevationMin", _shapeGenerator.Min);
                p.SetFloat("_ElevationMax", _shapeGenerator.Max);

                var renderer = _meshFilters[i].GetComponent<MeshRenderer>();
                renderer.SetPropertyBlock(p);
            }
        }

        public Color EvaluateColor(Vector3 pointOnUnitSphere)
        {
            var height = GetNormalizedHeight(pointOnUnitSphere);
            return _texture.GetPixelBilinear(height, EvaluateBiomeForUVs(pointOnUnitSphere));
        }

        public float EvaluateBiomeForUVs(Vector3 posOnUnitSphere)
        {
            // return biome between 0 to 1 for shaders
            var biomeIndex = GetBiomeIndex(posOnUnitSphere);
            return biomeIndex / Mathf.Max(1, ColorSettings.BiomeColorSettings.Biomes.Length - 1f);
        }

        public float GetBiomeIndex(Vector3 posOnUnitSphere)
        {
            // where are we in regards to up/down?
            var yVal = (posOnUnitSphere.y + 1) / 2f;
            // randomize with noise
            yVal += (_colorNoise.GetUncappedHeight(posOnUnitSphere) - ColorSettings.BiomeColorSettings.NoiseOffset) * ColorSettings.BiomeColorSettings.NoiseStrength;
            var biomes = ColorSettings.BiomeColorSettings.Biomes;
            Biome biome = null;
            float blendRange = ColorSettings.BiomeColorSettings.BlendAmount / 2f + 0.001f; // ensure not 0

            var biomeIndex = 0f;
            for (int i = 0; i < biomes.Length; i++)
            {
                biome = biomes[i];
                var dist = yVal - biome.StartHeight;
                // blend based on distance to split
                var weight = Mathf.InverseLerp(-blendRange, blendRange, dist);
                /*if (yVal >= biome.StartHeight)
                {
                    return i;
                }*/
                biomeIndex *= (1 - weight);
                biomeIndex += i * weight;
            }

            return biomeIndex;
        }

        private Color GetColorForNormaizedHeight(Biome biome, float normalizedVal)
        {
            Color color = biome.Gradient.Evaluate(normalizedVal);

            var tint = biome.Tint;
            return color * (1f - biome.TintPercent) + biome.Tint * biome.TintPercent;
        }

        private void GenerateGradientStrip()
        {
            var biomes = ColorSettings.BiomeColorSettings.Biomes;
            var texHeight = biomes.Length;

            if (_texture == null || _texture.height != texHeight)
            {
                _texture = new Texture2D(TEXTURE_RES, texHeight, TextureFormat.RGBA32, false);
                _texture.wrapMode = TextureWrapMode.Clamp;
            }

            var colors = new Color[TEXTURE_RES * texHeight];
            var index = 0;
            foreach (var biome in biomes)
            {
                for (int i = 0; i < TEXTURE_RES; i++)
                {
                    colors[index] = GetColorForNormaizedHeight(biome, i / (TEXTURE_RES - 1f));
                    index++;
                }
            }
            _texture.SetPixels(colors);
            _texture.Apply();
        }
    }
}
