using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OrbitLine : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _lineRenderer;
    [SerializeField]
    private int _segments = 50;

    private float _xRadius;
    private float _yRadius;

    public void SetOrbit(float xRadius, float yRadius, Vector3 center)
    {
        _xRadius = xRadius;
        _yRadius = yRadius;

        _lineRenderer.useWorldSpace = false;
        _lineRenderer.positionCount = _segments + 1;

        this.transform.position = center;

        float angle = 20f;

        for (int i = 0; i < (_segments + 1); i++)
        {
            var x = Mathf.Sin(Mathf.Deg2Rad * angle) * _xRadius;
            var y = Mathf.Cos(Mathf.Deg2Rad * angle) * _yRadius;

            _lineRenderer.SetPosition(i, new Vector3(x, 0, y));

            angle += (360f / _segments);
        }
    }

    public Vector3 GetPositionAtAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0)
        {
            angle = angle + 360f;
        }

        var normalized = (angle / 360f);
        var roundedIndex = (int)(normalized * _segments);

        return _lineRenderer.GetPosition(roundedIndex);
    }
}
