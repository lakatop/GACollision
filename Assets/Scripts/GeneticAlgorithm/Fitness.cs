using UnityEngine;
using NativeQuadTree;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Unity.Burst;

public class BasicFitnessFunction : IPopulationModifier<BasicIndividual>
{
  Vector2 _startPosition { get; set; }
  Vector2 _destination { get; set; }
  float _agentRadius { get; set; }
  int _agentIndex { get; set; }
  NativeQuadTree<TreeNode> _quadTree { get; set; }

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
    // Call Collides
    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
    // If doesnt collide, continue on next step.
    // At the end, check how far are we from destination
    var population = currentPopulation.GetPopulation();
    for (int i = 0; i < population.Length; i++)
    {
      var newPos = _startPosition;
      var rotationVector = newPos.normalized;

      var stepIndex = 1;
      foreach (var pos in population[i].path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, newPos);
        rotatedAndTranslatedVector = rotatedAndTranslatedVector * pos.y;

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        AABB2D bounds = new AABB2D(newPos, new float2(_agentRadius * 1.5f, _agentRadius * 1.5f));
        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
        _quadTree.RangeQuery(bounds, queryRes);

        if (UtilsGA.UtilsGA.Collides(newPos, queryRes, stepIndex, _agentRadius, _agentIndex) is var col && col > 0)
        {
          population[i].fitness = 0;
          break;
        }

        queryRes.Dispose();

        stepIndex++;
      }

      // We broke cycle before finishing - this individual is colliding
      if (stepIndex - 1 < population[i].path.Count)
      {
        continue;
      }

      var diff = (_destination - newPos).magnitude;
      float fitness;
      if (diff < 0.001f)
      {
        fitness = 1;
      }
      else
      {
        fitness = 1 / (_destination - newPos).magnitude;
      }
      population[i].fitness = fitness;
    }

    currentPopulation.SetPopulation(population);
    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 5);

    _startPosition = (Vector2)resources[0];
    _destination = (Vector2)resources[1];
    _agentRadius = (float)resources[2];
    _agentIndex = (int)resources[3];
    _quadTree = (NativeQuadTree<TreeNode>)resources[4];
  }
}


[BurstCompile]
public struct BasicFitnessFunctionParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
    // Call Collides
    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
    // If doesnt collide, continue on next step.
    // At the end, check how far are we from destination
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;
      foreach (var pos in currentPopulation[i].path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = rotatedVector * pos.y;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);


        if (UtilsGA.UtilsGA.Collides(_quadTree, newPos, rotatedAndTranslatedVector, _agentRadius, _agentIndex, stepIndex) is var col && col > 0)
        {
          var temp = currentPopulation[i];
          temp.fitness = 0;
          currentPopulation[i] = temp;
          break;
        }

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;
      }

      // We broke cycle before finishing - this individual is colliding
      if (stepIndex - 1 < currentPopulation[i].path.Length)
      {
        continue;
      }

      var diff = (_destination - newPos).magnitude;
      float fitness;
      if (diff < 0.001f)
      {
        fitness = 1000;
      }
      else
      {
        fitness = 1 / (_destination - newPos).magnitude;
      }
      var temp2 = currentPopulation[i];
      temp2.fitness = fitness;
      currentPopulation[i] = temp2;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose() { }
}


/// <summary>
/// "Along-the-way" fitness
/// Takes (position - destination).magnitude as initial fitness
/// For each position in individuals path calculates (pos - detination).magnitude and substracts this value ^2 from current fitness
/// Penalization -> if pos collides, it substracts value^3 (instead of value^2)
/// Warning: resulting fitness may be negative
/// </summary>
[BurstCompile]
public struct FitnessContinuousDistanceParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;
  public NativeArray<float> fitnesses;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var fitness = Mathf.Pow((_destination - _startPosition).magnitude, 2);

      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;

      foreach (var pos in individual.path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = rotatedVector * pos.y;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        if (UtilsGA.UtilsGA.Collides(_quadTree, newPos, rotatedAndTranslatedVector, _agentRadius, _agentIndex, stepIndex) is var col && col > 0)
        {
          fitness = (Mathf.Pow((_destination - newPos).magnitude, 5));
        }
        else
        {
          fitness -= Mathf.Pow((_destination - newPos).magnitude, 2);
        }

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;
      }

      fitnesses[index] = fitness;
      index++;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = fitnesses[i];
      currentPopulation[i] = temp;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

/// <summary>
/// Fitness that reacts relatively to segments along the path. Also checks for collisions.
/// </summary>
[BurstCompile]
public struct FitnessRelativeVectorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;
  public NativeArray<float> fitnesses;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var fitness = Mathf.Pow((_destination - _startPosition).magnitude, 2);

      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;

      foreach (var pos in individual.path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = rotatedVector * pos.y;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        var firstPosMagnintude = (_destination - newPos).magnitude;
        var secondPosMagnitude = (_destination - rotatedAndTranslatedVector).magnitude;
        var diff = firstPosMagnintude - secondPosMagnitude;

        // we are getting away from destination
        if(diff < 0)
        {
          fitness -= Mathf.Abs(diff) * 5; // penalization
        }
        else
        {
          fitness -= Mathf.Abs(diff);
        }

        // Also check for collisions
        if (UtilsGA.UtilsGA.Collides(_quadTree, newPos, rotatedAndTranslatedVector, _agentRadius, _agentIndex, stepIndex) is var col && col > 0)
        {
          // Take closer collisions more seriously
          fitness -= Mathf.Pow((individual.path.Length + 1 - stepIndex), 7);

          //fitness -= (Mathf.Pow((rotatedAndTranslatedVector - newPos).magnitude, individual.path.Length - stepIndex));
        }

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;

        // Last segment, also subtract that from fitness
        // Segments that end closer to the destination should be preferred
        if(stepIndex == individual.path.Length + 1)
        {
          fitness -= Mathf.Pow((_destination - rotatedAndTranslatedVector).magnitude, 2);
        }
      }

      fitnesses[index] = fitness;
      index++;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = fitnesses[i];
      currentPopulation[i] = temp;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

/// <summary>
/// Fitness for smooth path regarding the segments turning.
/// </summary>
[BurstCompile]
public struct FitnessAngleSumSmoothnessParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public NativeArray<float> fitnesses;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {

      float angleSum = 0;

      for (int i = 0; i < individual.path.Length; i++)
      {
        angleSum += individual.path[i].x;
      }

      // Check for straight paths, we cant divide by 0
      angleSum = (angleSum < 0.001f) ? 0.001f : angleSum;
      fitnesses[index] = 1 / angleSum;
      index++;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = fitnesses[i];
      currentPopulation[i] = temp;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

/// <summary>
/// Fitness calcualted using jerk cost
/// </summary>
[BurstCompile]
public struct FitnessJerkCostParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _forward;
  public NativeArray<float> fitnesses;
  public float weight;
  public float startVelocity;
  public float maxAcc;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;
      var rotationVector = _forward.normalized;
      var prevVelocity = startVelocity;

      var stepIndex = 1;

      NativeArray<Vector2> velocities = new NativeArray<Vector2>(individual.path.Length, Allocator.Temp);
      NativeArray<Vector2> accelerations = new NativeArray<Vector2>(velocities.Length - 1, Allocator.Temp);
      NativeArray<Vector2> jerks = new NativeArray<Vector2>(accelerations.Length - 1, Allocator.Temp);

      for (int i = 0; i < individual.path.Length; i++)
      {
        var pos = individual.path[i];
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var acc = maxAcc * pos.y; 
        var velocity = prevVelocity + acc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);
        var rotatedAndTranslatedVector = rotatedVector * velocity;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        velocities[i] = rotatedAndTranslatedVector - newPos;

        prevVelocity = velocity;
        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;
      }

      for (int i = 1; i < velocities.Length; i++)
      {
        accelerations[i - 1] = velocities[i] - velocities[i - 1]; 
      }

      for (int i = 1; i < accelerations.Length; i++)
      {
        jerks[i - 1] = accelerations[i] - accelerations[i - 1];
      }

      float sumSquaredMagnitude = 0f;
      foreach (var v in jerks)
      {
        sumSquaredMagnitude += v.sqrMagnitude;
      }

      float averageSqrMagnirude = sumSquaredMagnitude / jerks.Length;
      float averageMagnitude = Mathf.Sqrt(averageSqrMagnirude);

      fitnesses[index] = averageMagnitude;
      index++;

      velocities.Dispose();
      accelerations.Dispose();
      jerks.Dispose();
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

/// <summary>
/// Fitness calculating number of collision each individual will make along its path
/// </summary>
[BurstCompile]
public struct FitnessCollisionParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;
  public NativeArray<float> fitnesses;
  public float weight;
  public float maxAcc;
  public float startVelocity;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      float fitness = 0f;

      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;
      var prevVelocity = startVelocity;

      foreach (var pos in individual.path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var acc = maxAcc * pos.y;
        var velocity = prevVelocity + acc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);
        var rotatedAndTranslatedVector = rotatedVector * velocity;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        if (UtilsGA.UtilsGA.Collides(_quadTree, newPos, rotatedAndTranslatedVector, _agentRadius, _agentIndex, stepIndex) is var col && col > 0)
        {
          //PathDrawer.DrawCollisionPoint(newPos);
          //PathDrawer.DrawCollisionPoint(rotatedAndTranslatedVector);
          PathDrawer.DrawConnectionLine(newPos, rotatedAndTranslatedVector);
          fitness += col;
        }

        prevVelocity = velocity;
        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;
      }

      fitnesses[index] = fitness;
      index++;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

/// <summary>
/// Fitness calculating distance between end of individuals path and destination
/// </summary>
[BurstCompile]
public struct FitnessEndDistanceParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public Vector2 _forward;
  public NativeArray<float> fitnesses;
  public float weight;
  public float maxAcc;
  public float startVelocity;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;

      var prevVelocity = startVelocity;
      foreach (var pos in individual.path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var acc = maxAcc * pos.y;
        var velocity = prevVelocity + acc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);
        var rotatedAndTranslatedVector = rotatedVector * velocity;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        prevVelocity = velocity;
        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        stepIndex++;
      }

      fitnesses[index] = (_destination - newPos).magnitude;
      index++;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


// ------------ Bezier individual fitnesses ------------

[BurstCompile]
public struct BezierFitnessEndDistanceParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public NativeArray<float> fitnesses;
  public float weight;
  public float maxAcc;
  public float startVelocity;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;

      var alreadyTraveled = 0f;

      var fitness = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var prevVelocity = startVelocity;
      for (int i = 0; i < individual.accelerations.Length; i++)
      {
        var acc = individual.accelerations[i];
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;

        // If true, we overshoot the distance
        if (t >= 1 && i < individual.accelerations.Length)
        {
          // Calculate Distance To Destination
          var distanceToDestination = (_destination - newPos).magnitude;

          prevVelocity = individual.accelerations[i - 1];

          if (prevVelocity - distanceToDestination < maxAcc)
          {
            fitness = distanceToDestination;
          }
          else
          {
            var traveled = 0f;
            while (prevVelocity > 0)
            {
              if (prevVelocity - maxAcc > 0)
              {
                traveled += prevVelocity - maxAcc;
              }

              prevVelocity -= maxAcc;
            }

            fitness = ((traveled - distanceToDestination) * 2) + distanceToDestination;
          }

          break;
        }

        bool overshoot = true;

        while(t <= 1)
        {
          t += 1f / divisions;
          Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
            individual.bezierCurve.points[0],
            individual.bezierCurve.points[1],
            individual.bezierCurve.points[2],
            individual.bezierCurve.points[3],
            t);

          var distanceSinceLastPoint = (newPos - pointOncurve).magnitude;
          // We may have overshoot it, but only by small distance so we will not bother with it
          if(distanceSinceLastPoint >= velocity)
          {
            newPos = pointOncurve;
            alreadyTraveled = t;
            overshoot = false;
            break;
          }

          alreadyTraveled = t;
        }

        // Lets not count this as we overshoot
        // Go to the beginning of the cycle where we handle this situation
        if (overshoot && i > 0)
        {
          i--;
        }

        prevVelocity = velocity;

        fitness = (_destination - newPos).magnitude;
      }

      fitnesses[index] = fitness;
      index++;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


[BurstCompile]
public struct BezierFitnessTimeToDestinationParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public NativeArray<float> fitnesses;
  public float weight;
  public float maxAcc;
  public float startVelocity;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;

      var alreadyTraveled = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var prevVelocity = startVelocity;
      var pathSize = individual.accelerations.Length - 1;
      for (int i = 0; i < individual.accelerations.Length; i++)
      {
        var acc = individual.accelerations[i];
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;

        // If true, we overshoot the distance
        if (t >= 1 && i < individual.accelerations.Length)
        {
          pathSize = i -1;
          break;
        }

        bool overshoot = true;

        while (t <= 1)
        {
          t += 1f / divisions;
          Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
            individual.bezierCurve.points[0],
            individual.bezierCurve.points[1],
            individual.bezierCurve.points[2],
            individual.bezierCurve.points[3],
            t);

          var distanceSinceLastPoint = (newPos - pointOncurve).magnitude;
          // We may have overshoot it, but only by small distance so we will not bother with it
          if (distanceSinceLastPoint >= velocity)
          {
            newPos = pointOncurve;
            alreadyTraveled = t;
            overshoot = false;
            break;
          }

          alreadyTraveled = t;
        }

        // Lets not count this as we overshoot
        // Go to the beginning of the cycle where we handle this situation
        if (overshoot && i > 0)
        {
          i--;
        }

        prevVelocity = velocity;
      }

      fitnesses[index] = pathSize;
      index++;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


[BurstCompile]
public struct BezierFitnessCollisionParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  public Vector2 _startPosition;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public NativeArray<float> fitnesses;
  public float weight;
  public float maxAcc;
  public float startVelocity;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;
      var fitness = 0f;

      var stepIndex = 1;
      var alreadyTraveled = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var prevVelocity = startVelocity;
      foreach (var acc in individual.accelerations)
      {
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;
        while (t <= 1)
        {
          t += 1f / divisions;
          Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
            individual.bezierCurve.points[0],
            individual.bezierCurve.points[1],
            individual.bezierCurve.points[2],
            individual.bezierCurve.points[3],
            t);

          var distanceSinceLastPoint = (newPos - pointOncurve).magnitude;
          // We may have overshoot it, but only by small distance so we will not bother with it
          if (distanceSinceLastPoint >= velocity)
          {
            if (UtilsGA.UtilsGA.Collides(_quadTree, newPos, pointOncurve, _agentRadius, _agentIndex, stepIndex) is var col && col > 0)
            {
              PathDrawer.DrawConnectionLine(newPos, pointOncurve);
              fitness += col * UtilsGA.UtilsGA.CalculateCollisionDecayFunction(stepIndex - 1);
            }
            newPos = pointOncurve;
            alreadyTraveled = t;
            break;
          }

          alreadyTraveled = t;
        }

        prevVelocity = velocity;
        stepIndex++;
      }

      fitnesses[index] = fitness;
      index++;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}

[BurstCompile]
public struct BezierFitnessJerkCostParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  public Vector2 _startPosition;
  public NativeArray<float> fitnesses;
  public float weight;
  public float startVelocity;
  public float maxAcc;
  public float updateInteraval;
  public float maxAgentSpeed;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = _startPosition;
      var prevVelocity = startVelocity;

      NativeArray<Vector2> velocities = new NativeArray<Vector2>(individual.accelerations.Length, Allocator.Temp);
      NativeArray<Vector2> accelerations = new NativeArray<Vector2>(velocities.Length - 1, Allocator.Temp);
      NativeArray<Vector2> jerks = new NativeArray<Vector2>(accelerations.Length - 1, Allocator.Temp);

      var alreadyTraveled = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var velocityIndex = 0;
      foreach (var acc in individual.accelerations)
      {
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;
        while (t <= 1)
        {
          t += 1f / divisions;
          Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
            individual.bezierCurve.points[0],
            individual.bezierCurve.points[1],
            individual.bezierCurve.points[2],
            individual.bezierCurve.points[3],
            t);

          var distanceSinceLastPoint = (newPos - pointOncurve).magnitude;
          // We may have overshoot it, but only by small distance so we will not bother with it
          if (distanceSinceLastPoint >= velocity)
          {
            velocities[velocityIndex] = (pointOncurve - newPos);
            newPos = pointOncurve;
            alreadyTraveled = t;
            break;
          }

          alreadyTraveled = t;
        }

        velocityIndex++;
        prevVelocity = velocity;
      }

      for (int i = 1; i < velocities.Length; i++)
      {
        accelerations[i - 1] = velocities[i] - velocities[i - 1];
      }

      for (int i = 1; i < accelerations.Length; i++)
      {
        jerks[i - 1] = accelerations[i] - accelerations[i - 1];
      }

      float sumSquaredMagnitude = 0f;
      foreach (var v in jerks)
      {
        sumSquaredMagnitude += v.sqrMagnitude;
      }

      float averageSqrMagnirude = sumSquaredMagnitude / jerks.Length;
      float averageMagnitude = Mathf.Sqrt(averageSqrMagnirude);

      fitnesses[index] = averageMagnitude;
      index++;

      velocities.Dispose();
      accelerations.Dispose();
      jerks.Dispose();
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    fitnesses.Dispose();
  }
}