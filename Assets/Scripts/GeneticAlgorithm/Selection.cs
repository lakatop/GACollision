using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;


[BurstCompile]
public struct BezierElitistSelectionParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {

    currentPopulation.Sort(new BezierIndividualSortAscending());

    int n = 50;

    for (int i = n; i < currentPopulation.Length; i++)
    {
      var newIndividual = currentPopulation[i % n];
      var outdatedIndividual = currentPopulation[i];
      outdatedIndividual.fitness = newIndividual.fitness;
      for (int j = 0; j < outdatedIndividual.accelerations.Length; j++)
      {
        outdatedIndividual.accelerations[j] = newIndividual.accelerations[j];
      }
      for(int j = 0; j < outdatedIndividual.bezierCurve.points.Length; j++)
      {
        outdatedIndividual.bezierCurve.points[j] = newIndividual.bezierCurve.points[j];
      }
      currentPopulation[i] = outdatedIndividual;
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
