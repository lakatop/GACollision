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
    /// <param name="initialVector">Vector to which we perform rotation. New vector ahs origin where initialVector ends</param>
    /// <returns>Rotated vector</returns>
    public static Vector2 CalculateRotatedVector(float degrees, Vector2 initialVector)
    {
      // Convert degrees to radians for Mathf functions
      float radians = Mathf.Deg2Rad * degrees;


      // Calculate the components of the new vector using trigonometry
      float xComponent = Mathf.Cos(radians) * initialVector.x - Mathf.Sin(radians) * initialVector.y;
      float yComponent = Mathf.Sin(radians) * initialVector.x + Mathf.Cos(radians) * initialVector.y;

      // Create the new vector
      Vector2 generatedVector = new Vector2(xComponent, yComponent).normalized;

      return generatedVector;

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