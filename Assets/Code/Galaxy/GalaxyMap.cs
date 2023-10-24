using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;
using Sirenix.OdinInspector;

public class GalaxyMap : MonoBehaviour
{
    public static GalaxyMap Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindAnyObjectByType<GalaxyMap>();
            }

            return _instance;
        }
    }

    private static GalaxyMap _instance;

    [SerializeField]
    private GalaxySettings _defaultSettings;
    [SerializeField]
    private bool _debugVisualizePropGrid;

    private GameObject _galaxyMapParent;
    private List<GameObject> _galaxyPooledObjects = new List<GameObject>();

    private Bounds _bounds;

    private PrefabPoolManager _poolManager;

    public SerializedGalaxyData StarData => UserDataManager.Instance.Current?.GalaxyData;

    private void Awake()
    {
        _poolManager = this.GetComponent<PrefabPoolManager>();
        if (_poolManager == null)
        {
            _poolManager = this.gameObject.AddComponent<PrefabPoolManager>();
        }
        _poolManager.InitializePoolParent();
    }

    private void OnDestroy()
    {
    }

    // should be called on destroy to un-init
    public void Cleanup()
    {
        ClearGalaxyPools();

        if (_galaxyMapParent != null)
        {
            GameObject.Destroy(_galaxyMapParent);
        }
    }

    public void RandomizedSeed()
    {
        var starData = GalaxyGenerator.Instance.GenerateNew(_defaultSettings, UnityEngine.Random.Range(0, int.MaxValue));
        UserDataManager.Instance.Current.GalaxyData = starData;
        UserDataManager.Instance.SetDirty();

        UpdateGalaxyRender();
    }

    public void LoadStarMap(SerializedGalaxyData data)
    {
        UpdateGalaxyRender();
    }

    private void OnDrawGizmos()
    {
        var debug_lastGrid = GalaxyGenerator.Instance.Debug_GetLastGrid();
        if (debug_lastGrid != null && _debugVisualizePropGrid)
        {
            var originX = _bounds.min.x;
            var originY = _bounds.min.y;
            var originZ = _bounds.min.z;

            for (int x = 0; x < debug_lastGrid.GetLength(0); x++)
            {
                for (int y = 0; y < debug_lastGrid.GetLength(1); y++)
                {
                    for (int z = 0; z < debug_lastGrid.GetLength(2); z++)
                    {
                        // enable for cross section
                        /*if (z != 25 && x != 25)
                        {
                            continue;
                        }*/

                        var pos = new Vector3(originX + x, originY + y, originZ + z);
                        var index = debug_lastGrid[x, y, z];
                        if (index == 0)
                        {
                            Gizmos.color = Color.white;
                        }
                        else if (index == -1)
                        {
                            Gizmos.color = Color.red;
                        }
                        else if (index < -1)
                        {
                            Gizmos.color = new Color(1, 0, (100 - (index * -1)) / 100f, 1);
                        }
                        else
                        {
                            Gizmos.color = Color.green;
                        }
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }

    private void EnsureInit()
    {
        if (_galaxyMapParent == null)
        {
            _galaxyMapParent = new GameObject("Galaxy Objects");
        }

        _bounds = new Bounds(Vector3.zero, _defaultSettings.Bounds);;
        _poolManager.TryCreatePool(_defaultSettings.LineRendererPrefab.gameObject);
        CameraManager.Instance.SetToGalaxyCamera(_bounds);
        foreach (var star in _defaultSettings.StarOptions)
        {
            _poolManager.TryCreatePool(star.StarBillboardPrefab);
        }
    }

    private void UpdateGalaxyRender()
    {
        EnsureInit();
        ClearGalaxyPools();

        if (StarData == null)
        {
            return;
        }

        var originX = _bounds.min.x;
        var originY = _bounds.min.y;
        var originZ = _bounds.min.z;

        // create the game objects!
        foreach (var kvp in StarData.StarMap)
        {
            var point = kvp.Value;
            var propData = _defaultSettings.StarOptions[point.AssetIndex];
            var worldPoint = new Vector3(point.Pos.x + originX, point.Pos.y + originY, point.Pos.z + originZ);

            var rotation = Quaternion.AngleAxis(point.Rotation, Vector3.up);
            var instance = _poolManager.Retrieve(propData.StarBillboardPrefab, _galaxyMapParent.transform);
            instance.transform.SetPositionAndRotation(worldPoint, rotation);
            instance.transform.localScale = propData.StarBillboardPrefab.transform.localScale * point.Scale;

            var script = instance.GetComponent<GalaxyStar>();
            if (script == null)
            {
                script = instance.AddComponent<GalaxyStar>();
            }
            script.Init(kvp.Key);

            _galaxyPooledObjects.Add(instance);
        }

        if (StarData.StarMap.Count > 0)
        {
            var visited = new HashSet<int>();
            var keys = StarData.StarMap.Keys.ToList();
            var index = 0;
            while (index < keys.Count)
            {
                if (!visited.Contains(keys[index]))
                {
                    UpdateConnectionLinesRecurisve(keys[index], visited);
                }
                index++;
            }
        }
    }

    private void UpdateConnectionLinesRecurisve(int currentIndex, HashSet<int> visited)
    {
        if (visited.Contains(currentIndex))
        {
            return;
        }
        visited.Add(currentIndex);

        var originX = _bounds.min.x;
        var originY = _bounds.min.y;
        var originZ = _bounds.min.z;

        var currentNode = StarData.StarMap[currentIndex];
        foreach (var connection in currentNode.Connections)
        {
            var connectionData = StarData.StarMap[connection];
            var lineGameObject = _poolManager.Retrieve(_defaultSettings.LineRendererPrefab.gameObject, _galaxyMapParent.transform);
            _galaxyPooledObjects.Add(lineGameObject);
            var lineRenderer = lineGameObject.GetComponent<LineRenderer>();
            var worldPoint1 = new Vector3(currentNode.Pos.x + originX, currentNode.Pos.y + originY, currentNode.Pos.z+ originZ);
            var worldPoint2 = new Vector3(connectionData.Pos.x + originX, connectionData.Pos.y + originY, connectionData.Pos.z + originZ);
            lineRenderer.SetPosition(0, worldPoint1);
            lineRenderer.SetPosition(1, worldPoint2);

            UpdateConnectionLinesRecurisve(connection, visited);
        }
    }

    public void ClearGalaxyPools()
    {
        foreach (var entry in _galaxyPooledObjects)
        {
            _poolManager.Release(entry);
        }
        _galaxyPooledObjects.Clear();
    }

    public void OnSelectStar(GalaxyStar star)
    {
        var data = star.GetData();
        UnityEngine.Debug.Log("select " + data.Name);
        HideGalaxy();

        SolarSystemMap.Instance.ShowSolarSystem(_defaultSettings, star.DataId);
    }

    public void ShowGalaxy()
    {
        UIManager.Instance.GetPanel<GalaxyHud>().Show();
        _galaxyMapParent.gameObject.SetActive(true);
        CameraManager.Instance.SetToGalaxyCamera(_bounds);
        InputManager.Instance.SetState(InputManager.STATE_GALAXY_BROWSE);
    }

    public void HideGalaxy()
    {
        UIManager.Instance.GetPanel<GalaxyHud>().Hide();
        _galaxyMapParent.gameObject.SetActive(false);
    }
}

