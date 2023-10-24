using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Various math utility methods.
/// SEE http://csharphelper.com/blog/search/
/// </summary>
public class MathUtility
{
    public static Vector3[] GetFrustumPlaneSlice(Camera camera, Plane? plane = null)
    {
        var viewport = new Rect(0, 0, 1, 1);
        var array = new Vector3[4];
        var origin = camera.transform.position;

        if (camera.orthographic)
        {
            var bottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            var topLeft = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.nearClipPlane));
            var topRight = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane));
            var bottomRight = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.nearClipPlane));

            array[0] = bottomLeft;
            array[1] = topLeft;
            array[2] = topRight;
            array[3] = bottomRight;

            if (plane == null)
            {
                plane = new Plane(camera.transform.forward * -1, camera.farClipPlane);
            }

            for (var i = 0; i < array.Length; i++)
            {
                if (plane.HasValue)
                {
                    var dir = camera.transform.forward;
                    if (plane.Value.Raycast(new Ray(array[i], dir), out var distance))
                    {
                        array[i] = array[i] + dir * distance;
                    }
                }
            }
            return array;
        }

        camera.CalculateFrustumCorners(
          viewport,
          camera.farClipPlane,
          Camera.MonoOrStereoscopicEye.Mono,
          array
        );

        for (var i = 0; i < array.Length; i++)
        {
            array[i] = camera.transform.TransformPoint(array[i]);

            if (plane.HasValue)
            {
                var dir = (array[i] - origin).normalized;
                if (plane.Value.Raycast(new Ray(origin, dir), out var distance))
                {
                    array[i] = origin + dir * distance;
                }
            }
        }

        return array;
    }


    /// <summary>
    /// Remaps a value t in the range [low..high] into the range [0..1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Remap01(float low, float high, float t)
    {
        if (t <= low) return 0;
        if (t >= high) return 1;
        return (t - low) / (high - low);
    }

    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    public static float GetAngle(Vector3 A, Vector3 B, Vector3 C)
    {
        var ab = A - B;
        var bc = C - B;

        return Vector3.SignedAngle(ab, bc, Vector3.up) * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Return a point on line AB that is closest to point
    /// </summary>
    /// <param name="linePointA"></param>
    /// <param name="linePointB"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector3 GetClosestPointOnLine(Vector3 linePointA, Vector3 linePointB, Vector3 point)
    {
        var ap = point - linePointA;
        var ab = linePointB - linePointA;
        var square = ab.sqrMagnitude;
        var dot = Vector3.Dot(ap, ab);
        var t = dot / square;
        return new Vector3(
            linePointA.x + ab.x * t,
            linePointA.y + ab.y * t,
            linePointA.z + ab.z * t);
    }

    public static float DistanceLineSegmentPoint(Vector3 start, Vector3 end, Vector3 point)
    {
        var wander = point - start;
        var span = end - start;

        // Compute how far along the line is the closest approach to our point.
        float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);

        Vector3 nearest = start + t * span;
        return (nearest - point).magnitude;
    }


    /// <summary>
    /// Calculates the intersection of two given lines
    /// </summary>
    /// <param name="intersection">returned intersection</param>
    /// <param name="linePoint1">start location of the line 1</param>
    /// <param name="lineDirection1">direction of line 1</param>
    /// <param name="linePoint2">start location of the line 2</param>
    /// <param name="lineDirection2">direction of line2</param>
    /// <returns>true: lines intersect, false: lines do not intersect</returns>
    public static bool RayRayIntersection(out Vector3 intersection,
        Vector3 linePoint1, Vector3 lineDirection1,
        Vector3 linePoint2, Vector3 lineDirection2, float tolerance = 0.0001f)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < tolerance
                && crossVec1and2.sqrMagnitude > tolerance)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineDirection1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    public static bool LineLineIntersection(out Vector3 intersection,
        Vector3 linePoint1a, Vector3 linePoint1b,
        Vector3 linePoint2a, Vector3 linePoint2b, float tolerance = 0.0001f)
    {
        var success = RayRayIntersection(out intersection, linePoint1a, linePoint1b - linePoint1a, linePoint2a, linePoint2b - linePoint2a, tolerance);
        if (success)
        {
            var distance = DistanceLineSegmentPoint(linePoint1a, linePoint1b, intersection);
            if (distance > 0.001f)
            {
                return false;
            }
            distance = DistanceLineSegmentPoint(linePoint2a, linePoint2b, intersection);
            if (distance > 0.001f)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public static float DistanceToRay(Ray ray, Vector3 point)
    {
        var pointOfIntersection = ray.origin + ray.direction * Vector3.Dot(ray.direction, point - ray.origin);
        //VisualDebugger.Instance.AddSphere(Color.green, pointOfIntersection, 0.2f, 0.01f);

        var angle = Vector3.Angle(pointOfIntersection - ray.origin, ray.GetPoint(1) - ray.origin);
        //VisualDebugger.Instance.AddText(pointOfIntersection, angle.ToString(), 0.01f);
        if (angle > 0.2f)
        {
            return (point - ray.origin).magnitude;
        }
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    // Find the points where the two circles intersect.
    public static int FindCircleCircleIntersections(
        float cx0, float cy0, float radius0,
        float cx1, float cy1, float radius1,
        out Vector2 intersection1, out Vector2 intersection2)
    {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Mathf.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else
        {
            // Find a and h.
            double a = (radius0 * radius0 -
                radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(radius0 * radius0 - a * a);

            // Find P2.
            double cx2 = cx0 + a * (cx1 - cx0) / dist;
            double cy2 = cy0 + a * (cy1 - cy0) / dist;

            // Get the points P3.
            intersection1 = new Vector2(
                (float)(cx2 + h * (cy1 - cy0) / dist),
                (float)(cy2 - h * (cx1 - cx0) / dist));
            intersection2 = new Vector2(
                (float)(cx2 - h * (cy1 - cy0) / dist),
                (float)(cy2 + h * (cx1 - cx0) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return 1;
            return 2;
        }
    }

    // Find the tangent points for this circle and external point.
    // Return true if we find the tangents, false if the point is
    // inside the circle.
    public static bool FindTangents(Vector2 center, float radius,
        Vector2 external_point, out Vector2 pt1, out Vector2 pt2)
    {
        // Find the distance squared from the
        // external point to the circle's center.
        double dx = center.x - external_point.x;
        double dy = center.y - external_point.y;
        double D_squared = dx * dx + dy * dy;
        if (D_squared < radius * radius)
        {
            pt1 = new Vector2(-1, -1);
            pt2 = new Vector2(-1, -1);
            return false;
        }

        // Find the distance from the external point
        // to the tangent points.
        double L = Math.Sqrt(D_squared - radius * radius);

        // Find the points of intersection between
        // the original circle and the circle with
        // center external_point and radius dist.
        FindCircleCircleIntersections(
            center.x, center.y, radius,
            external_point.x, external_point.y, (float)L,
            out pt1, out pt2);

        return true;
    }

    public static Vector2[] FindCircleSegmentIntersections(float radius, Vector2 pos, Vector2 pt1, Vector2 pt2, bool segment_only)
    {
        return FindEllipseSegmentIntersections(new Rect(pos.x - radius, pos.y - radius, radius * 2, radius * 2), pt1, pt2, segment_only);
    }

    // Find the points of intersection between
    // an ellipse and a line segment.
    public static Vector2[] FindEllipseSegmentIntersections(
        Rect rect, Vector2 pt1, Vector2 pt2, bool segment_only)
    {
        // If the ellipse or line segment are empty, return no intersections.
        if ((rect.width == 0) || (rect.height == 0) ||
            ((pt1.x == pt2.x) && (pt1.y == pt2.y)))
            return new Vector2[] { };

        // Make sure the rectangle has non-negative width and height.
        if (rect.width < 0)
        {
            rect.x = rect.xMax;
            rect.width = -rect.width;
        }
        if (rect.height < 0)
        {
            rect.y = rect.yMax;
            rect.height = -rect.height;
        }

        // Translate so the ellipse is centered at the origin.
        float cx = rect.xMin + rect.width / 2f;
        float cy = rect.yMin + rect.height / 2f;
        rect.x -= cx;
        rect.y -= cy;
        pt1.x -= cx;
        pt1.y -= cy;
        pt2.x -= cx;
        pt2.y -= cy;

        // Get the semimajor and semiminor axes.
        float a = rect.width / 2;
        float b = rect.height / 2;

        // Calculate the quadratic parameters.
        float A = (pt2.x - pt1.x) * (pt2.x - pt1.x) / a / a +
                  (pt2.y - pt1.y) * (pt2.y - pt1.y) / b / b;
        float B = 2 * pt1.x * (pt2.x - pt1.x) / a / a +
                  2 * pt1.y * (pt2.y - pt1.y) / b / b;
        float C = pt1.x * pt1.x / a / a + pt1.y * pt1.y / b / b - 1;

        // Make a list of t values.
        List<float> t_values = new List<float>();

        // Calculate the discriminant.
        float discriminant = B * B - 4 * A * C;
        if (discriminant == 0)
        {
            // One real solution.
            t_values.Add(-B / 2 / A);
        }
        else if (discriminant > 0)
        {
            // Two real solutions.
            t_values.Add((float)((-B + Math.Sqrt(discriminant)) / 2 / A));
            t_values.Add((float)((-B - Math.Sqrt(discriminant)) / 2 / A));
        }

        // Convert the t values into points.
        List<Vector2> points = new List<Vector2>();
        foreach (float t in t_values)
        {
            // If the points are on the segment (or we
            // don't care if they are), add them to the list.
            if (!segment_only || ((t >= 0f) && (t <= 1f)))
            {
                float x = pt1.x + (pt2.x - pt1.x) * t + cx;
                float y = pt1.y + (pt2.y - pt1.y) * t + cy;
                points.Add(new Vector2(x, y));
            }
        }

        // Return the points.
        return points.ToArray();
    }

    // Find the points of intersection.
    public static int FindLineCircleIntersections(
        float cx, float cy, float radius,
        Vector2 point1, Vector2 point2,
        out Vector2 intersection1, out Vector2 intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
        C = (point1.x - cx) * (point1.x - cx) +
            (point1.y - cy) * (point1.y - cy) -
            radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));
            intersection1 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - Mathf.Sqrt(det)) / (2 * A));
            intersection2 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            return 2;
        }
    }

}

