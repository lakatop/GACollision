using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Collections;

/// <summary>
/// Struct for bezier curve interpretation
/// </summary>
[BurstCompile]
public struct BezierCurve
{
  /// <summary>
  /// Points defining the bezier curve
  /// </summary>
  public UnsafeList<Vector2> points;

  /// <summary>
  /// Initialization of vezier curve
  /// </summary>
  /// <param name="length">How many points will this bezier curve consists of</param>
  /// <param name="allocator">Storage allocator</param>
  public void Initialize(int length, Allocator allocator)
  {
    points = new UnsafeList<Vector2>(length, allocator);
    points.Resize(length);
  }

  /// <summary>
  /// Creates cubic bezier curve
  /// </summary>
  /// <param name="startPos">First control points</param>
  /// <param name="endPos">Last control point</param>
  /// <param name="agentsDirection">Forward vector of agent</param>
  /// <param name="controlPointsDirection">Perpendicular distance of second and third control point from their forwarding vectors</param>
  public void CreateInitialPath(Vector2 startPos, Vector2 endPos, Vector2 agentsDirection, float controlPointsDirection)
  {
    var quarterDistance = (endPos - startPos).magnitude / 4;
    var P1 = startPos + ((agentsDirection.normalized * quarterDistance) + (Vector2.Perpendicular(agentsDirection.normalized) * controlPointsDirection));
    var P2Dir = (startPos - endPos);
    var P2 = endPos + (P2Dir.normalized * quarterDistance) + (Vector2.Perpendicular((endPos - startPos).normalized) * controlPointsDirection);
    points[0] = startPos;
    points[1] = P1;
    points[2] = P2;
    points[3] = endPos;
  }

  /// <summary>
  /// Returns point on the quadtratic bezier curve
  /// </summary>
  /// <param name="a">First CP</param>
  /// <param name="b">Second CP</param>
  /// <param name="c">Third CP</param>
  /// <param name="t">[0-1] parameter</param>
  /// <returns>Point on curve given by CPs and t</returns>
  public Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
  {
    Vector2 p0 = Vector2.Lerp(a, b, t);
    Vector2 p1 = Vector2.Lerp(b, c, t);
    return Vector2.Lerp(p0, p1, t);
  }

  /// <summary>
  /// Returns point on the cubic bezier curve
  /// </summary>
  /// <param name="a">First CP</param>
  /// <param name="b">Second CP</param>
  /// <param name="c">Third CP</param>
  /// <param name="d">Fourth CP</param>
  /// <param name="t">[0-1] parameter</param>
  /// <returns>Point on curve given by CPs and t</returns>
  public Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
  {
    Vector2 p0 = EvaluateQuadratic(a, b, c, t);
    Vector2 p1 = EvaluateQuadratic(b, c, d, t);
    return Vector2.Lerp(p0, p1, t);
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    points.Dispose();
  }
}
