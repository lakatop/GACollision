using Unity.Collections;
using UnityEngine;
using Unity.Burst;


[BurstCompile]
public struct BezierWeightedSumRanking : IParallelPopulationModifier<BezierIndividualStruct>
{
  public NativeArray<float> resultingFitnesses;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    //Write resultingFitness to individuals
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = resultingFitnesses[i];
      currentPopulation[i] = temp;
    }
  }

  public void CalculateRanking(ref NativeArray<float> fitnessValues1, ref NativeArray<float> fitnessValues2,
                               ref NativeArray<float> fitnessValues3, ref NativeArray<float> fitnessValues4,
                               float weight1, float weight2, float weight3, float weight4)
  {
    //Calculate resultingFitness using Z-score
    var normF1 = new NativeArray<float>(fitnessValues1.Length, Allocator.Temp);
    var normF2 = new NativeArray<float>(fitnessValues2.Length, Allocator.Temp);
    var normF3 = new NativeArray<float>(fitnessValues3.Length, Allocator.Temp);
    var normF4 = new NativeArray<float>(fitnessValues4.Length, Allocator.Temp);

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
    if (stdDev == 0)
    {
      for (int i = 0; i < fitnessValues1.Length; i++)
      {
        normF1[i] = 0;
      }
    }
    else
    {
      for (int i = 0; i < fitnessValues1.Length; i++)
      {
        normF1[i] = (fitnessValues1[i] - mean) / stdDev;
      }
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
    if (stdDev == 0)
    {
      for (int i = 0; i < fitnessValues2.Length; i++)
      {
        normF2[i] = 0;
      }
    }
    else
    {
      for (int i = 0; i < fitnessValues2.Length; i++)
      {
        normF2[i] = (fitnessValues2[i] - mean) / stdDev;
      }
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
    if (stdDev == 0)
    {
      for (int i = 0; i < fitnessValues3.Length; i++)
      {
        normF3[i] = 0;
      }
    }
    else
    {
      for (int i = 0; i < fitnessValues3.Length; i++)
      {
        normF3[i] = (fitnessValues3[i] - mean) / stdDev;
      }
    }

    // Z-score normalization of third fitnesses
    mean = 0f;

    for (int i = 0; i < fitnessValues4.Length; i++)
    {
      mean += fitnessValues4[i];
    }
    mean = mean / fitnessValues4.Length;

    squaredSum = 0f;
    for (int i = 0; i < fitnessValues4.Length; i++)
    {
      squaredSum += ((mean - fitnessValues4[i]) * (mean - fitnessValues4[i]));
    }

    variance = squaredSum / fitnessValues4.Length;
    stdDev = Mathf.Sqrt(variance);
    if (stdDev == 0)
    {
      for (int i = 0; i < fitnessValues4.Length; i++)
      {
        normF4[i] = 0;
      }
    }
    else
    {
      for (int i = 0; i < fitnessValues4.Length; i++)
      {
        normF4[i] = (fitnessValues4[i] - mean) / stdDev;
      }
    }

    for (int i = 0; i < resultingFitnesses.Length; i++)
    {
      resultingFitnesses[i] = normF1[i] * weight1 + normF2[i] * weight2 + normF3[i] * weight3 + normF4[i] * weight4;
    }

    normF1.Dispose();
    normF2.Dispose();
    normF3.Dispose();
    normF4.Dispose();
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