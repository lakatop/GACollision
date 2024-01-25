using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct WeightedSumRanking : IParallelPopulationModifier<BasicIndividualStruct>
{
  NativeArray<float> resultingFitnesses;


  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    //Write resultingFitness to individuals
  }

  public void CalculateRanking(ref NativeArray<float> fitnessValues1, ref NativeArray<float> fitnessValues2, ref NativeArray<float> fitnessValues3,
                               float weight1, float weight2, float weight3)
  {
    //Calculate resultingFitness using Z-score
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}