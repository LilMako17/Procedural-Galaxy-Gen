using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using UnityEngine;

public class GalaxyBrowseState : State
{
    private Camera _camera;

    private Vector3? _previousMousePos;
    private Vector3 _mouseDownPos;
    private bool _previousMouse2Down;
    private bool _previousMouse1Down;

    private bool _isDrag;
    private float? _zoomStartTime;
    private int _zoomCooldownCounter;

    protected virtual bool _forceRotationSnapBack => true;

    public GalaxyBrowseState(Camera camera)
    {
        _camera = camera;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnLogic()
    {
        UpdateZoom();
        UpdateMoveDrag();
        UpdateObjectInspect();
    }

    private void UpdateZoom()
    {
        var scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            var currentZoom = CameraManager.Instance.ZoomLevel;

            if (currentZoom > 0 && scrollDelta > 0 || currentZoom < 1 && scrollDelta < 0)
            {
                CameraManager.Instance.ZoomLevel = currentZoom + (0.05f * (scrollDelta * -1));
                if (!_zoomStartTime.HasValue && scrollDelta > 0)
                {
                    _zoomStartTime = Time.time;
                    CameraManager.Instance.CenterPositionOnScreenPos(Input.mousePosition);
                }
            }
        }
        else if (_zoomStartTime.HasValue)
        {
            _zoomCooldownCounter++;
            if (_zoomCooldownCounter > 600)
            {
                ResetZoomTimer();
            }
        }
    }

    private void ResetZoomTimer()
    {
        _zoomCooldownCounter = 0;
        _zoomStartTime = null;
    }

    private void UpdateMoveDrag()
    {
        // on mouse down
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            _mouseDownPos = Input.mousePosition;
        }

        // while mouse down
        if (_previousMousePos.HasValue)
        {
            var delta = Input.mousePosition - _previousMousePos.Value;

            if (Input.GetMouseButton(2) && _previousMouse2Down)
            {
                ResetZoomTimer();
                CameraManager.Instance.SetPositionDelta(delta);
            }
            if (Input.GetMouseButton(1) && _previousMouse1Down)
            {
                CameraManager.Instance.SetRotationDelta(delta);
            }
        }

        // on mouse up
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            _isDrag = false;
        }

        _previousMousePos = Input.mousePosition;
        _previousMouse2Down = Input.GetMouseButton(2);
        _previousMouse1Down = Input.GetMouseButton(1);

        if (_forceRotationSnapBack)
        {
            CameraManager.Instance.SetHorizontalAutoSnap(_previousMouse1Down == false);
        }
    }

    protected virtual void UpdateObjectInspect()
    {
        var p = UIManager.Instance.GetPanel<GalaxyHud>();
        var mousePos = Input.mousePosition;
        if (!UIManager.Instance.IsOverUI(mousePos))
        {

            var ray = _camera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                var script = hit.transform.GetComponent<GalaxyStar>();
                if (script != null)
                {
                    var name = script.GetData().Name;
                    var str = name + " (" + script.GetStarTypeDisplayName(script.GetData()) + ")";
                    p.ShowTooltip(mousePos, str);
                }
                else
                {
                    //UnityEngine.Debug.Log("hit " + hit.transform.name, hit.transform);
                    p.HideTooltip();
                }
            }
            else
            {
                p.HideTooltip();
            }
        }
        else
        {
            p.HideTooltip();
        }
    }
}

