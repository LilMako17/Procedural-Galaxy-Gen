using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeSphere
{
    public class ShapeGenerator
    {
        private PlanetShapeSettings _settings;
        private NoiseGenerator3D _noise;

        public float Min { get; private set; }
        public float Max { get; private set; }

        private float _scale;

        public ShapeGenerator(PlanetShapeSettings shapeSettings, int seed, float scale)
        {
            _scale = scale;
            _settings = shapeSettings;
            _noise = new NoiseGenerator3D(shapeSettings.NoiseLayers, seed);
            Min = float.MaxValue;
            Max = float.MinValue;
        }

        public float GetHeight(Vector3 pointOnUnitSphere)
        {
            var noiseValue = _noise.GetUncappedHeight(pointOnUnitSphere);
            var clampedValue = (1 + noiseValue);
            var val = _settings.Radius * clampedValue * _scale;

            UpdateMinMax(val);
            return val;
        }

        public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
        {
            var val = GetHeight(pointOnUnitSphere);
            return pointOnUnitSphere * val;
        }

        private void UpdateMinMax(float v)
        {
            if (v > Max)
            {
                Max = v;
            }
            if (v < Min)
            {
                Min = v;
            }
        }
    }
}
