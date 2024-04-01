using UnityEngine;
using NativeQuadTree;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// EndDistance fitness for BezierIndividualStruct designed ot be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierFitnessEndDistanceParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Current position of an agent
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// Destination of an agent
  /// </summary>
  [ReadOnly] public Vector2 destination;
  /// <summary>
  /// Weight of this fitness used inside weighted sum
  /// </summary>
  [ReadOnly] public float weight;
  /// <summary>
  /// Maximum agents acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// agents current velocity
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// How often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInteraval;
  /// <summary>
  /// Maximum agents speed
  /// </summary>
  [ReadOnly] public float maxAgentSpeed;
  /// <summary>
  /// Array for holding population fitnesses
  /// </summary>
  public NativeArray<float> fitnesses;

  /// <summary>
  /// Calcualte EndDistance fitness in population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = startPosition;

      var alreadyTraveled = 0f;

      var fitness = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 20);

      bool overshoot = false;
      bool inDestination = false;
      var prevVelocity = startVelocity;

      for (int i = 0; i < individual.accelerations.Length; i++)
      {
        if (inDestination)
          break;

        var acc = individual.accelerations[i];
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;

        // If true, we overshoot the distance
        if (overshoot)
        {
          // Calculate Distance To Destination
          var distanceToDestination = (destination - newPos).magnitude;

          // Calculate how far it would be untill we slow down to 0
          var traveled = 0f;
          var initialVelocity = velocity;
          while (velocity > 0)
          {
            traveled += velocity;
            velocity -= maxAcc;
          }
          if (traveled < distanceToDestination)
          {
            Debug.Log(string.Format("TRAVELED < DTD: {0}, {1}, {2}", traveled, distanceToDestination, initialVelocity));
          }
          // Fitness = distance from endpos to destination (*2 = for path to the endpos and back to destination)
          fitness = (traveled - distanceToDestination) * 2;
          // Penalize individuals that overshoot
          fitness *= 2f;

          break;
        }

        if (velocity > 0)
        {
          overshoot = true;

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
            if (velocity <= distanceSinceLastPoint)
            {
              newPos = pointOncurve;
              alreadyTraveled = t;
              overshoot = false;
              break;
            }

            alreadyTraveled = t;
          }
        }


        if (overshoot)
        {
          var maxVel = UtilsGA.UtilsGA.CalculateMaxVelocity((destination - newPos).magnitude + 0.15f); // 0.1f for imprecision in bezier length calculation
          // Check if we would be able to stop at the destination
          // If yes, count this as we would arrive properly and dont overshoot
          if (Mathf.Abs(maxVel - prevVelocity) <= maxAcc)
          {
            inDestination = true;
            newPos = destination;
            fitness = 0;
            continue;
          }
          i--;
        }
        else
        {
          prevVelocity = velocity;
        }


        fitness = (destination - newPos).magnitude;
      }

      fitnesses[index] = fitness;
      index++;
    }
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
  /// Getter for fitness weight
  /// </summary>
  /// <returns>Fitness weight</returns>
  public float GetFitnessWeight()
  {
    return weight;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


/// <summary>
/// TimeToDestination fitness for BezierIndividualStruct designed ot be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierFitnessTimeToDestinationParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Current position of an agent
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// Destination of an agent
  /// </summary>
  [ReadOnly] public Vector2 destination;
  /// <summary>
  /// Weight of this fitness used inside weighted sum
  /// </summary>
  [ReadOnly] public float weight;
  /// <summary>
  /// Maximum agents acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// agents current velocity
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// How often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInteraval;
  /// <summary>
  /// Maximum agents speed
  /// </summary>
  [ReadOnly] public float maxAgentSpeed;
  /// <summary>
  /// Array for holding population fitnesses
  /// </summary>
  public NativeArray<float> fitnesses;

  /// <summary>
  /// Calcualte TimeToDestination fitness in population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = startPosition;

      var alreadyTraveled = 0f;

      float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
        Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
        Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
      float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
      int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

      var prevVelocity = startVelocity;
      var pathSize = individual.accelerations.Length - 1;
      bool overshoot = false;
      bool inDestination = false;

      for (int i = 0; i < individual.accelerations.Length; i++)
      {
        if (inDestination)
          break;

        var acc = individual.accelerations[i];
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);


        // Calculate position on a bezier curve
        float t = alreadyTraveled;

        // If true, we overshoot the distance
        if (overshoot)
        {
          // Calculate how many steps it would take untill we slow down to 0
          var traveled = 0;
          var initialVelocity = velocity;
          while (velocity > 0)
          {
            traveled++;
            velocity -= maxAcc;
          }
          // Fitness = distance from endpos to destination (*2 = for path to the endpos and back to destination)
          pathSize = i; // the distance it took so far
          pathSize += 1; // +1 for crossing the destination
          pathSize += (traveled - 1) * 2; // slowing down steps + return to destination (for returning simply multiply by steps it took to slow donw)

          break;
        }

        overshoot = true;

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
          if (velocity <= distanceSinceLastPoint)
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
        if (overshoot)
        {
          var maxVel = UtilsGA.UtilsGA.CalculateMaxVelocity((destination - newPos).magnitude + 0.15f); // 0.1f for imprecision in bezier length calculation
          // Check if we would be able to stop at the destination
          // If yes, count this as we would arrive properly and dont overshoot
          if (Mathf.Abs(maxVel - prevVelocity) <= maxAcc)
          {
            inDestination = true;
            newPos = destination;
            pathSize = i;
            continue;
          }
          i--;
        }
        else
        {
          prevVelocity = velocity;
        }

      }

      fitnesses[index] = pathSize;
      index++;
    }
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
  /// Getter for fitness weight
  /// </summary>
  /// <returns>Fitness weight</returns>
  public float GetFitnessWeight()
  {
    return weight;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


/// <summary>
/// Collision fitness for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierFitnessCollisionParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Agents current position
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// Radius of agent
  /// </summary>
  [ReadOnly] public float agentRadius;
  /// <summary>
  /// Agents id
  /// </summary>
  [ReadOnly] public int agentIndex;
  /// <summary>
  /// Quadtree for collision detection
  /// </summary>
  [ReadOnly] public NativeQuadTree<TreeNode> quadTree;
  /// <summary>
  /// Weight of this fitness used inside weighted sum
  /// </summary>
  [ReadOnly] public float weight;
  /// <summary>
  /// Maximum agents acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// Agents starting velocity
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// How often is agent running GA
  /// </summary>  /// <summary>
  /// How often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInteraval;
  /// <summary>
  /// MAximum agents velocity
  /// </summary>
  [ReadOnly] public float maxAgentSpeed;
  /// <summary>
  /// Array for holding population fitnesses
  /// </summary>
  public NativeArray<float> fitnesses;

  /// <summary>
  /// Calcualte Collision fitness in population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = startPosition;
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
          if (velocity <= distanceSinceLastPoint)
          {
            if (UtilsGA.UtilsGA.Collides(quadTree, newPos, pointOncurve, agentRadius, agentIndex, stepIndex) is var col && col > 0)
            {
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

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Getter for fitness weight
  /// </summary>
  /// <returns>Fitness weight</returns>
  public float GetFitnessWeight()
  {
    return weight;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    fitnesses.Dispose();
  }
}


/// <summary>
/// JerkCost fitness for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierFitnessJerkCostParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Current position of an agent
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// Destination of an agent
  /// </summary>
  [ReadOnly] public Vector2 destination;
  /// <summary>
  /// Weight of this fitness used inside weighted sum
  /// </summary>
  [ReadOnly] public float weight;
  /// <summary>
  /// Maximum agents acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// agents current velocity
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// How often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInteraval;
  /// <summary>
  /// Maximum agents speed
  /// </summary>
  [ReadOnly] public float maxAgentSpeed;
  /// <summary>
  /// Array holding population fitnesses
  /// </summary>
  public NativeArray<float> fitnesses;

  /// <summary>
  /// Calcualte JerkCost fitness in population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var newPos = startPosition;
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
      bool overshoot = false;
      bool inDestination = false;

      for (int i = 0; i < individual.accelerations.Length; i++)
      {
        if (inDestination)
          break;

        var acc = individual.accelerations[i];
        var currentAcc = maxAcc * acc;
        var velocity = prevVelocity + currentAcc;
        velocity = Mathf.Clamp(velocity, 0, updateInteraval * maxAgentSpeed);

        // If true, we overshoot
        if (overshoot)
        {
          // Calculate remaining velocities as if we try to go directly to the destination
          var remainingVelocity = velocity;
          var headingVelocity = (destination - newPos).normalized;
          while (remainingVelocity > 0 && velocityIndex < velocities.Length)
          {
            velocities[velocityIndex] = headingVelocity * remainingVelocity;
            newPos = newPos + velocities[velocityIndex];
            remainingVelocity -= maxAcc;
            remainingVelocity = Mathf.Clamp(remainingVelocity, 0, updateInteraval * maxAgentSpeed);
            velocityIndex++;
          }

          // Take path back - evenly distributed
          var remainingDistance = (newPos - destination).magnitude;
          // Switch to opposite direction
          headingVelocity = new Vector2(-headingVelocity.x, -headingVelocity.y);
          while (remainingDistance > Mathf.Epsilon && velocityIndex < velocities.Length)
          {
            velocity = (remainingDistance < maxAcc) ? remainingDistance : maxAcc;
            velocities[velocityIndex] = headingVelocity * velocity;
            velocityIndex++;
            remainingDistance -= velocity;
          }

          break;
        }


        overshoot = true;

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
            overshoot = false;
            break;
          }

          alreadyTraveled = t;
        }

        if (overshoot)
        {
          var maxVel = UtilsGA.UtilsGA.CalculateMaxVelocity((destination - newPos).magnitude + 0.15f); // 0.1f for imprecision in bezier length calculation
          // Check if we would be able to stop at the destination
          // If yes, count this as we would arrive properly and dont overshoot
          if (Mathf.Abs(maxVel - prevVelocity) <= maxAcc)
          {
            inDestination = true;
            velocities[velocityIndex] = (destination - newPos);
            for (int j = velocityIndex + 1; j < individual.accelerations.Length; j++)
            {
              velocities[j] = Vector2.zero;
            }
            newPos = destination;

            continue;
          }

          i--;
        }
        else
        {
          velocityIndex++;
          prevVelocity = velocity;
        }
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

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Getter for fitness weight
  /// </summary>
  /// <returns>Fitness weight</returns>
  public float GetFitnessWeight()
  {
    return weight;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    fitnesses.Dispose();
  }
}
