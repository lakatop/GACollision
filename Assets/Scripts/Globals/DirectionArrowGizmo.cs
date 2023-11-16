using UnityEngine;

public class DirectionArrowGizmo : MonoBehaviour
{
  public float length = 2.0f;

  void OnDrawGizmos()
  {
    DrawArrow(transform.position, gameObject.transform.forward, length, Color.red);
  }

  // Draw an arrow using Gizmos
  void DrawArrow(Vector3 start, Vector3 dir, float length, Color color)
  {
    Gizmos.color = color;

    // Draw the line
    Gizmos.DrawRay(start, dir * length);

    // Draw the arrowhead
    float arrowSize = 0.2f;
    Vector3 arrowEnd = start + dir * length;
    Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + 45, 0) * Vector3.forward;
    Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - 45, 0) * Vector3.forward;

    Gizmos.DrawRay(arrowEnd, right * arrowSize);
    Gizmos.DrawRay(arrowEnd, left * arrowSize);
  }
}