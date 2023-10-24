using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CubeSphere
{
    public class TerrainFace
    {
        private Mesh _mesh;
        private int _resolution;
        private Vector3 _localUp;
        private Vector3 _axisA;
        private Vector3 _axisB;
        private ShapeGenerator _shapeGenerator;

        public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
        {
            _mesh = mesh;
            _resolution = resolution;
            _localUp = localUp;
            _shapeGenerator = shapeGenerator;

            _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            _axisB = Vector3.Cross(localUp, _axisA);
        }

        public void UpdateUVs(Planet planet)
        {
            Vector2[] uv = new Vector2[_resolution * _resolution];
            for (int y = 0; y < _resolution; y++)
            {
                for (int x = 0; x < _resolution; x++)
                {
                    var i = x + y * _resolution;
                    var posOnUnitSphere = PosOnUnitSphereFromCube(x, y);
                    var biomeVal = planet.EvaluateBiomeForUVs(posOnUnitSphere);
                    uv[i] = new Vector2(biomeVal, 0f);
                }
            }

            _mesh.uv = uv;
        }

        public Vector3 PosOnUnitSphereFromCube(int x, int y)
        {
            var percent = new Vector2(x, y) / (_resolution - 1);
            var posOnCube = _localUp + (percent.x - 0.5f) * 2f * _axisA + (percent.y - 0.5f) * 2f * _axisB;
            return posOnCube.normalized;
        }

        public void ConstructMesh()
        {
            var verts = new Vector3[_resolution * _resolution];
            var tris = new int[(_resolution - 1) * (_resolution - 1) * 6];
            var triIndex = 0;
            var uvs = _mesh.uv;
            if (uvs.Length != verts.Length)
            {
                uvs = new Vector2[verts.Length];
            }

            for (int y = 0; y < _resolution; y++)
            {
                for (int x = 0; x < _resolution; x++)
                {
                    var i = x + y * _resolution;
                    var posOnUnitSphere = PosOnUnitSphereFromCube(x, y);
                    verts[i] = _shapeGenerator.CalculatePointOnPlanet(posOnUnitSphere);
                    
                    // create quad
                    if (x != _resolution - 1 && y != _resolution - 1)
                    {
                        // first tri
                        tris[triIndex + 0] = i;
                        tris[triIndex + 1] = i + _resolution + 1;
                        tris[triIndex + 2] = i + _resolution;

                        // second tri
                        tris[triIndex + 3] = i;
                        tris[triIndex + 4] = i + 1;
                        tris[triIndex + 5] = i + _resolution + 1;
                        triIndex += 6;
                    }
                }
            }

            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.triangles = tris;
            _mesh.uv = uvs;
            _mesh.RecalculateNormals();
        }
    }
}
