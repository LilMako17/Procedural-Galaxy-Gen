using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MathUtilityTest : MonoBehaviour
{
    public Vector3 point1A;
    public Vector3 point1B;
    public Vector3 point2A;
    public Vector3 point2B;

    public float tolerance = 0.0001f;

    private void OnDrawGizmos()
    {
        var success = MathUtility.LineLineIntersection(out var interesection, point1A, point1B, point2A, point2B, tolerance);

        Gizmos.color = Color.white;

        Gizmos.DrawSphere(point1A, 1f);
        Gizmos.DrawSphere(point1B, 1f);
        Gizmos.DrawSphere(point2A, 1f);
        Gizmos.DrawSphere(point2B, 1f);

        if (success)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(interesection, 1f);
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawLine(point1A, point1B);
        Gizmos.DrawLine(point2A, point2B);
    }
}
