using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Unity.Burst;

public class BasicMutationOperator : IPopulationModifier<BasicIndividual>
{
  System.Random _rand = new System.Random();
  float _agentSpeed { get; set; }
  float _timeDelta { get; set; }

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    var population = currentPopulation.GetPopulation();
    for (int i = 0; i < population.Length; i++)
    {
      for (int j = 0; j < population[i].path.Count; j++)
      {
        // Mutation with probability 0.2
        var mutProb = _rand.NextDouble();
        if (mutProb > 0.8f)
        {
          var size = UnityEngine.Random.Range(0f, _agentSpeed) * _timeDelta;
          float2 newVal = population[i].path[j];
          newVal.y = size;
          population[i].path[j] = newVal;
        }
      }
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 2);

    _agentSpeed = (float)resources[0];
    _timeDelta = (float)resources[1];
  }
}

[BurstCompile]
public struct BasicMutationOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public float _agentSpeed;
  [ReadOnly] public float _updateInterval;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      for (int j = 0; j < currentPopulation[i].path.Length; j++)
      {
        // Mutation with probability 0.2
        var mutProb = _rand.NextFloat();
        if (mutProb > 0.8f)
        {
          var size = _rand.NextFloat(_agentSpeed) * _updateInterval;
          float2 newVal = currentPopulation[i].path[j];
          newVal.y = size;
          var tempPop = currentPopulation;
          var tempPath = tempPop[i].path;
          tempPath[j] = newVal;
          currentPopulation = tempPop;
        }
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
/// Rotate towards destination in even circular movement if we can make it in single path
/// Only if there is special case when we can go straight to destination by single vector, use that instead
/// </summary>
[BurstCompile]
public struct EvenCircleMutationOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public Vector2 _destination;
  [ReadOnly] public Vector2 _agentPosition;
  [ReadOnly] public Vector2 _forward;
  [ReadOnly] public float _rotationAngle;
  [ReadOnly] public float _agentSpeed;
  [ReadOnly] public float _updateInterval;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    // How often we want mutation to happen
    var mutationRate = 0.3f;
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb < 1 - mutationRate)
        continue;

      var individual = currentPopulation[i];

      var rotationVector = _forward.normalized;
      var seg1 = individual.path[0];
      var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, seg1.x);

      var straightVectorToDestination = (_destination - _agentPosition);
      var startAngle = -Vector2.SignedAngle(straightVectorToDestination, rotatedVector);

      // Special case when we can go straight to the destination with single vector
      if (Mathf.Abs(startAngle) < _rotationAngle
        && straightVectorToDestination.magnitude < (_agentSpeed * _updateInterval))
      {
        individual.path[0] = new float2 { x = startAngle, y = straightVectorToDestination.magnitude };
        for (int j = 1; j < individual.path.Length; j++)
        {
          individual.path[j] = new float2 { x = 0, y = 0 };
        }

        currentPopulation[i] = individual;
        continue;
      }

      // Check if we can achieve turning towards the destination in smooth circle motion
      var maxAngleChange = (individual.path.Length - 1) * _rotationAngle;
      // * 2 because first half of circle will take angle, second is symmetrical
      // only acute angles
      if (maxAngleChange < startAngle * 2 && (startAngle >= 90 || startAngle <= -90))
        continue;

      // An arc with n segments has n-1 turning joints
      float angleIncrement = 2 * startAngle / (individual.path.Length - 1);

      float totalLength = 0;
      for (int j = 0; j < individual.path.Length; j++)
      {
        float segmentAngle = startAngle - j * angleIncrement;
        totalLength += Mathf.Cos(segmentAngle * Mathf.Deg2Rad);
      }

      var uniformSegmentSize = straightVectorToDestination.magnitude / totalLength;

      // We wont be able to make it in single path
      // Go as further as we can
      if (uniformSegmentSize > _agentSpeed * _updateInterval)
      {
        rotatedVector = rotatedVector * _agentSpeed * _updateInterval;
        var rotatedAndTranslated = _agentPosition + rotatedVector;

        var radius = UtilsGA.UtilsGA.GetCircleRadius(
          new System.Numerics.Complex(_agentPosition.x, _agentPosition.y),
          new System.Numerics.Complex(_destination.x, _destination.y),
          new System.Numerics.Complex(rotatedAndTranslated.x, rotatedAndTranslated.y));

        if (radius < 0)
          continue;

        var baseHalf = (_agentSpeed * _updateInterval) / 2;
        var stepAngle = 2 * Mathf.Asin((float)(baseHalf / radius));

        var stepAngleDegrees = stepAngle * Mathf.Rad2Deg;

        // Create a new path
        individual.path[0] = new float2 { x = individual.path[0].x, y = _agentSpeed * _updateInterval };
        for (int j = 1; j < individual.path.Length; j++)
        {
          individual.path[j] = new float2 { x = stepAngleDegrees, y = _agentSpeed * _updateInterval };
        }

        currentPopulation[i] = individual;
        continue;
      }

      // Create a new path
      individual.path[0] = new float2 { x = individual.path[0].x, y = uniformSegmentSize };
      for (int j = 1; j < individual.path.Length; j++)
      {
        individual.path[j] = new float2 { x = angleIncrement, y = uniformSegmentSize };
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


/// <summary>
/// Rotate towards destination in "greedy" circular movement
///   - start with max velocity till you can, then slow down
///   - may break _rotationAngle restriction
/// Only if there is special case when we can go straight to destination by single vector, use that instead
/// </summary>
[BurstCompile]
public struct GreedyCircleMutationOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public Vector2 _destination;
  [ReadOnly] public Vector2 _agentPosition;
  [ReadOnly] public Vector2 _forward;
  [ReadOnly] public float _rotationAngle;
  [ReadOnly] public float _agentSpeed;
  [ReadOnly] public float _updateInterval;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    // How often we want mutation to happen
    var mutationRate = 1f;
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb < 1 - mutationRate)
        continue;

      var individual = currentPopulation[i];

      var straightVectorToDestination = (_destination - _agentPosition);
      var startAngle = Vector2.SignedAngle(straightVectorToDestination, _forward);

      // Special case when we can go straight to the destination with single vector
      if (straightVectorToDestination.magnitude < (_agentSpeed * _updateInterval))
      {
        individual.path[0] = new float2 { x = -startAngle, y = straightVectorToDestination.magnitude };
        for (int j = 1; j < individual.path.Length; j++)
        {
          individual.path[j] = new float2 { x = 0, y = 0 };
        }

        currentPopulation[i] = individual;
        continue;
      }

      var remainingLength = straightVectorToDestination.magnitude;
      var index = 0;
      var maxMove = _agentSpeed * _updateInterval;

      // Straight line to destination
      if (-_rotationAngle < startAngle && startAngle < _rotationAngle)
      {
        var turnAngle = startAngle;
        while (remainingLength > 0 && index < individual.path.Length)
        {
          // First path will turn towards the direction, rest will be straight line
          individual.path[index] = new float2 { x = -turnAngle, y = maxMove };
          turnAngle = 0;
          remainingLength -= maxMove;
          index++;

          // We went too far, replace last segment with line straight to destination
          if (remainingLength < 0)
          {
            individual.path[index - 1] = new float2 { x = 0, y = remainingLength + maxMove };
          }
        }

        for (int j = index; j < individual.path.Length; j++)
        {
          individual.path[index] = new float2 { x = 0, y = 0 };
        }

        currentPopulation[i] = individual;
        continue;
      }

      // Create greedy circle rotation towards the destination
      var rotationVector = _forward.normalized;
      var seg1 = individual.path[0];
      var rotationSign = seg1.x > 0 ? -1 : 1;
      var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, seg1.x);
      rotatedVector = rotatedVector * maxMove;

      var pos = _agentPosition;
      var rotatedAndTranslated = pos + rotatedVector;
      var radius = UtilsGA.UtilsGA.GetCircleRadius(
        new System.Numerics.Complex(_agentPosition.x, _agentPosition.y),
        new System.Numerics.Complex(_destination.x, _destination.y),
        new System.Numerics.Complex(rotatedAndTranslated.x, rotatedAndTranslated.y));

      if (radius < 0)
        continue;

      individual.path[0] = new float2 { x = seg1.x, y = maxMove };
      index++;

      do
      {
        // We can go straight to destination by 1 move
        if((rotatedAndTranslated - _destination).magnitude < maxMove)
        {
          individual.path[index] = new float2 { x = Vector2.SignedAngle(rotatedVector, (_destination - rotatedAndTranslated)), y = (rotatedAndTranslated - _destination).magnitude };
          index++;
          break;
        }

        var baseHalf = maxMove / 2;
        var stepAngle = 2 * Mathf.Asin((float)(baseHalf / radius));

        var stepAngleDegrees = stepAngle * Mathf.Rad2Deg;
        individual.path[index] = new float2 { x = stepAngleDegrees * rotationSign, y = maxMove };
        index++;

        rotationVector = rotatedVector.normalized;
        rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, stepAngleDegrees * rotationSign);
        rotatedVector = rotatedVector * maxMove;
        rotatedAndTranslated = rotatedAndTranslated + rotatedVector;

      } while (index < individual.path.Length);

      // We cut loop too early => we arrived in destination
      // Fill the rest of the path with zeros
      for(int j = index; j < individual.path.Length; j++)
      {
        individual.path[j] = new float2 { x = 0, y = 0 };
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