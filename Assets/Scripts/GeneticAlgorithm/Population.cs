using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


[BurstCompile]
public struct BezierIndividualStruct
{
  public void Initialize(int length, Allocator allocator)
  {
    fitness = 0f;
    bezierCurve.Initialize(length * 3, allocator);
    accelerations = new UnsafeList<float>(length, allocator);
    accelerations.Resize(length);
  }

  public void Dispose()
  {
    bezierCurve.Dispose();
    accelerations.Dispose();
  }

  public float fitness;
  public BezierCurve bezierCurve;
  public UnsafeList<float> accelerations;
}

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

[BurstCompile]
public struct NativeBezierPopulation : IParallelPopulation<BezierIndividualStruct>
{
  [BurstCompile]
  public void Dispose()
  {
    foreach (var individual in population)
    {
      individual.Dispose();
    }
    population.Dispose();
  }

  public NativeArray<BezierIndividualStruct> population;
}

[BurstCompile]
public struct BezierPopulationDrawer
{
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
