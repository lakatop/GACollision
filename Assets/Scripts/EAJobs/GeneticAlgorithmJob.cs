using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct GeneticAlgorithmJob : IJob
{ 
  public bool stopExecution;
  [ReadOnly] public int index;
  [ReadOnly] public float timeDelta;
  [ReadOnly] public NativeArray<Vector2> positions;
  [ReadOnly] public NativeArray<float> speeds;
  [ReadOnly] public NativeArray<UnsafeList<Vector2>> paths;

  public void Execute()
  {
    if (stopExecution)
      return;

    InitializePopulation();
    while (!stopExecution)
    {
      CalculateFitness();
      ApplySelection();
      ApplyOperators();
      SelectNewPopulation();
    }
  }

  private void InitializePopulation()
  {

  }

  private void CalculateFitness()
  {

  }

  private void ApplySelection()
  {

  }

  private void ApplyOperators()
  {

  }

  private void SelectNewPopulation()
  {

  }
}
