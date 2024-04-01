using Unity.Burst;
using Unity.Collections;

/// <summary>
/// Elitist selection for BezierIndividualStruct desidned to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierElitistSelectionParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Perform Elitist selection on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
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
