using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Collections;


[BurstCompile]
public struct BezierCurve
{
  public UnsafeList<Vector2> points;

  public void Initialize(int length, Allocator allocator)
  {
    points = new UnsafeList<Vector2>(length, allocator);
    points.Resize(length);
  }

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