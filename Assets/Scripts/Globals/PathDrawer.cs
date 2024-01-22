using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public static class PathDrawer
{
  public static void DrawPath(Vector2 previousPosition, Vector2 currentPosition, Vector2 pathSelected)
  {
    Debug.DrawLine(new Vector3(previousPosition.x, 0f, previousPosition.y), new Vector3(currentPosition.x, 0f, currentPosition.y), new Color(0,0,1), 100, false);

    Debug.DrawRay(new Vector3(previousPosition.x, 0f, previousPosition.y), new Vector3(pathSelected.x, 0f, pathSelected.y), new Color(1, 0, 0), 100, false);

    var dir = currentPosition - previousPosition;
    float arrowSize = 0.2f;
    var arrowEnd = previousPosition + pathSelected;
    Vector3 right = Quaternion.LookRotation(new Vector3(dir.x,0,dir.y)) * Quaternion.Euler(0, 180 + 45, 0) * Vector3.forward;
    Vector3 left = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y)) * Quaternion.Euler(0, 180 - 45, 0) * Vector3.forward;
    Debug.DrawRay(new Vector3(arrowEnd.x, 0, arrowEnd.y), right * arrowSize, new Color(1,0,0), 100, false);
    Debug.DrawRay(new Vector3(arrowEnd.x, 0, arrowEnd.y), left * arrowSize, new Color(1,0,0), 100, false);
  }

  public static void DrawDestination(Vector2 destination)
  {
    Debug.DrawLine(new Vector3(destination.x - 1, 0, destination.y), new Vector3(destination.x + 1, 0, destination.y), new Color(1, 1, 1), 0, false);
    Debug.DrawLine(new Vector3(destination.x, 0, destination.y - 1), new Vector3(destination.x, 0, destination.y + 1), new Color(1, 1, 1), 0, false);
  }

  public static void DrawCollisionPoint(Vector2 position)
  {
    Debug.DrawLine(new Vector3(position.x - 1, 0, position.y), new Vector3(position.x + 1, 0, position.y), new Color(0, 0, 0), 0, false);
    Debug.DrawLine(new Vector3(position.x, 0, position.y - 1), new Vector3(position.x, 0, position.y + 1), new Color(0, 0, 0), 0, false);
  }

  public static void DrawConnectionLine(Vector2 start, Vector2 end)
  {
    Debug.DrawLine(new Vector3(start.x, 0f, start.y), new Vector3(end.x, 0f, end.y), new Color(0, 0, 0), 100, false);
  }

  public static void DrawCircle(Vector2 center2, float radius)
  {
    Vector3 center = new Vector3(center2.x, 0f, center2.y);
    float angleIncrement = 360f / 36;

    Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

    for (int i = 1; i <= 36; i++)
    {
      float angle = i * angleIncrement;
      float x = center.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
      float z = center.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);

      Vector3 currentPoint = new Vector3(x, center.y, z);

      Debug.DrawLine(prevPoint, currentPoint, Color.black, Mathf.Infinity, false);

      prevPoint = currentPoint;
    }

    // Connect the last point to the first point to complete the circle
    Debug.DrawLine(prevPoint, center + new Vector3(radius, 0f, 0f), Color.black, Mathf.Infinity, false);
  }
}