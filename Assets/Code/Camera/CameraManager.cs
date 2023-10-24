using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CameraManager>();
            }
            return _instance;
        }
    }

    private static CameraManager _instance;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private CinemachineVirtualCamera _mainGalaxyCamera;
    [SerializeField]
    private CinemachineVirtualCamera _mainSolarSystemCamera;
    [SerializeField]
    private GameObject _mainGalaxyCamTarget;
    [SerializeField]
    private float _zoomYDistanceMinGalaxy = 10f;
    [SerializeField]
    private float _ZoomYDistanceMaxGalaxy = 50f;
    [SerializeField]
    private float _zoomYDistanceMinSolarSystem = 20f;
    [SerializeField]
    private float _ZoomYDistanceMaxSolarSystem = 30f;
    [SerializeField]
    private AnimationCurve _zoomPanMultiplier;

    public bool IsMainCameraActive { get; private set; } = false;

    private Bounds _bounds;
    private CapsuleCollider _panConfiner;
    private CinemachineVirtualCamera _currentCamera;
    private float _currentCameraZoomMin;
    private float _currentCameraZoomMax;

    public void SetToGalaxyCamera(Bounds bounds)
    {
        DisableCurrentCamera();
        IsMainCameraActive = true;
        _currentCameraZoomMin = _zoomYDistanceMinGalaxy;
        _currentCameraZoomMax = _ZoomYDistanceMaxGalaxy;
        _currentCamera = _mainGalaxyCamera;
        _currentCamera.enabled = true;
        SetBounds(bounds);
        ResetCamera(true, -15f);
    }

    public void SetToSolarSystemCamera(Bounds bounds)
    {
        DisableCurrentCamera();
        IsMainCameraActive = true;
        _currentCameraZoomMin = _zoomYDistanceMinSolarSystem;
        _currentCameraZoomMax = _ZoomYDistanceMaxSolarSystem;
        _currentCamera = _mainSolarSystemCamera;
        _currentCamera.enabled = true;
        SetBounds(bounds);
        ResetCamera(true, -30f);
        ZoomLevel = 0.5f;
    }

    public void ReturnToSolarSystemCamera()
    {
        DisableCurrentCamera();
        IsMainCameraActive = true;
        _currentCamera = _mainSolarSystemCamera;
        _currentCamera.enabled = true;
    }

    public void SetFocusCamera(CinemachineVirtualCamera cam, float radius)
    {
        DisableCurrentCamera();
        var body = cam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (body != null)
        {
            body.m_CameraDistance = 1 + (radius * 4f);
        }
        IsMainCameraActive = false;
        _currentCamera = cam;
        _currentCamera.enabled = true;
    }

    private void DisableCurrentCamera()
    {
        IsMainCameraActive = false;
        if (_currentCamera != null)
        {
            _currentCamera.enabled = false;
        }
    }

    private void SetBounds(Bounds bounds)
    {
        if (_panConfiner == null)
        {
            _panConfiner = this.gameObject.AddComponent<CapsuleCollider>();
        }

        _bounds = bounds;
        _panConfiner.radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        _panConfiner.center = _bounds.center;
        _panConfiner.height = Mathf.Max(_currentCameraZoomMax, _panConfiner.radius);

        var confiner = _currentCamera.GetComponent<CinemachineConfiner>();
        if (confiner != null)
        {
            const float padding = 10f;
            var collider = confiner.m_BoundingVolume;
            if (collider is SphereCollider sphereCollider)
            {
                sphereCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.z) + padding;
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.z) + padding;
                capsuleCollider.height = Mathf.Max(_currentCameraZoomMax, capsuleCollider.radius);
            }
            else if (collider is BoxCollider boxCollider)
            {
                boxCollider.size = new Vector3(padding, padding, padding) + bounds.size;
            }
        }
    }

    public void ResetCamera(bool instant, float verticalTilt = -15f)
    {
        var delta = _mainGalaxyCamTarget.transform.position - _bounds.center;
        _mainGalaxyCamTarget.transform.position = _bounds.center;
        var pov = _currentCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_VerticalAxis.Value = verticalTilt;
            pov.m_HorizontalAxis.Value = 0;
        }

        ZoomLevel = 1;
        SetHorizontalAutoSnap(true);

        if (instant)
        {
            _currentCamera.PreviousStateIsValid = false;
            _currentCamera.OnTargetObjectWarped(_mainGalaxyCamTarget.transform, delta);
        }
    }

    public void SetHorizontalAutoSnap(bool allowSnapBack)
    {
        if (_currentCamera == null)
        {
            return;
        }

        var pov = _currentCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_HorizontalRecentering.m_enabled = allowSnapBack;
        }
    }

    public float ZoomLevel
    {
        get
        {
            if (IsMainCameraActive)
            {
                if (_currentCamera.m_Lens.Orthographic)
                {
                    //return Mathf.InverseLerp(_orthoSizeMin, _orthoSizeMax, _mainCamera.m_Lens.OrthographicSize);
                }
                var module = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (module != null)
                {
                    return Mathf.InverseLerp(_currentCameraZoomMin, _currentCameraZoomMax, module.m_CameraDistance);
                }
            }

            return 0f;
        }
        set
        {
            if (IsMainCameraActive)
            {
                if (_currentCamera.m_Lens.Orthographic)
                {
                    //_mainCamera.m_Lens.OrthographicSize = Mathf.Lerp(_orthoSizeMin, _orthoSizeMax, value);
                }
                else
                {
                    var module = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                    if (module != null)
                    {
                        var pos = module.m_CameraDistance;
                        pos = Mathf.Lerp(_currentCameraZoomMin, _currentCameraZoomMax, value);
                        module.m_CameraDistance = pos;
                    }
                }
            }
        }
    }

    public void SetRotationDelta(Vector2 screenPosDelta)
    {
        if (_currentCamera == null)
        {
            return;
        }

        var pov = _currentCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_VerticalAxis.Value += screenPosDelta.y * -1;
            pov.m_HorizontalAxis.Value += screenPosDelta.x;
        }
    }

    public void SetPositionDelta(Vector2 screenPosDelta)
    {
        if (!IsMainCameraActive)
        {
            return;
        }
        var position = _mainGalaxyCamTarget.transform.position;

        var rotatedDelta = new Vector3(screenPosDelta.y, 0, -1 * screenPosDelta.x);
        rotatedDelta = Quaternion.AngleAxis(90, Vector3.up) * rotatedDelta;

        var panSpeedVal = ZoomLevel;
        if (_currentCameraZoomMin == _currentCameraZoomMax)
        {
            panSpeedVal = 1f;
        }

        var multiplier = 0.5f;
        multiplier = _zoomPanMultiplier.Evaluate(panSpeedVal);

        rotatedDelta *= multiplier;
        _mainGalaxyCamTarget.transform.position = position + rotatedDelta;

        // clamp position
        var groundCenter = _mainGalaxyCamTarget.transform.position;

        if (_panConfiner != null && !_panConfiner.bounds.Contains(groundCenter))
        {
            var clampedPoint = _panConfiner.ClosestPoint(groundCenter);
            _mainGalaxyCamTarget.transform.position = clampedPoint;
        }
    }

    public void CenterPositionOnScreenPos(Vector2 screenPos)
    {
        var ray = _camera.ScreenPointToRay(screenPos);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out var result))
        {
            var groundPos = ray.GetPoint(result);


            if (_panConfiner != null && !_panConfiner.bounds.Contains(groundPos))
            {
                var clampedPoint = _panConfiner.ClosestPoint(groundPos);
                groundPos = clampedPoint;
            }

            _lastZoomPos = groundPos;
            _mainGalaxyCamTarget.transform.position = groundPos;
        }
        else
        {
            Debug.Log("Failed to hit plane");
        }

    }

    private Vector3 _lastZoomPos;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_lastZoomPos, 1f);
    }
}