using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Collections;


[BurstCompile]
public struct BezierCurve
{
  UnsafeList<Vector2> points;

  public void Initialize(int length, Allocator allocator)
  {
    points = new UnsafeList<Vector2>(length, allocator);
  }

  public void CreateInitialPath(Vector2 startPos, Vector2 endPos, Vector2 agentsDirection, Vector2 controlPointsDirection)
  {
    var quarterDistance = (endPos - startPos).magnitude / 4;
    var P1 = startPos + ((agentsDirection.normalized * quarterDistance) + (Vector2.Perpendicular(agentsDirection.normalized) * controlPointsDirection));
    var P2Dir = (endPos - startPos);
    var P2 = endPos + (P2Dir.normalized * quarterDistance) + (Vector2.Perpendicular(P2Dir) * controlPointsDirection);
    points.Add(startPos);
    points.Add(P1);
    points.Add(P2);
    points.Add(endPos);
  }

  public void AddAditionalAnchorPoints()
  {

  }

  public Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
  {
    Vector2 p0 = Vector2.Lerp(a, b, t);
    Vector2 p1 = Vector2.Lerp(b, c, t);
    return Vector2.Lerp(p0, p1, t);
  }

  public Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
  {
    Vector2 p0 = EvaluateQuadratic(a, b, c, t);
    Vector2 p1 = EvaluateQuadratic(b, c, d, t);
    return Vector2.Lerp(p0, p1, t);
  }

  public void Dispose()
  {
    points.Dispose();
  }
}