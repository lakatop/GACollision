using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using Unity.Collections;



[BurstCompile]
public struct BasicGeneticAlgorithmParallel : IJob, IGeneticAlgorithmParallel<BasicIndividualStruct>
{
  public MeanCrossOperatorParallel cross;
  public BasicMutationOperatorParallel mutation;
  public FitnessJerkCostParallel jerkFitness;
  public FitnessCollisionParallel collisionFitness;
  public FitnessEndDistanceParallel endDistanceFitness;
  public NegativeSelectionParallel selection;
  public BezierInitialization popInitialization;
  public WeightedSumRanking ranking;
  public NativeBezierPopulation pop;
  public FitnessEvaluationLogger logger;
  public BezierPopulationDrawer popDrawer;


  public int iterations { get; set; }
  public int populationSize { get; set; }

  public NativeArray<Vector2> _winner;
  public float _timeDelta;
  public float _agentSpeed;
  public Vector2 _startPosition;
  public Vector2 _forward;

  public float startVelocity;
  public float maxAcc;
  public float updateInteraval;

  public Unity.Mathematics.Random _rand;

  public void Execute()
  {
    popInitialization.ModifyPopulation(ref pop._population, 0);

    for (int i = 0; i < iterations; i++)
    {
      //jerkFitness.ModifyPopulation(ref pop._population, i);
      //collisionFitness.ModifyPopulation(ref pop._population, i);
      //endDistanceFitness.ModifyPopulation(ref pop._population, i);

      //ranking.CalculateRanking(ref jerkFitness.fitnesses, ref collisionFitness.fitnesses, ref endDistanceFitness.fitnesses,
      //  jerkFitness.weight, collisionFitness.weight, endDistanceFitness.weight);
      //ranking.ModifyPopulation(ref pop._population, i);

      //logger.LogPopulationState(ref pop._population, i);
      //selection.ModifyPopulation(ref pop._population, i);
      //cross.ModifyPopulation(ref pop._population, i);
      //mutation.ModifyPopulation(ref pop._population, i);
    }

    //jerkFitness.ModifyPopulation(ref pop._population, iterations);
    //collisionFitness.ModifyPopulation(ref pop._population, iterations);
    //endDistanceFitness.ModifyPopulation(ref pop._population, iterations);

    //ranking.CalculateRanking(ref jerkFitness.fitnesses, ref collisionFitness.fitnesses, ref endDistanceFitness.fitnesses,
    //    jerkFitness.weight, collisionFitness.weight, endDistanceFitness.weight);
    //ranking.ModifyPopulation(ref pop._population, iterations);

    //logger.LogPopulationState(ref pop._population, iterations);
    popDrawer.DrawPopulation(ref pop._population);
    SetWinner();
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 7);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
    _forward = (Vector2)resources[3];
    if (_forward.x == 0 && _forward.y == 0)
      _forward = new Vector2(1, 0);
    startVelocity = (float)resources[4];
    maxAcc = (float)resources[5];
    updateInteraval = (float)resources[6];
  }

  public Vector2 GetResult()
  {
    return _winner[0];
  }

  private void SetWinner()
  {
    _winner[0] = new Vector2(0, 0);
    //float minFitness = float.MaxValue;
    //foreach (var individual in pop._population)
    //{
    //  if (minFitness > individual.fitness)
    //  {
    //    var v = UtilsGA.UtilsGA.RotateVector(_forward.normalized, individual.path[0].x);
    //    var acc = maxAcc * individual.path[0].y;
    //    var velocity = startVelocity + acc;
    //    velocity = Mathf.Clamp(velocity, 0, updateInteraval * _agentSpeed);
    //    v *= velocity;
    //    _winner[0] = new Vector2(v.x, v.y);
    //    minFitness = individual.fitness;
    //  }
    //}
  }

  public string GetConfiguration()
  {
    var builder = new System.Text.StringBuilder();
    builder.AppendLine(string.Format("CROSS,{0}", cross.GetComponentName()));
    builder.AppendLine(string.Format("MUTATION,{0}", mutation.GetComponentName()));
    builder.AppendLine(string.Format("FITNESSES,{0}, {1}, {2}", jerkFitness.GetComponentName(), endDistanceFitness.GetComponentName(), collisionFitness.GetComponentName()));
    builder.AppendLine(string.Format("SELECTION,{0}", selection.GetComponentName()));
    builder.AppendLine(string.Format("INITIALIZATION,{0}", popInitialization.GetComponentName()));

    return builder.ToString();
  }

  public void Dispose()
  {
    cross.Dispose();
    mutation.Dispose();
    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    selection.Dispose();
    logger.Dispose();
    ranking.Dispose();
    _winner.Dispose();
    pop.Dispose();
  }
}

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
  public BezierNegativeSelectionParallel selection;

  // Initialization
  public BezierInitialization popInitialization;

  // Ranking
  public BezierWeightedSumRanking ranking;

  // Population
  public NativeBezierPopulation pop;
  //public FitnessEvaluationLogger logger;
  public BezierPopulationDrawer popDrawer;


  public int iterations { get; set; }
  public int populationSize { get; set; }

  public NativeArray<Vector2> _winner;
  public float _timeDelta;
  public float _agentSpeed;
  public Vector2 _startPosition;
  public Vector2 _forward;

  public float startVelocity;
  public float maxAcc;
  public float updateInteraval;

  public Unity.Mathematics.Random _rand;

  public void Execute()
  {
    // Initialisation
    popInitialization.ModifyPopulation(ref pop._population, 0);

    for (int i = 0; i < iterations; i++)
    {
      // Fitness
      jerkFitness.ModifyPopulation(ref pop._population, i);
      collisionFitness.ModifyPopulation(ref pop._population, i);
      endDistanceFitness.ModifyPopulation(ref pop._population, i);
      ttdFitness.ModifyPopulation(ref pop._population, i);

      // Ranking
      ranking.CalculateRanking(ref jerkFitness.fitnesses,
                               ref collisionFitness.fitnesses,
                               ref endDistanceFitness.fitnesses,
                               ref ttdFitness.fitnesses,
                               jerkFitness.weight,
                               collisionFitness.weight,
                               endDistanceFitness.weight,
                               ttdFitness.weight);
      ranking.ModifyPopulation(ref pop._population, i);

      // Logging
      //logger.LogPopulationState(ref pop._population, i);

      // Selection
      selection.ModifyPopulation(ref pop._population, i);

      // Operators - cross
      cross.ModifyPopulation(ref pop._population, i);

      // Operators - mutation
      controlPointsMutation.ModifyPopulation(ref pop._population, i);
      smoothMutation.ModifyPopulation(ref pop._population, i);
      shuffleMutation.ModifyPopulation(ref pop._population, i);
      clampVelocityMutation.ModifyPopulation(ref pop._population, i);
      //straightFinishMutation.ModifyPopulation(ref pop._population, i);

      // Debug draw
      //popDrawer.DrawPopulation(ref pop._population);
    }

    jerkFitness.ModifyPopulation(ref pop._population, iterations);
    collisionFitness.ModifyPopulation(ref pop._population, iterations);
    endDistanceFitness.ModifyPopulation(ref pop._population, iterations);
    ttdFitness.ModifyPopulation(ref pop._population, iterations);

    ranking.CalculateRanking(ref jerkFitness.fitnesses,
                         ref collisionFitness.fitnesses,
                         ref endDistanceFitness.fitnesses,
                         ref ttdFitness.fitnesses,
                         jerkFitness.weight,
                         collisionFitness.weight,
                         endDistanceFitness.weight,
                         ttdFitness.weight);
    ranking.ModifyPopulation(ref pop._population, iterations);

    //logger.LogPopulationState(ref pop._population, iterations);
    popDrawer.DrawPopulation(ref pop._population);
    SetWinner();
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 7);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
    _forward = (Vector2)resources[3];
    if (_forward.x == 0 && _forward.y == 0)
      _forward = new Vector2(1, 0);
    startVelocity = (float)resources[4];
    maxAcc = (float)resources[5];
    updateInteraval = (float)resources[6];
  }

  public Vector2 GetResult()
  {
    return _winner[0];
  }

  private void SetWinner()
  {
    _winner[0] = new Vector2(0, 0);
    float minFitness = float.MaxValue;
    var winnerIndex = 0;
    for (int i = 0; i < pop._population.Length; i++)
    {
      if(minFitness > pop._population[i].fitness)
      {
        minFitness = pop._population[i].fitness;
        winnerIndex = i;
      }
    }

    var individual = pop._population[winnerIndex];
    float controlNetLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[1]) +
      Vector2.Distance(individual.bezierCurve.points[1], individual.bezierCurve.points[2]) +
      Vector2.Distance(individual.bezierCurve.points[2], individual.bezierCurve.points[3]);
    float estimatedCurveLength = Vector2.Distance(individual.bezierCurve.points[0], individual.bezierCurve.points[3]) + controlNetLength / 2f;
    int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);
    var currentAcc = maxAcc * individual.accelerations[0];
    var velocity = startVelocity + currentAcc;
    velocity = Mathf.Clamp(velocity, 0, updateInteraval * _agentSpeed);
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

      var distanceSinceLastPoint = (_startPosition - pointOncurve).magnitude;
      // We may have overshoot it, but only by small distance so we will not bother with it
      if (distanceSinceLastPoint >= velocity)
      {
        _winner[0] = pointOncurve - _startPosition;
        // We need to scale velocity back
        _winner[0] = _winner[0] * (1 / updateInteraval);
        overshoot = false;
        break;
      }
    }

    if (overshoot)
    {
      var destination = individual.bezierCurve.points[3];
      var headingVector = (destination - _startPosition).normalized * velocity;
      _winner[0] = headingVector;
      _winner[0] = _winner[0] * (1 / updateInteraval);
    }
  }

  public string GetConfiguration()
  {
    var builder = new System.Text.StringBuilder();
    //builder.AppendLine(string.Format("CROSS,{0}", cross.GetComponentName()));
    //builder.AppendLine(string.Format("MUTATION,{0}", mutation.GetComponentName()));
    builder.AppendLine(string.Format("FITNESSES,{0}, {1}, {2}", jerkFitness.GetComponentName(), endDistanceFitness.GetComponentName(), collisionFitness.GetComponentName()));
    builder.AppendLine(string.Format("SELECTION,{0}", selection.GetComponentName()));
    builder.AppendLine(string.Format("INITIALIZATION,{0}", popInitialization.GetComponentName()));

    return builder.ToString();
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
    //logger.Dispose();
    ranking.Dispose();
    _winner.Dispose();
    pop.Dispose();
  }
}



