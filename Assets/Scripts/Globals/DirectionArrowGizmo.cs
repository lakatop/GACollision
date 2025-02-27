using UnityEngine;

/// <summary>
/// Debug class for drawing agents forward vector
/// </summary>
public class DirectionArrowGizmo : MonoBehaviour
{
  public float length = 2.0f;

  /// <summary>
  /// Called by unity, triggers drawing
  /// </summary>
  void OnDrawGizmos()
  {
    DrawArrow(transform.position, gameObject.transform.forward, length, Color.red);
  }

  /// <summary>
  /// Draw an arrow using Gizmos
  /// </summary>
  /// <param name="start">staring position</param>
  /// <param name="dir">direction</param>
  /// <param name="length">length of arrow</param>
  /// <param name="color">color of arrow</param>
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