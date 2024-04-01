using Unity.Collections;
using UnityEngine;
using Unity.Burst;

/// <summary>
/// StraightFinish mutation for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierStraightFinishMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;
  /// <summary>
  /// Current position of an agent
  /// </summary>
  [ReadOnly] public Vector2 startPos;
  /// <summary>
  /// Destination of an agent
  /// </summary>
  [ReadOnly] public Vector2 destination;
  /// <summary>
  /// Agents forward vector
  /// </summary>
  [ReadOnly] public Vector2 forward;
  /// <summary>
  /// Maximum agents speed
  /// </summary>
  [ReadOnly] public float agentSpeed;
  /// <summary>
  /// how often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInterval;
  /// <summary>
  /// Current velocity of an agent
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// Maximum agents acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// Probablity of mutation
  /// </summary>
  [ReadOnly] public float mutationProb;

  /// <summary>
  /// Perform mutation on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    var mutProb = rand.NextFloat();
    if (mutProb > mutationProb)
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

      var newVelocity = Mathf.Clamp(maxAcceptableVelocity, 0, agentSpeed * updateInterval);
      var newAcc = newVelocity - prevVelocity;
      newAcc = Mathf.Clamp(newAcc, -1, maxAcc);
      newVelocity = prevVelocity + newAcc;
      individual.accelerations[i] = newAcc;
      prevVelocity = newVelocity;
      currentPosition = currentPosition + ((destination - currentPosition).normalized * newVelocity);
    }


    currentPopulation[currentPopulation.Length - 1] = individual;
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
  /// Getter for mutation probability
  /// </summary>
  /// <returns>Mutation probability</returns>
  public float GetMutationProbabilty()
  {
    return mutationProb;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}


/// <summary>
/// ClampVelocity mutation for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierClampVelocityMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;
  /// <summary>
  /// Maximum agents speed
  /// </summary>
  [ReadOnly] public float agentSpeed;
  /// <summary>
  /// How often is agent running GA
  /// </summary>
  [ReadOnly] public float updateInterval;
  /// <summary>
  /// agents current velocity
  /// </summary>
  [ReadOnly] public float startVelocity;
  /// <summary>
  /// agents maximum acceleration/deceleration
  /// </summary>
  [ReadOnly] public float maxAcc;
  /// <summary>
  /// Probability of mutation
  /// </summary>
  [ReadOnly] public float mutationProb;

  /// <summary>
  /// Perform mutation on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = rand.NextFloat();
      if (mutProb > mutationProb)
        return;

      var individual = currentPopulation[i];

      var currentAcc = maxAcc * individual.accelerations[0];
      var velocity = startVelocity + currentAcc;
      velocity = Mathf.Clamp(velocity, 0, updateInterval * agentSpeed);


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

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Getter for mutation probability
  /// </summary>
  /// <returns>Mutation probability</returns>
  public float GetMutationProbabilty()
  {
    return mutationProb;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}


/// <summary>
/// SmoothAcc mutation for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierSmoothAccMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;
  /// <summary>
  /// Probability of mutation
  /// </summary>
  [ReadOnly] public float mutationProb;

  /// <summary>
  /// Perform mutation on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = rand.NextFloat();
      if (mutProb > mutationProb)
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

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Getter for mutation probability
  /// </summary>
  /// <returns>Mutation probability</returns>
  public float GetMutationProbabilty()
  {
    return mutationProb;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}

/// <summary>
/// ShuffleAcc mutation for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierShuffleAccMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;
  /// <summary>
  /// Probability of mutation
  /// </summary>
  [ReadOnly] public float mutationProb;

  /// <summary>
  /// Perform mutation on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = rand.NextFloat();
      // Low mutation rate because we are counting on other mutation to smooth accelerations
      if (mutProb > mutationProb)
        return;

      var individual = currentPopulation[i];

      for (int j = 0; j < individual.accelerations.Length; j++)
      {
        // Also dont change every acceleration, just some
        mutProb = rand.NextFloat();
        if (mutProb > mutationProb)
          continue;

        var acc = (rand.NextFloat() * 2f) - 1f;
        individual.accelerations[j] = acc;
      }

      currentPopulation[i] = individual;
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
  /// Getter for mutation probability
  /// </summary>
  /// <returns>Mutation probability</returns>
  public float GetMutationProbabilty()
  {
    return mutationProb;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}

/// <summary>
/// ShuffleControlPoints mutation for BezierIndividualStruct designed to be used inside Unity jobs
/// </summary>
[BurstCompile]
public struct BezierShuffleControlPointsMutationOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  /// <summary>
  /// Random object variable
  /// </summary>
  [ReadOnly] public Unity.Mathematics.Random rand;
  /// <summary>
  /// agents current position
  /// </summary>
  [ReadOnly] public Vector2 startPosition;
  /// <summary>
  /// agents destination
  /// </summary>
  [ReadOnly] public Vector2 endPosition;
  /// <summary>
  /// agents forward vector
  /// </summary>
  [ReadOnly] public Vector2 forward;
  /// <summary>
  /// Probability of mutation
  /// </summary>
  [ReadOnly] public float mutationProb;

  /// <summary>
  /// Perform mutation on population
  /// </summary>
  /// <param name="currentPopulation">Population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var mutProb = rand.NextFloat();
      if (mutProb > mutationProb)
        return;

      // Define restrictions on control points position
      var individual = currentPopulation[i];
      float maxDeg = 30;
      float halfDistance = (endPosition - startPosition).magnitude / 2;
      float upDistance = rand.NextFloat(halfDistance);
      float controlPointLenght = Mathf.Tan(maxDeg * Mathf.Deg2Rad) * upDistance;
      float sideDistance = rand.NextFloat(-controlPointLenght, controlPointLenght);

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

  /// <summary>
  /// Getter for component name
  /// </summary>
  /// <returns>Name of this struct</returns>
  public string GetComponentName()
  {
    return GetType().Name;
  }

  /// <summary>
  /// Getter for mutation probability
  /// </summary>
  /// <returns>Mutation probability</returns>
  public float GetMutationProbabilty()
  {
    return mutationProb;
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
  }
}
