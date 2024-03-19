using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
public struct BezierInitialization : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public int populationSize;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 endPosition;
  [ReadOnly] public Vector2 forward;
  [ReadOnly] public Unity.Mathematics.Random rand;

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

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}
