using NativeQuadTree;
using Unity.Collections;
using Unity.Mathematics;
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
    /// Check number of collision on my position with results returned by quadtree
    /// </summary>
    /// <param name="pos">Agents current position</param>
    /// <param name="queryRes">Result from query performed on quadtree</param>
    /// <param name="stepIndex">Current step index in simulation</param>
    /// <returns>Number of collisions</returns>
    public static int Collides(Vector2 pos, NativeList<NativeQuadTree.QuadElement<TreeNode>> queryRes, int stepIndex, float _agentRadius, int _agentIndex)
    {
      int collisionCount = 0;

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
          collisionCount++;
        }
      }

      return collisionCount;
    }

    /// <summary>
    /// Check nubmer of collisions between agents startPos and endPos path
    /// If stepIndex is greated than 1, dont count startPos collisions - because they were counted in previous call
    /// </summary>
    /// <param name="quadTree">Quadtree with all objects present in simulation</param>
    /// <param name="startPos">Starting position of agent</param>
    /// <param name="endPos">Ending position of agent</param>
    /// <param name="agentRadius">Agents radius</param>
    /// <param name="agentIndex">Agents index</param>
    /// <param name="stepIndex">Step index - index of which path segment this is</param>
    /// <returns>Number of collisions</returns>
    public static int Collides(NativeQuadTree<TreeNode> quadTree, Vector2 startPos, Vector2 endPos, float agentRadius, int agentIndex, int stepIndex)
    {
      int stepsCount = (int)(((endPos - startPos).magnitude - (2 * agentRadius)) / (2 * agentRadius));
      stepsCount++; // for startPos
      var newPos = startPos;
      var stepVelocity = (endPos - startPos).normalized * 2 * agentRadius;
      var collisionCount = 0;
      for (int i = 0; i < stepsCount; i++)
      {
        // We want to skip startPos because it was counted in the previous call
        if(stepIndex != 1 && i == 0)
        {
          newPos += stepVelocity;
          continue;
        }

        AABB2D bounds = new AABB2D(newPos, new float2(5, 5));
        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
        quadTree.RangeQuery(bounds, queryRes);

        collisionCount += Collides(newPos, queryRes, stepIndex, agentRadius, agentIndex);
        queryRes.Dispose();

        newPos += stepVelocity;
      }

      AABB2D boundsEnd = new AABB2D(endPos, new float2(5, 5));
      NativeList<QuadElement<TreeNode>> queryResEnd = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
      quadTree.RangeQuery(boundsEnd, queryResEnd);

      collisionCount += Collides(newPos, queryResEnd, stepIndex, agentRadius, agentIndex);
      queryResEnd.Dispose();

      return collisionCount;
    }

    /// <summary>
    /// Gets radius of circle based on 3 points that are on the arc
    /// https://math.stackexchange.com/a/3503338/1278830
    /// </summary>
    /// <param name="z1">Point 1</param>
    /// <param name="z2">Point 2</param>
    /// <param name="z3">Point 3</param>
    /// <returns>Radius of circle</returns>
    public static double GetCircleRadius(System.Numerics.Complex z1, System.Numerics.Complex z2, System.Numerics.Complex z3)
    {
      if ((z1 == z2) || (z2 == z3) || (z3 == z1))
      {
        return -1;
      }

      var w = (z3 - z1) / (z2 - z1);
      if(System.Numerics.Complex.Abs(w.Imaginary) <= 0)
      {
        return -1;
      }

      var c = ((z2 - z1) * ((w - System.Math.Pow(System.Numerics.Complex.Abs(w), 2)) / (new System.Numerics.Complex(0, 2) * w.Imaginary))) + z1;

      var r = System.Numerics.Complex.Abs(z1 - c);

      return r;
    }
  }
}