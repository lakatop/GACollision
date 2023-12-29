using UnityEngine;

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
}