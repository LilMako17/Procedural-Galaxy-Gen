using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeSphere
{
    // noise manager
    public class NoiseGenerator3D
    {
        public FastNoise Noise { get { return _noise; } }

        private NoiseFilter _settings;
        private FastNoise _noise;

        public NoiseGenerator3D(NoiseFilter settings, int seed = 1337)
        {
            _settings = settings;
            _noise = new FastNoise(seed);
            // set global granularity
            _noise.SetFrequency(settings.Frequency);
        }

        // returns a noise value for a 3D normalized point using current settings
        public float GetUncappedHeight(Vector3 pointOnUnitSphere)
        {
            var firstValue = 0f;
            var noiseValue = 0f;

            // cache out first noise value to use for masking purposes
            if (_settings.NoiseLayers.Length > 0)
            {
                firstValue = CalculateNoise(pointOnUnitSphere, _settings.NoiseLayers[0], 0f);
                if (_settings.NoiseLayers[0].Enabled)
                {
                    noiseValue = firstValue;
                }
            }

            // add in the rest of the noise layers
            for (int i = 1; i < _settings.NoiseLayers.Length; i++)
            {
                var layer = _settings.NoiseLayers[i];
                noiseValue += CalculateNoise(pointOnUnitSphere, layer, firstValue);
            }

            return noiseValue;
        }

        private float CalculateNoise(Vector3 pointOnUnitSphere, NoiseLayer layer, float maskValue)
        {
            if (!layer.Enabled)
            {
                return 0f;
            }

            float noise;

            switch (layer.Type)
            {
                case FilterType.Rigid:
                    {
                        noise = CalculateRigidNoise(pointOnUnitSphere, layer.NoiseSettings);
                        break;
                    }
                case FilterType.Simple:
                    {
                        noise = CalculateSimpleNoise(pointOnUnitSphere, layer.NoiseSettings);
                        break;
                    }
                case FilterType.Fractal:
                    {
                        noise = CalculateFractalNoise(pointOnUnitSphere, layer.NoiseSettings);
                        break;
                    }
                default:
                    {
                        throw new Exception(layer.Type + " not implemented");
                    }
            }

            if (layer.UseFirstLayerAsMask)
            {
                return noise * maskValue;
            }

            return noise;
        }

        private float CalculateSimpleNoise(Vector3 pointOnUnitSphere, NoiseSettings settings)
        {
            var noiseValue = 0f;
            var frequency = settings.BaseRoughness;
            var amplitude = 1f;

            for (int i = 0; i < settings.NumLayers; i++)
            {
                var v = GetSimplex(
                    (pointOnUnitSphere.x + settings.Center.x) * frequency,
                    (pointOnUnitSphere.y + settings.Center.y) * frequency,
                    (pointOnUnitSphere.z + settings.Center.z) * frequency);

                // normalize from (-1 to 1) to (0 to 1) before applying amplitude
                noiseValue += (v + 1f) / 2f * amplitude;

                // intensify frequency as we go up layers
                frequency *= settings.Roughness;
                amplitude *= settings.Persitence;
            }

            return ApplyCommonSettings(noiseValue, settings);
        }

        private float CalculateRigidNoise(Vector3 pointOnUnitSphere, NoiseSettings settings)
        {
            var noiseValue = 0f;
            var frequency = settings.BaseRoughness;
            var amplitude = 1f;
            var weight = 1f;

            for (int i = 0; i < settings.NumLayers; i++)
            {
                var v = GetSimplex(
                    (pointOnUnitSphere.x + settings.Center.x) * frequency,
                    (pointOnUnitSphere.y + settings.Center.y) * frequency,
                    (pointOnUnitSphere.z + settings.Center.z) * frequency);

                // v is range -1 to 1, get the inverse magnitude squared
                v = 1f - Mathf.Abs(v);
                v *= v;
                v *= weight;
                weight = v;

                noiseValue += v * amplitude;
                frequency *= settings.Roughness;
                amplitude *= settings.Persitence;
            }

            return ApplyCommonSettings(noiseValue, settings);
        }

        private float CalculateFractalNoise(Vector3 pointOnUnitSphere, NoiseSettings settings)
        {
            var noiseValue = 0f;
            var frequency = settings.BaseRoughness;
            var amplitude = 1f;

            for (int i = 0; i < settings.NumLayers; i++)
            {
                var v = _noise.GetSimplexFractal(
                    (pointOnUnitSphere.x + settings.Center.x) * frequency,
                    (pointOnUnitSphere.y + settings.Center.y) * frequency,
                    (pointOnUnitSphere.z + settings.Center.z) * frequency);

                // normalize from (-1 to 1) to (0 to 1) before applying amplitude
                noiseValue += (v + 1f) / 2f * amplitude;

                frequency *= settings.Roughness;
                amplitude *= settings.Persitence;
            }

            return ApplyCommonSettings(noiseValue, settings);
        }

        private float ApplyCommonSettings(float noiseValue, NoiseSettings settings)
        {
            noiseValue = (noiseValue + settings.OffsetValue) * settings.MultiplierValue;
            if (settings.UseMin && noiseValue < settings.MinValue)
            {
                noiseValue = settings.MinValue;
            }
            if (settings.UseMax && noiseValue > settings.MaxValue)
            {
                noiseValue = settings.MaxValue;
            }

            return noiseValue;
        }

        private float GetSimplex(float x, float y, float z)
        {
            return _noise.GetSimplex(x, y, z);
        }
    }
}
