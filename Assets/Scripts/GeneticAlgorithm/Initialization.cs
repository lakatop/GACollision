using Unity.Burst;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Initialization for BezierIndividualStruct designed ot be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierInitialization : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Size of population
  /// </summary>
  [ReadOnly] public int populationSize;
  /// <summary>
  /// Length of path BezierIndividualStruct has
  /// </summary>
  [ReadOnly] public int pathSize;
  /// <summary>
  /// Agents current position
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// Agents destination
  /// </summary>
  [ReadOnly] public Vector2 endPosition;
  /// <summary>
  /// Agents forward vector
  /// </summary>
  [ReadOnly] public Vector2 forward;
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;

  /// <summary>
  /// Initialize population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    float maxDeg = 30;
    float quadDistance = (endPosition - startPosition).magnitude / 4;
    float controlPointLenght = Mathf.Tan(maxDeg * Mathf.Deg2Rad) * quadDistance;
    float subFactor = (controlPointLenght * 2) / populationSize;

    for(int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.bezierCurve.CreateInitialPath(startPosition, endPosition, forward, controlPointLenght);
      controlPointLenght -= subFactor;
      for(int j = 0; j < pathSize; j++)
      {
        var acc = (rand.NextFloat() * 2f) - 1f;
        individual.accelerations[j] = acc;
      }
      currentPopulation[i] = individual;
    }
  }

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}
