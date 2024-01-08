using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct BasicInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float timeDelta;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    float rotationRange = 120f;

    for (int i = 0; i < populationSize; i++)
    {
      var individual = new BasicIndividualStruct();
      individual.Initialize(pathSize, Allocator.TempJob);
      for (int j = 0; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * timeDelta;
        individual.path.Add(new float2(rotation, size));
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.5f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y));
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}

/// <summary>
/// Initial rotation range is 60 degree cone (-30 - 30)
/// After that, only 5 degree rotations are allowed
/// </summary>
public struct DebugInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    var pathSize = 10;

    var individual = new BasicIndividualStruct();
    individual.Initialize(pathSize, Allocator.TempJob);
    individual.path.Add(new float2(45, 1));

    var individual2 = new BasicIndividualStruct();
    individual2.Initialize(pathSize, Allocator.TempJob);
    individual2.path.Add(new float2(-45, 1));

    for (int j = 0; j < pathSize - 1; j++)
    {
      individual.path.Add(new float2(0, 1));
      individual2.path.Add(new float2(0, 1));
    }
    currentPopulation[0] = individual;
    currentPopulation[1] = individual2;


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y), new Color(0, 1, 0), 50, false);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}


public struct GlobeInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    float initRotationRange = 360 / populationSize;
    float rotationRange = 30;
    float initRotation = 0f;

    for (int i = 0; i < populationSize; i++)
    {
      var individual = new BasicIndividualStruct();
      individual.Initialize(pathSize, Allocator.TempJob);
      individual.path.Add(new float2(initRotation, agentSpeed * updateInterval));
      initRotation += initRotationRange;

      for (int j = 0; j < pathSize - 1; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * updateInterval;
        individual.path.Add(new float2(rotation, size));
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.2f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y));
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}

/// <summary>
/// Initial rotation range is 60 degree cone (-30 - 30)
/// After that, only 5 degree rotations are allowed
/// </summary>
public struct KineticFriendlyInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    float initRotationRange = 120 / populationSize;
    float rotationRange = 15;
    float initRotation = -60f;

    for (int i = 0; i < populationSize; i++)
    {
      var individual = new BasicIndividualStruct();
      individual.Initialize(pathSize, Allocator.TempJob);
      individual.path.Add(new float2(initRotation, agentSpeed * updateInterval));
      initRotation += initRotationRange;

      for (int j = 0; j < pathSize - 1; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * updateInterval;
        individual.path.Add(new float2(rotation, size));
      }
      currentPopulation[i] = individual;
    }


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized * 0.2f;
      var path = currentPopulation[i].path;

      for (int j = 0; j < path.Length; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y), new Color(0,1,0), 50, false);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
      }
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}
