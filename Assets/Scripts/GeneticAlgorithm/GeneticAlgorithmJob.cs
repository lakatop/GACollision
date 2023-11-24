using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

[BurstCompile]
public struct GeneticAlgorithmJob : IJob
{ 
  [ReadOnly] public int index;
  [ReadOnly] public int loopIterations;
  [ReadOnly] public float timeDelta;
  [ReadOnly] public NativeArray<Vector2> positions;
  [ReadOnly] public NativeArray<float> speeds;
  [ReadOnly] public NativeArray<UnsafeList<Vector2>> paths;

  public void Execute()
  {
    InitializePopulation();

    for (int i = 0; i < loopIterations; i++) 
    {
      CalculateFitness();
      ApplySelection();
      ApplyOperators();
      SelectNewPopulation();
    }

    SetWinner();
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

  /// <summary>
  /// Set return velocity.
  /// </summary>
  private void SetWinner()
  {

  }
}
