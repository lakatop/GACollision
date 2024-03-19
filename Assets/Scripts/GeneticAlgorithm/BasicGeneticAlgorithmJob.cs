using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using Unity.Collections;


[BurstCompile]
public struct BezierGeneticAlgorithmParallel : IJob, IGeneticAlgorithmParallel<BezierIndividualStruct>
{
  // Crossover
  public UniformBezierCrossOperatorParallel cross;

  // Mutation
  public BezierStraightFinishMutationOperatorParallel straightFinishMutation;
  public BezierClampVelocityMutationOperatorParallel clampVelocityMutation;
  public BezierShuffleAccMutationOperatorParallel shuffleMutation;
  public BezierShuffleControlPointsMutationOperatorParallel controlPointsMutation;
  public BezierSmoothAccMutationOperatorParallel smoothMutation;

  // Fitness
  public BezierFitnessJerkCostParallel jerkFitness;
  public BezierFitnessCollisionParallel collisionFitness;
  public BezierFitnessEndDistanceParallel endDistanceFitness;
  public BezierFitnessTimeToDestinationParallel ttdFitness;

  // Selection
  public BezierElitistSelectionParallel selection;

  // Initialization
  public BezierInitialization popInitialization;

  // Ranking
  public BezierWeightedSumRanking ranking;

  // Population
  public NativeBezierPopulation pop;
  public BezierIndividualLogger logger;
  public BezierPopulationDrawer popDrawer;


  public int iterations { get; set; }
  public int populationSize { get; set; }

  public NativeArray<Vector2> winner;
  public float timeDelta;
  public float agentSpeed;
  public Vector2 startPosition;
  public Vector2 forward;

  public float startVelocity;
  public float maxAcc;
  public float updateInteraval;

  public Unity.Mathematics.Random rand;

  public void Execute()
  {
    // Initialisation
    popInitialization.ModifyPopulation(ref pop.population, 0);

    for (int i = 0; i < iterations; i++)
    {
      // Fitness
      jerkFitness.ModifyPopulation(ref pop.population, i);
      collisionFitness.ModifyPopulation(ref pop.population, i);
      endDistanceFitness.ModifyPopulation(ref pop.population, i);
      ttdFitness.ModifyPopulation(ref pop.population, i);

      // Ranking
      ranking.CalculateRanking(ref jerkFitness.fitnesses,
                               ref collisionFitness.fitnesses,
                               ref endDistanceFitness.fitnesses,
                               ref ttdFitness.fitnesses,
                               jerkFitness.weight,
                               collisionFitness.weight,
                               endDistanceFitness.weight,
                               ttdFitness.weight);
      ranking.ModifyPopulation(ref pop.population, i);

      // Logging
      logger.LogPopulationState(ref ranking.resultingFitnesses,
                                ref jerkFitness.fitnesses,
                                ref collisionFitness.fitnesses,
                                ref endDistanceFitness.fitnesses,
                                ref ttdFitness.fitnesses,
                                i);

      // Selection
      selection.ModifyPopulation(ref pop.population, i);

      // Operators - cross
      cross.ModifyPopulation(ref pop.population, i);

      // Operators - mutation
      controlPointsMutation.ModifyPopulation(ref pop.population, i);
      smoothMutation.ModifyPopulation(ref pop.population, i);
      shuffleMutation.ModifyPopulation(ref pop.population, i);
      clampVelocityMutation.ModifyPopulation(ref pop.population, i);
      straightFinishMutation.ModifyPopulation(ref pop.population, i);

      // Debug draw
      //popDrawer.DrawPopulation(ref pop._population);
    }

    jerkFitness.ModifyPopulation(ref pop.population, iterations);
    collisionFitness.ModifyPopulation(ref pop.population, iterations);
    endDistanceFitness.ModifyPopulation(ref pop.population, iterations);
    ttdFitness.ModifyPopulation(ref pop.population, iterations);

    ranking.CalculateRanking(ref jerkFitness.fitnesses,
                         ref collisionFitness.fitnesses,
                         ref endDistanceFitness.fitnesses,
                         ref ttdFitness.fitnesses,
                         jerkFitness.weight,
                         collisionFitness.weight,
                         endDistanceFitness.weight,
                         ttdFitness.weight);
    ranking.ModifyPopulation(ref pop.population, iterations);

    // Logging
    //logger.LogPopulationState(ref ranking.resultingFitnesses,
    //                      ref jerkFitness.fitnesses,
    //                      ref collisionFitness.fitnesses,
    //                      ref endDistanceFitness.fitnesses,
    //                      ref ttdFitness.fitnesses,
    //                      iterations);

    // Debug Draw
    //popDrawer.DrawPopulation(ref pop._population);

    SetWinner();
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 7);

    timeDelta = (float)resources[0];
    agentSpeed = (float)resources[1];
    startPosition = (Vector2)resources[2];
    forward = (Vector2)resources[3];
    if (forward.x == 0 && forward.y == 0)
      forward = new Vector2(1, 0);
    startVelocity = (float)resources[4];
    maxAcc = (float)resources[5];
    updateInteraval = (float)resources[6];
  }

  public Vector2 GetResult()
  {
    return winner[0];
  }

  private void SetWinner()
  {
    winner[0] = new Vector2(0, 0);
    float minFitness = float.MaxValue;
    var winnerIndex = 0;
    for (int i = 0; i < pop.population.Length; i++)
    {
      if(minFitness > pop.population[i].fitness)
      {
        minFitness = pop.population[i].fitness;
        winnerIndex = i;
      }
    }

    var individual = pop.population[winnerIndex];
    float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
      Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
      Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
    float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
    int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);
    var currentAcc = maxAcc * individual.accelerations[0];
    var velocity = startVelocity + currentAcc;
    velocity = Mathf.Clamp(velocity, 0, updateInteraval * agentSpeed);
    bool overshoot = true;
    float t = 0;
    while (t <= 1)
    {
      t += 1f / divisions;
      Vector2 pointOncurve = individual.bezierCurve.EvaluateCubic(
        individual.bezierCurve.points[0],
        individual.bezierCurve.points[1],
        individual.bezierCurve.points[2],
        individual.bezierCurve.points[3],
        t);

      var distanceSinceLastPoint = (startPosition - pointOncurve).magnitude;
      // We may have overshoot it, but only by small distance so we will not bother with it
      if (distanceSinceLastPoint >= velocity)
      {
        winner[0] = pointOncurve - startPosition;
        // We need to scale velocity back
        winner[0] = winner[0] * (1 / updateInteraval);
        overshoot = false;
        break;
      }
    }

    if (overshoot)
    {
      var destination = individual.bezierCurve.points[3];
      var headingVector = (destination - startPosition).normalized * velocity;
      winner[0] = headingVector;
      winner[0] = winner[0] * (1 / updateInteraval);
    }
  }

  public string GetConfiguration()
  {
    var builder = new System.Text.StringBuilder();

    // cross
    builder.AppendLine(string.Format("CROSS,{0}", cross.GetComponentName()));

    // mutation
    builder.AppendLine(string.Format("MUTATION {0}, {1}", controlPointsMutation.GetComponentName(), controlPointsMutation.GetMutationProbabilty().ToString()));
    builder.AppendLine(string.Format("MUTATION {0}, {1}", smoothMutation.GetComponentName(), smoothMutation.GetMutationProbabilty().ToString()));
    builder.AppendLine(string.Format("MUTATION {0}, {1}", shuffleMutation.GetComponentName(), shuffleMutation.GetMutationProbabilty().ToString()));
    builder.AppendLine(string.Format("MUTATION {0}, {1}", clampVelocityMutation.GetComponentName(), clampVelocityMutation.GetMutationProbabilty().ToString()));
    builder.AppendLine(string.Format("MUTATION {0}, {1}", straightFinishMutation.GetComponentName(), straightFinishMutation.GetMutationProbabilty().ToString()));

    // fitness
    builder.AppendLine(string.Format("FITNESSES {0}, {1}", jerkFitness.GetComponentName(), jerkFitness.GetFitnessWeight().ToString()));
    builder.AppendLine(string.Format("FITNESSES {0}, {1}", collisionFitness.GetComponentName(), collisionFitness.GetFitnessWeight().ToString()));
    builder.AppendLine(string.Format("FITNESSES {0}, {1}", endDistanceFitness.GetComponentName(), endDistanceFitness.GetFitnessWeight().ToString()));
    builder.AppendLine(string.Format("FITNESSES {0}, {1}", ttdFitness.GetComponentName(), ttdFitness.GetFitnessWeight().ToString()));

    // selection
    builder.AppendLine(string.Format("SELECTION {0}", selection.GetComponentName()));

    // initialisation
    builder.AppendLine(string.Format("INITIALIZATION {0}", popInitialization.GetComponentName()));

    // general population info
    builder.AppendLine(string.Format("POPULATION SIZE {0}, ITERATIONS {1}", populationSize, iterations));

    return builder.ToString();
  }

  public string GetHyperparametersId()
  {
    return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}",
      controlPointsMutation.GetMutationProbabilty().ToString(),
      smoothMutation.GetMutationProbabilty().ToString(),
      shuffleMutation.GetMutationProbabilty().ToString(),
      clampVelocityMutation.GetMutationProbabilty().ToString(),
      straightFinishMutation.GetMutationProbabilty().ToString(),
      cross.GetCrossProbability().ToString(),
      collisionFitness.GetFitnessWeight().ToString(),
      endDistanceFitness.GetFitnessWeight().ToString(),
      jerkFitness.GetFitnessWeight().ToString(),
      ttdFitness.GetFitnessWeight().ToString(),
      populationSize.ToString(),
      iterations.ToString());
  }

  public void Dispose()
  {
    straightFinishMutation.Dispose();
    shuffleMutation.Dispose();
    controlPointsMutation.Dispose();
    smoothMutation.Dispose();
    clampVelocityMutation.Dispose();

    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    ttdFitness.Dispose();

    cross.Dispose();

    selection.Dispose();
    logger.Dispose();
    ranking.Dispose();
    winner.Dispose();
    pop.Dispose();
  }
}
