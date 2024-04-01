using Unity.Collections;
using UnityEngine;
using Unity.Burst;

/// <summary>
/// Weighted sum for Bezier individual fitnesses
/// </summary>
[BurstCompile]
public struct BezierWeightedSumRanking : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Resulting fitnesses for each individual
  /// </summary>
  public NativeArray<float> resultingFitnesses;

  /// <summary>
  /// Set fitness for population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
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

  /// <summary>
  /// Perform weighted sum calculation from population fitnesses
  /// </summary>
  /// <param name="fitnessValues1">Array of first fitnesses</param>
  /// <param name="fitnessValues2">Array of second fitnesses</param>
  /// <param name="fitnessValues3">Array of third fitnesses</param>
  /// <param name="fitnessValues4">Array of fourth fitnesses</param>
  /// <param name="weight1">Weight of first fitnesses</param>
  /// <param name="weight2">Weight of second fitnesses</param>
  /// <param name="weight3">Weight of third fitnesses</param>
  /// <param name="weight4">Weight of fourth fitnesses</param>
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

    // Z-score normalization of fourth fitnesses
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
    resultingFitnesses.Dispose();
  }
}