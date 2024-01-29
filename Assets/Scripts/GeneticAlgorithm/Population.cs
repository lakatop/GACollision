using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public class BasicIndividual
{
  public BasicIndividual()
  {
    fitness = 0f;
    path = new List<float2>();
  }

  public float fitness { get; set; }
  public List<float2> path { get; set; }
}

public struct BasicIndividualStruct
{
  public void Initialize(int length, Allocator allocator)
  {
    fitness = 0f;
    path = new UnsafeList<float2>(length, allocator);
  }

  public void Dispose()
  {
    path.Dispose();
  }

  public float fitness;
  public UnsafeList<float2> path;
}

public class BasicPopulation : IPopulation<BasicIndividual>
{
  public BasicPopulation()
  {
    _population = new List<BasicIndividual>();
  }

  public BasicIndividual[] GetPopulation()
  {
    return _population.ToArray();
  }

  public void SetPopulation(BasicIndividual[] population)
  {
    _population = new List<BasicIndividual>(population);
  }

  private List<BasicIndividual> _population { get; set; }
}

[BurstCompile]
public struct NativeBasicPopulation : IParallelPopulation<BasicIndividualStruct>
{
  [BurstCompile]
  public void Dispose()
  {
    foreach(var individual in _population)
    {
      individual.Dispose();
    }
    _population.Dispose();
  }

  public NativeArray<BasicIndividualStruct> _population;
}

[BurstCompile]
public struct BasicIndividualSortDescending : IComparer<BasicIndividualStruct>
{
  [BurstCompile]
  public int Compare(BasicIndividualStruct x, BasicIndividualStruct y)
  {
    if (x.fitness < y.fitness)
    {
      return 1;
    }

    if(x.fitness > y.fitness)
    {
      return -1;
    }

    return 0;
  }
}

[BurstCompile]
public struct BasicIndividualSortAscending : IComparer<BasicIndividualStruct>
{
  [BurstCompile]
  public int Compare(BasicIndividualStruct x, BasicIndividualStruct y)
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
public struct NativeBasicPopulationDrawer
{
  public UnityEngine.Vector2 startPosition;
  public UnityEngine.Vector2 forward;

  public void DrawPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        rotatedVector = rotatedVector * path[j].y;

        UnityEngine.Debug.DrawRay(new UnityEngine.Vector3(placeOrigin.x, 0f, placeOrigin.y),
          new UnityEngine.Vector3(rotatedVector.x, 0f, rotatedVector.y), new UnityEngine.Color(0, 1, 0), 0.5f, false);

        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector.normalized;
      }
    }
  }
}

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
public struct NativeBezierPopulation : IParallelPopulation<BasicIndividualStruct>
{
  [BurstCompile]
  public void Dispose()
  {
    foreach (var individual in _population)
    {
      individual.Dispose();
    }
    _population.Dispose();
  }

  public NativeArray<BezierIndividualStruct> _population;
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
          new UnityEngine.Vector3(endDraw.x, 0, endDraw.y), new UnityEngine.Color(0, 1, 0), 1, false);
        startDraw = endDraw;
      }
    }
  }
}