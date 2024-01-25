using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct WeightedSumRanking : IParallelPopulationModifier<BasicIndividualStruct>
{
  public NativeArray<float> resultingFitnesses;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    //Write resultingFitness to individuals
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = resultingFitnesses[i];
      currentPopulation[i] = temp;
    }
  }

  public void CalculateRanking(ref NativeArray<float> fitnessValues1, ref NativeArray<float> fitnessValues2, ref NativeArray<float> fitnessValues3,
                               float weight1, float weight2, float weight3)
  {
    //Calculate resultingFitness using Z-score
    var normF1 = new NativeArray<float>(fitnessValues1.Length, Allocator.Temp);
    var normF2 = new NativeArray<float>(fitnessValues2.Length, Allocator.Temp);
    var normF3 = new NativeArray<float>(fitnessValues3.Length, Allocator.Temp);

    // Z-score normalization of first fitnesses
    float mean = 0f;
    for (int i = 0; i < fitnessValues1.Length; i++)
    {
      mean += fitnessValues1[i];
    }
    mean = mean / fitnessValues1.Length;

    var squaredSum = 0f;
    for (int i = 0; i < fitnessValues1.Length; i++)
    {
      squaredSum += ((mean - fitnessValues1[i]) * (mean - fitnessValues1[i]));
    }

    var variance = squaredSum / fitnessValues1.Length;
    var stdDev = Mathf.Sqrt(variance);
    for (int i = 0; i < fitnessValues1.Length; i++)
    {
      normF1[i] = (fitnessValues1[i] - mean) / stdDev;
    }

    // Z-score normalization of second fitnesses
    mean = 0f;

    for (int i = 0; i < fitnessValues2.Length; i++)
    {
      mean += fitnessValues2[i];
    }
    mean = mean / fitnessValues2.Length;

    squaredSum = 0f;
    for (int i = 0; i < fitnessValues2.Length; i++)
    {
      squaredSum += ((mean - fitnessValues2[i]) * (mean - fitnessValues2[i]));
    }

    variance = squaredSum / fitnessValues2.Length;
    stdDev = Mathf.Sqrt(variance);
    for (int i = 0; i < fitnessValues2.Length; i++)
    {
      normF2[i] = (fitnessValues2[i] - mean) / stdDev;
    }

    // Z-score normalization of third fitnesses
    mean = 0f;

    for (int i = 0; i < fitnessValues3.Length; i++)
    {
      mean += fitnessValues3[i];
    }
    mean = mean / fitnessValues3.Length;

    squaredSum = 0f;
    for (int i = 0; i < fitnessValues3.Length; i++)
    {
      squaredSum += ((mean - fitnessValues3[i]) * (mean - fitnessValues3[i]));
    }

    variance = squaredSum / fitnessValues3.Length;
    stdDev = Mathf.Sqrt(variance);
    for (int i = 0; i < fitnessValues3.Length; i++)
    {
      normF3[i] = (fitnessValues3[i] - mean) / stdDev;
    }

    for (int i = 0; i < resultingFitnesses.Length; i++)
    {
      resultingFitnesses[i] = normF1[i] * weight1 + normF2[i] * weight2 + normF3[i] * weight3;
    }

    normF1.Dispose();
    normF2.Dispose();
    normF3.Dispose();
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    resultingFitnesses.Dispose();
  }
}