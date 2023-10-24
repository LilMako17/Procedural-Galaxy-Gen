using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GalaxyGenerator
{
    public static GalaxyGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GalaxyGenerator();
            }

            return _instance;
        }
    }

    private static GalaxyGenerator _instance;

    private int[,,] debug_lastGrid;
    // -2  ==  no spawn from mask
    // -1  ==  no spawn from obstical
    //  0  ==  empty
    //  1+ Id of object
    private Dictionary<int, StarNode> debug_lastGenerated;
    private GalaxySettings _settings;
    private int _randomSeed;

    public Dictionary<int, StarNode> GetStarData()
    {
        return debug_lastGenerated;
    }

    public int[,,] Debug_GetLastGrid()
    {
        return debug_lastGrid;
    }

    public void Init(GalaxySettings settings, int seed)
    {
        _settings = settings;
        _randomSeed = seed;
    }
  
    private Color GetPixel(int xMax, int yMax, Texture2D tex, int x, int y, Vector2 currentUvs)
    {
        if (tex != null)
        {
            if (tex.isReadable)
            {
                var width = tex.width * currentUvs.x;
                var height = tex.height * currentUvs.y;

                var xNormalized = 1 - ((float)x / xMax);
                var yNormalized = 1 - ((float)y / yMax);

                var xPixel = (int)((xNormalized * width) % tex.width);
                var yPixel = (int)((yNormalized * height) % tex.height);
                var color = tex.GetPixel(xPixel, yPixel);
                return color;
            }
            else
            {
                UnityEngine.Debug.LogError("cannot read scatter mask as its not marked as read/write in settings");
            }
        }

        return UnityEngine.Color.white;
    }

    private float GetScatterDistanceMultiplier(int xMax, int yMax, Texture2D tex, int x, int y, Vector2 currentUvs)
    {
        var color = GetPixel(xMax, yMax, tex, x, y, currentUvs);
        return 1 - color.grayscale;
    }

    private int GetPropIndex(int xMax, int yMax, int x, int y, Vector2 currentUvs)
    {
        if (_settings.StarOptions.Count < 2)
        {
            return 0;
        }

        var total = 0f;
        var chanceTable = ListPool<float>.Retrieve();
        for (int i = 0; i < _settings.StarOptions.Count; i++)
        {
            var distance = Vector2.Distance(new Vector2(x, y), new Vector2(xMax / 2f, yMax / 2f)) / (Mathf.Min(xMax, yMax) / 2f);
            if (distance < _settings.StarOptions[i].DistributionMinDistanceFromCenter || distance > _settings.StarOptions[i].DistributionMaxDistanceFromCenter)
            {
                chanceTable.Add(0);
            }
            else
            {
                var color = GetPixel(xMax, yMax, _settings.StarOptions[i].DistributionMask, x, y, currentUvs);
                chanceTable.Add(color.grayscale);
                total += color.grayscale;
            }
        }

        int index = 0;
        var rand = new System.Random(_randomSeed + (x * 100) + y);
        var r = (float)rand.NextDouble() * total;
        for (var i = 0; i < chanceTable.Count; i++)
        {
            r -= chanceTable[i];
            if (r < 0)
            {
                index = i;
                break;
            }
        }

        ListPool<float>.Release(chanceTable);
        return index;
    }


    public SerializedGalaxyData GenerateNew(GalaxySettings settings, int seed)
    {
        Init(settings, seed);

        var boundsArea = new Vector3(_settings.Bounds.x, _settings.Bounds.y, _settings.Bounds.z);
        var totalArea = (_settings.Bounds.x * _settings.Bounds.z);
        var random = new System.Random(seed);
        const int numSpreadAttemptsPerPoint = 24;
        var numReseedAttempts = Mathf.Min(150, (int)(30 / 728f * totalArea));
        var cellSize = 1; //Mathf.Clamp(propData.SampleDensity, 1f, Mathf.Min(_bounds.size.x, _bounds.size.z));
        var points = new Dictionary<int, StarNode>();

        // Poisson Disc Sampling -- adapted from https://www.youtube.com/watch?v=7WcmyxyFO7o
        int[,,] grid = new int[Mathf.CeilToInt(boundsArea.x / cellSize), Mathf.CeilToInt(boundsArea.y / cellSize), Mathf.CeilToInt(boundsArea.z / cellSize)];

        var xMax = grid.GetLength(0);
        var yMax = grid.GetLength(1);
        var zMax = grid.GetLength(2);

        //Log.Debug("grid size " + grid.GetLength(0) + ", " + grid.GetLength(1)+" total: "+(grid.GetLength(0) * grid.GetLength(1)));

        // initialize grid with scatter mask dead zones
        if (_settings.GlobalShapeMask != null)
        {
            if (_settings.GlobalShapeMask.isReadable)
            {
                //var floorRenderer = _floorRootObject.GetComponent<MeshRenderer>();
                //var currentUvs = floorRenderer.material.GetTextureScale("_MainTex");
                var currentUvs = new Vector2(1, 1);


                for (int x = 0; x < xMax; x++)
                {
                    for (int y = 0; y < yMax; y++)
                    {
                        for (int z = 0; z < zMax; z++)
                        {
                            var width = _settings.GlobalShapeMask.width * currentUvs.x;
                            var height = _settings.GlobalShapeMask.height * currentUvs.y;

                            var xNormalized = 1 - ((float)x / xMax);
                            var yNormalized = 1 - ((float)z / zMax);
                            var halfDepthNormalized = Mathf.Abs(0.5f - ((float)y / yMax)) * 2;
                            var distance = (Vector2.Distance(new Vector2(xNormalized, yNormalized), new Vector2(0.5f, 0.5f)) * 2);
                            var depthCutoff = _settings.YHeightCutOffCurve.Evaluate(distance);

                            var xPixel = (int)((xNormalized * width) % _settings.GlobalShapeMask.width);
                            var yPixel = (int)((yNormalized * height) % _settings.GlobalShapeMask.height);
                            var color = _settings.GlobalShapeMask.GetPixel(xPixel, yPixel);
                            var grayscale = color.grayscale;
                            if (grayscale <= _settings.MaskCutoffThreshold || halfDepthNormalized > depthCutoff)
                            {
                                grid[x, y, z] = -101 + (int)(grayscale * 100);
                            }
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("cannot read scatter mask as its not marked as read/write in settings");
            }
        }

        var thisPropNumCreated = 0;
        var areaMultiplier = totalArea / 145f;
        var maxItems = _settings.MaxObjects * areaMultiplier;
        //Log.Debug(totalArea + " max objects " + _maxObjects + " * " + areaMultiplier+" = "+maxItems);

        var workingSet = new List<StarNode>();

        var numFailedReseeds = 0;

        // start seed with center of the area
        workingSet.Add(new StarNode() { Pos = new Vector3(boundsArea.x / 2f, boundsArea.y / 2f, boundsArea.z / 2f), InstanceIndex = -1 });

        while (workingSet.Count > 0 && thisPropNumCreated < maxItems)
        {
            var success = false;
            var index = random.Next(0, workingSet.Count);
            var existingPoint = workingSet[index];

            for (int i = 0; i < numSpreadAttemptsPerPoint; i++)
            {
                var newInstanceIndex = _randomSeed + points.Count;

                // 2D random angle
                //var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                //var dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

                // 3D random angle
                var u = (float)random.NextDouble();
                var v = (float)random.NextDouble();
                var theta = 2 * Mathf.PI * u;
                var phi = Mathf.Acos(2 * v - 1);
                var x = (Mathf.Sin(phi) * Mathf.Cos(theta));
                var y = (Mathf.Sin(phi) * Mathf.Sin(theta));
                var z = (Mathf.Cos(phi));
                var dir = new Vector3(x, y, z);

                var xCoord = (int)(existingPoint.Pos.x / cellSize);
                var yCoord = (int)(existingPoint.Pos.y / cellSize);
                var zCoord = (int)(existingPoint.Pos.z / cellSize);
                var scatterDistanceMultiplier = GetScatterDistanceMultiplier(xMax, zMax, _settings.GlobalShapeMask, xCoord, zCoord, Vector2.one);
                var scatterDistance = _settings.MinScatterDistance + ((_settings.MaxScatterDistance - _settings.MinScatterDistance) * scatterDistanceMultiplier);

                var propIndex = GetPropIndex(xMax, zMax, xCoord, zCoord, Vector2.one);
                if (propIndex < 0)
                {
                    break;
                }

                var propData = _settings.StarOptions[propIndex];
                var scale = ((float)random.NextDouble() * (propData.RandomScale.y - propData.RandomScale.x)) + propData.RandomScale.x;
                var multipliedRadius = propData.Radius * scale;
                var distance = scatterDistance + multipliedRadius + existingPoint.MultipliedRadius;
                var point = existingPoint.Pos + (dir * distance);
                if (IsValidPoint(existingPoint.Pos, point, boundsArea, cellSize, multipliedRadius, points, grid))
                {
                    thisPropNumCreated++;
                    var data = new StarNode()
                    {
                        InstanceIndex = newInstanceIndex,
                        Name = NameGeneratorManager.Instance.GenerateStarName(newInstanceIndex),
                        AssetIndex = propIndex,
                        Pos = point,
                        Scale = scale,
                        MultipliedRadius = multipliedRadius,
                        ScatterDistance = scatterDistance,
                        Rotation = (float)(random.NextDouble() * (propData.RandomRotation.y - propData.RandomRotation.x)) + propData.RandomRotation.x,
                        Connections = new HashSet<int>(),
                    };
                    points.Add(newInstanceIndex, data);
                    workingSet.Add(data);
                    grid[(int)(point.x / cellSize), (int)(point.y / cellSize), (int)(point.z / cellSize)] = newInstanceIndex + 1; // adding plus one so we are 1-based. so zero means 'unfilled'
                    success = true;

                    // don't add connections for dummy points
                    if (existingPoint.InstanceIndex >= 0)
                    {
                        ConnectPoints(points, newInstanceIndex, existingPoint.InstanceIndex);
                        if (data.Connections.Count >= propData.MaxConnections)
                        {
                            workingSet.Remove(data);
                        }
                        if (existingPoint.Connections.Count >= _settings.StarOptions[existingPoint.AssetIndex].MaxConnections)
                        {
                            workingSet.Remove(existingPoint);
                        }
                    }
                    else
                    {
                        workingSet.Remove(existingPoint);
                    }

                    break;
                }
            }

            if (!success)
            {
                workingSet.RemoveAt(index);
            }
            else
            {
                // delay call here if want to make async
                //debug_lastGenerated = points;
                //debug_lastGrid = grid;
                //yield return null;
            }

            // if we empty the working set before hitting a decent amount of objects, try again from random point
            if (workingSet.Count == 0 && numFailedReseeds < numReseedAttempts && thisPropNumCreated < maxItems)
            {
                numFailedReseeds++;
                var randomX = (float)random.NextDouble() * boundsArea.x;
                var randomY = (float)random.NextDouble() * boundsArea.y;
                var randomZ = (float)random.NextDouble() * boundsArea.z;
                workingSet.Add(new StarNode() { Pos = new Vector3(randomX, boundsArea.y / 2f, randomZ), InstanceIndex = -1 });
            }
        }

        UnityEngine.Debug.Log("used " + numFailedReseeds + " / " + numReseedAttempts + " reseed attempts. created " + thisPropNumCreated + " / " + maxItems);

        // remove orphaned entries
        if (_settings.RemoveOrphans)
        {
            var cloneList = ListPool<int>.Retrieve(points.Keys);
            foreach (var id in cloneList)
            {
                if (points[id].Connections.Count == 0)
                {
                    points.Remove(id);
                }
            }
            ListPool<int>.Release(cloneList);
        }

        debug_lastGenerated = points;
        debug_lastGrid = grid;

        return new SerializedGalaxyData()
        {
            Seed = seed,
            StarMap = points,
            Name = NameGeneratorManager.Instance.GenerateGalaxyName(seed)
        };
    }

    private void ConnectPoints(Dictionary<int, StarNode> points, int index1, int index2)
    {
        points[index1].Connections.Add(index2);
        points[index2].Connections.Add(index1);
    }

    private bool IsValidPoint(Vector3 origin, Vector3 point, Vector3 boundsSize, float cellSize, float multipliedRadius, Dictionary<int, StarNode> points, int[,,] grid)
    {
        // check to see if its within the bounds
        if (point.x >= 0 && point.x < boundsSize.x && point.y >= 0 && point.y < boundsSize.y && point.z >= 0 && point.z < boundsSize.z)
        {
            // check to see if thi point's cell is occupied
            var cellX = (int)(point.x / cellSize);
            var cellY = (int)(point.y / cellSize);
            var cellZ = (int)(point.z / cellSize);

            if (grid[cellX, cellY, cellZ] != 0)
            {
                return false;
            }

            // check to see if its atleast one raidus distance away from any other point
            var searchStartX = Mathf.Max(0, cellX - _settings.NeighborSearchDistance);
            var searchEndX = Mathf.Min(cellX + _settings.NeighborSearchDistance, grid.GetLength(0));
            var searchStartY = Mathf.Max(0, cellY - _settings.NeighborSearchDistance);
            var searchEndY = Mathf.Min(cellY + _settings.NeighborSearchDistance, grid.GetLength(1));
            var searchStartZ = Mathf.Max(0, cellZ - _settings.NeighborSearchDistance);
            var searchEndZ = Mathf.Min(cellZ + _settings.NeighborSearchDistance, grid.GetLength(2));

            for (int x = searchStartX; x < searchEndX; x++)
            {
                for (int y = searchStartY; y < searchEndY; y++)
                {
                    for (int z = searchStartZ; z < searchEndZ; z++)
                    {
                        // the values stored in the grid array are {index + 1} pointers to values in the existing points array
                        // becase 0 is the default int value, 0 here means uninitialized, so we subtract 1 to turn it into -1, for bad value
                        // a value of -1 means there's a blocker there (i.e. a room)
                        var index = grid[x, y, z] - 1;
                        if (index >= 0)
                        {
                            var sqrDst = Mathf.Abs((point - points[index].Pos).magnitude);
                            // do not overlap other points
                            /*if (sqrDst < multipliedRadius + points[index].MultipliedRadius)
                            {
                                return false;
                            }*/
                            // maintain minimum required distance from other points
                            if (sqrDst < points[index].ScatterDistance)
                            {
                                return false;
                            }
                            // do no cross connection lines
                            foreach (var connectionIndex in points[index].Connections)
                            {
                                var connectionPoint = points[connectionIndex];
                                if ((points[index].Pos - origin).sqrMagnitude > 0.01f &&
                                    (points[index].Pos - point).sqrMagnitude > 0.01f &&
                                    (connectionPoint.Pos - origin).sqrMagnitude > 0.01f &&
                                    (connectionPoint.Pos - point).sqrMagnitude > 0.01f)
                                {
                                    if (MathUtility.LineLineIntersection(out var intersection, origin, point, points[index].Pos, connectionPoint.Pos))
                                    {
                                        if (!_settings.AllowOverlappingLines)
                                            return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }


        return false;
    }
}

