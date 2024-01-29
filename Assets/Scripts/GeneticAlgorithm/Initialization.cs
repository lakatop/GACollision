using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct BasicInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float timeDelta;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float rotationRange = 120f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      for (int j = 0; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * timeDelta;
        individual.path[j] = new float2(rotation, size);
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
[BurstCompile]
public struct DebugInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;
  [ReadOnly] public float previousVelocity;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var pathSize = 10;

    var individual = new BasicIndividualStruct();
    individual.Initialize(pathSize, Allocator.TempJob);
    individual.path.Add(new float2(0, 1));

    for (int j = 0; j < pathSize - 1; j++)
    {
      if (j == 3 || j == 4 || j ==5)
      {
        individual.path.Add(new float2(0, -1));
      }
      else if (j == 6)
      {
        individual.path.Add(new float2(0, 0.5f));
      }
      else
      {
        individual.path.Add(new float2(0, 1));
      }
    }
    currentPopulation[0] = individual;

    //var individual2 = new BasicIndividualStruct();
    //individual2.Initialize(pathSize, Allocator.TempJob);
    //individual2.path.Add(new float2(0, 2.5f));

    //for (int j = 0; j < pathSize - 2; j++)
    //{
    //  individual2.path.Add(new float2(0, 2.5f));
    //}
    //individual2.path.Add(new float2(0, 0));
    //currentPopulation[1] = individual2;


    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var placeOrigin = startPosition;
      var rotationVector = forward.normalized;
      var path = currentPopulation[i].path;
      var prevVelocity = previousVelocity;

      for (int j = 0; j < path.Length; j++)
      {

        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        var acc = 1 * path[j].y;
        var velocity = prevVelocity + acc;
        velocity = Mathf.Clamp(velocity, 0, 2.5f);
        var rotatedAndTranslatedVector = rotatedVector * velocity;
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedVector.x, 0f, rotatedVector.y), new Color(0, 1, 0), 50, false);
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, placeOrigin);
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
        prevVelocity = velocity;
      }
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}


[BurstCompile]
public struct GlobeInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float initRotationRange = 360 / populationSize;
    float rotationRange = 30;
    float initRotation = 0f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.path[0] = new float2(initRotation, agentSpeed * updateInterval);
      initRotation += initRotationRange;

      for (int j = 1; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var size = _rand.NextFloat(agentSpeed) * updateInterval;
        individual.path[j] = new float2(rotation, size);
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
[BurstCompile]
public struct KineticFriendlyInitialization : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public int populationSize;
  [ReadOnly] public int pathSize;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    float initRotationRange = 60 / populationSize;
    float rotationRange = 15;
    float initRotation = -30f;

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.path[0] = new float2(initRotation, (_rand.NextFloat() * 2f) - 1f);
      initRotation += initRotationRange;

      for (int j = 1; j < pathSize; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange);
        var acc = (_rand.NextFloat() * 2f) - 1f;
        individual.path[j] = new float2(rotation, acc);
      }
      currentPopulation[i] = individual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}


[BurstCompile]
public struct BezierInitialization : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public int populationSize;
  [ReadOnly] public float agentSpeed;
  [ReadOnly] public float updateInterval;
  [ReadOnly] public int pathSize;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 endPosition;
  [ReadOnly] public Vector2 forward;
  [ReadOnly] public Unity.Mathematics.Random _rand;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    float maxDeg = 30;
    float quadDistance = (endPosition - startPosition).magnitude / 4;
    float controlPointLenght = Mathf.Tan(maxDeg * Mathf.Deg2Rad) * quadDistance;
    float subFactor = (controlPointLenght * 2) / populationSize;

    for(int i = 0; i < currentPopulation.Length; i++)
    {
      var individual = currentPopulation[i];
      individual.bezierCurve.CreateInitialPath(startPosition, endPosition, forward, controlPointLenght);
      controlPointLenght -= subFactor;
      for(int j = 0; j < pathSize; j++)
      {
        var acc = (_rand.NextFloat() * 2f) - 1f;
        individual.accelerations[j] = acc;
      }
      currentPopulation[i] = individual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}