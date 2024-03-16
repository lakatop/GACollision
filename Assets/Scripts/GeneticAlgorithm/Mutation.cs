using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Unity.Burst;
using NativeQuadTree;

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
  [ReadOnly] public float _rotationRange;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb > 0.5)
      {
        var individual = currentPopulation[i];
        for (int j = 0; j < individual.path.Length; j++)
        {
          var acc = (_rand.NextFloat() * 2) - 1;
          var angle = _rand.NextFloat(-_rotationRange, _rotationRange);
          var temp = individual.path[j];
          temp.x = angle;
          temp.y = acc;
          individual.path[j] = temp;
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

// -------------- Bezier mutations --------------

[BurstCompile]
public struct BezierStraightFinishMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public Vector2 startPos;
  [ReadOnly] public Vector2 destination;
  [ReadOnly] public Vector2 forward;
  [ReadOnly] public float _agentSpeed;
  [ReadOnly] public float _updateInterval;
  [ReadOnly] public float startVelocity;
  [ReadOnly] public float maxAcc;
  [ReadOnly] public float _mutationProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var mutProb = _rand.NextFloat();
    if (mutProb > _mutationProb)
      return;

    // Take last individual
    var individual = currentPopulation[currentPopulation.Length - 1];

    // Check whether we can go directly to destination
    var maxDeg = 30;
    var vectorToDestination = (destination - startPos);
    var angle = Vector2.SignedAngle(vectorToDestination, forward);

    if (Mathf.Abs(angle) > maxDeg)
    {
      return;
    }

    // Create straight bezier from currentPosition to destination
    individual.bezierCurve.points[0] = startPos;
    individual.bezierCurve.points[1] = startPos;
    individual.bezierCurve.points[2] = destination;
    individual.bezierCurve.points[3] = destination;

    var prevVelocity = startVelocity;
    var currentPosition = startPos;

    for (int i = 0; i < individual.accelerations.Length; i++)
    {
      var destinationDistance = (destination - currentPosition).magnitude;

      var maxAcceptableVelocity = UtilsGA.UtilsGA.CalculateMaxVelocity(destinationDistance);

      var newVelocity = Mathf.Clamp(maxAcceptableVelocity, 0, _agentSpeed * _updateInterval);
      var newAcc = newVelocity - prevVelocity;
      newAcc = Mathf.Clamp(newAcc, -1, maxAcc);
      newVelocity = prevVelocity + newAcc;
      individual.accelerations[i] = newAcc;
      prevVelocity = newVelocity;
      currentPosition = currentPosition + ((destination - currentPosition).normalized * newVelocity);
    }


    currentPopulation[currentPopulation.Length - 1] = individual;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetMutationProbabilty()
  {
    return _mutationProb;
  }

  public void Dispose()
  {
  }
}


[BurstCompile]
public struct BezierClampVelocityMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public float _agentSpeed;
  [ReadOnly] public float _updateInterval;
  [ReadOnly] public float startVelocity;
  [ReadOnly] public float maxAcc;
  [ReadOnly] public float _mutationProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb > _mutationProb)
        return;

      var individual = currentPopulation[i];

      var currentAcc = maxAcc * individual.accelerations[0];
      var velocity = startVelocity + currentAcc;
      velocity = Mathf.Clamp(velocity, 0, _updateInterval * _agentSpeed);


      // Calculate how long path is to destination
      var destinationDistance = 0f;
      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var previousPoint = individual.bezierCurve.points[0];
      var t = 0f;
      while(t <= 1)
      {
        t += 1f / divisions;
        Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
          individual.bezierCurve.points[0],
          individual.bezierCurve.points[1],
          individual.bezierCurve.points[2],
          individual.bezierCurve.points[3],
          t);

        destinationDistance += (pointOncurve - previousPoint).magnitude;
        previousPoint = pointOncurve;
      }

      // Calculate maximum velocity that we can go if we want to decelerate to 0 in destination
      var maxAcceptableVelocity = UtilsGA.UtilsGA.CalculateMaxVelocity(destinationDistance);

      // If true, we need to deccelerate
      if(velocity > maxAcceptableVelocity)
      {
        var newAcc = maxAcceptableVelocity - velocity;
        newAcc = Mathf.Clamp(newAcc, -1, 1);
        individual.accelerations[0] = newAcc;
      }

      currentPopulation[i] = individual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetMutationProbabilty()
  {
    return _mutationProb;
  }

  public void Dispose()
  {
  }
}


[BurstCompile]
public struct BezierSmoothAccMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public float _mutationProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb > _mutationProb)
        return;

      var individual = currentPopulation[i];

      for (int j = 1; j < individual.accelerations.Length; j++)
      {
        var acc1 = individual.accelerations[j];
        var acc2 = individual.accelerations[j - 1];

        var mean = (acc1 + acc2) / 2;
        individual.accelerations[j] = mean;
        individual.accelerations[j - 1] = mean;
      }

      currentPopulation[i] = individual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetMutationProbabilty()
  {
    return _mutationProb;
  }

  public void Dispose()
  {
  }
}

[BurstCompile]
public struct BezierShuffleAccMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public float _mutationProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      // Low mutation rate because we are counting on other mutation to smooth accelerations
      if (mutProb > _mutationProb)
        return;

      var individual = currentPopulation[i];

      for (int j = 0; j < individual.accelerations.Length; j++)
      {
        // Also dont change every acceleration, just some
        mutProb = _rand.NextFloat();
        if (mutProb > _mutationProb)
          continue;

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

  public float GetMutationProbabilty()
  {
    return _mutationProb;
  }

  public void Dispose()
  {
  }
}

/// <summary>
/// Bezier individual mutation.
/// Defines new position of control points randomly selected from appropriate space.
/// </summary>
[BurstCompile]
public struct BezierShuffleControlPointsMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  [ReadOnly] public Vector2 startPosition;
  [ReadOnly] public Vector2 endPosition;
  [ReadOnly] public Vector2 forward;
  [ReadOnly] public float _mutationProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = _rand.NextFloat();
      if (mutProb > _mutationProb)
        return;

      // Define restrictions on control points position
      var individual = currentPopulation[i];
      float maxDeg = 30;
      float halfDistance = (endPosition - startPosition).magnitude / 2;
      float upDistance = _rand.NextFloat(halfDistance);
      float controlPointLenght = Mathf.Tan(maxDeg * Mathf.Deg2Rad) * upDistance;
      float sideDistance = _rand.NextFloat(-controlPointLenght, controlPointLenght);

      // Calculate position of new P1 and P2 control points
      var newP1 = startPosition + ((forward.normalized * upDistance) + (Vector2.Perpendicular(forward.normalized) * sideDistance));
      var P2Dir = (startPosition - endPosition);
      var newP2 = endPosition + (P2Dir.normalized * upDistance) + (Vector2.Perpendicular((endPosition - startPosition).normalized) * sideDistance);

      // Replace old contorl points with new ones
      individual.bezierCurve.points[1] = newP1;
      individual.bezierCurve.points[2] = newP2;

      // Replace old individual
      currentPopulation[i] = individual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetMutationProbabilty()
  {
    return _mutationProb;
  }

  public void Dispose()
  {
  }
}

/// -------------------- Invalidated because of different individual representation --------------------

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
    var mutationRate = 0.5f;
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