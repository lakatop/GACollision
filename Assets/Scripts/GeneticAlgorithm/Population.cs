using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Bezier individual struct designed to be used inside of Unity jobs
/// </summary>
[BurstCompile]
public struct BezierIndividualStruct
{
  /// <summary>
  /// Initialize individual
  /// </summary>
  /// <param name="length">Number of control points</param>
  /// <param name="allocator">Storage allocator</param>
  public void Initialize(int length, Allocator allocator)
  {
    fitness = 0f;
    bezierCurve.Initialize(4, allocator); // 4 for cubic bezier curve
    accelerations = new UnsafeList<float>(length, allocator);
    accelerations.Resize(length);
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    bezierCurve.Dispose();
    accelerations.Dispose();
  }

  /// <summary>
  /// Fitness of individual
  /// </summary>
  public float fitness;
  /// <summary>
  /// Bezier curve of individual representing path
  /// </summary>
  public BezierCurve bezierCurve;
  /// <summary>
  /// List of agents accelerations on path
  /// </summary>
  public UnsafeList<float> accelerations;
}

/// <summary>
/// BezierIndividualStruct ascending comparer
/// </summary>
[BurstCompile]
public struct BezierIndividualSortAscending : IComparer<BezierIndividualStruct>
{
  [BurstCompile]
  public int Compare(BezierIndividualStruct x, BezierIndividualStruct y)
  {
    if (x.fitness > y.fitness)
    {
      return 1;
    }

    if (x.fitness < y.fitness)
    {
      return -1;
    }

    return 0;
  }
}

/// <summary>
/// BezierIndividualStruct population
/// </summary>
[BurstCompile]
public struct NativeBezierPopulation : IParallelPopulation<BezierIndividualStruct>
{
  /// <summary>
  /// Clear resources
  /// </summary>
  [BurstCompile]
  public void Dispose()
  {
    foreach (var individual in population)
    {
      individual.Dispose();
    }
    population.Dispose();
  }

  /// <summary>
  /// Population of individuals
  /// </summary>
  public NativeArray<BezierIndividualStruct> population;
}

/// <summary>
/// BezierIndividualStruct population debub drawer
/// </summary>
[BurstCompile]
public struct BezierPopulationDrawer
{
  /// <summary>
  /// Draw population using debug lines
  /// </summary>
  /// <param name="currentPopulation"></param>
  public void DrawPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      var resolution = 50;
      var startPos = individual.bezierCurve.points[0];
      var P1 = individual.bezierCurve.points[1];
      var P2 = individual.bezierCurve.points[2];
      var endPos = individual.bezierCurve.points[3];

      var startDraw = startPos;
      for (int j = 0; j < resolution; j++)
      {
        float t = j / (float)resolution;
        var endDraw = individual.bezierCurve.EvaluateCubic(startPos, P1, P2, endPos, t);
        UnityEngine.Debug.DrawLine(new UnityEngine.Vector3(startDraw.x, 0, startDraw.y),
          new UnityEngine.Vector3(endDraw.x, 0, endDraw.y), new UnityEngine.Color(0, 1, 0), 0.5f, false);
        startDraw = endDraw;
      }
    }
  }
}
