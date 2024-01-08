using Unity.Collections;
using UnityEngine;

namespace UtilsGA
{
  public static class UtilsGA
  {
    /// <summary>
    /// Calculate normalized vector with degrees rotation compared to initialVector
    /// </summary>
    /// <param name="degrees">Rotation of new vector - counter clockwise</param>
    /// <param name="initialVector">Vector to which we perform rotation. New vector has origin where initialVector ends</param>
    /// <returns>Rotated vector</returns>
    public static Vector2 CalculateRotatedAndTranslatedVector(float degrees, Vector2 initialVector)
    {
      var placeOrigin = initialVector;
      var rotationVector = placeOrigin.normalized;

      var rotatedVector = RotateVector(rotationVector, degrees);
      var rotatedAndTranslatedVector = MoveToOrigin(rotatedVector, placeOrigin);

      return rotatedAndTranslatedVector;
    }

    public static Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
      // Convert the vector to a Quaternion
      Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);

      // Rotate the vector using the Quaternion
      return rotation * vector;
    }

    /// <summary>
    /// Rotates vector around given axis.
    /// </summary>
    /// <param name="vectorToRotate">Vector that will be rotated</param>
    /// <param name="referenceVector">Axis around which vector rotates</param>
    /// <param name="angle">Rotation angle</param>
    /// <returns>Rotated vector</returns>
    public static Vector2 RotateVectorAroundAxes(Vector2 vectorToRotate, Vector2 referenceVector, float angle)
    {
      // Calculate the angle between the two vectors
      float currentAngle = Vector2.SignedAngle(referenceVector, vectorToRotate);

      // Adjust the rotation direction based on the specified angle
      float newAngle = currentAngle + angle;

      // Convert the angle back to radians
      float newAngleRad = newAngle * Mathf.Deg2Rad;

      // Calculate the rotated vector
      float rotatedX = Mathf.Cos(newAngleRad) * vectorToRotate.x - Mathf.Sin(newAngleRad) * vectorToRotate.y;
      float rotatedY = Mathf.Sin(newAngleRad) * vectorToRotate.x + Mathf.Cos(newAngleRad) * vectorToRotate.y;

      return new Vector2(rotatedX, rotatedY);
    }

    public static Vector2 MoveToOrigin(Vector2 vectorToMove, Vector2 referenceVector)
    {
      // Get the length of the reference vector
      //float length = referenceVector.magnitude;

      // Translate the vector by the length of the reference vector
      return vectorToMove + referenceVector;
    }

    /// <summary>
    /// Check whether my position collides with results returned by quadtree
    /// </summary>
    /// <param name="pos">Agents current position</param>
    /// <param name="queryRes">Result from query performed on quadtree</param>
    /// <param name="stepIndex">Current step index in simulation</param>
    /// <returns></returns>
    public static bool Collides(Vector2 pos, NativeList<NativeQuadTree.QuadElement<TreeNode>> queryRes, int stepIndex, float _agentRadius, int _agentIndex)
    {
      bool collides = false;

      foreach (var element in queryRes)
      {
        if ((element.element.agentIndex == _agentIndex)
            || (!element.element.staticObstacle && element.element.stepIndex != stepIndex))
        {
          continue;
        }

        Vector2 v = new Vector2(element.pos.x - pos.x, element.pos.y - pos.y);
        if (v.magnitude < (_agentRadius * 2))
        {
          collides = true;
          break;
        }
      }

      return collides;
    }
  }
}