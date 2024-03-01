using UnityEngine;
using Unity.Collections;
using System.Text;
using System.IO;
using Unity.Burst;
using System.Linq;

[BurstCompile]
public struct StraightLineEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;
  public Vector2 _agentPosition;
  public Vector2 _agentDestination;
  public Vector2 _agentForward;
  public float _agentSpeed;


  public void LogPopulationState(ref NativeArray<BasicIndividualStruct> pop, int iteration)
  {
    var bestIndividual = pop[0];

    for (int i = 0; i < pop.Length; i++)
    {
      if (pop[i].fitness > bestIndividual.fitness)
      {
        bestIndividual = pop[i];
      }
    }

    var outdatedIndividual = _topIndividuals[iteration];
    outdatedIndividual.fitness = bestIndividual.fitness;
    for (int j = 0; j < outdatedIndividual.path.Length; j++)
    {
      outdatedIndividual.path[j] = bestIndividual.path[j];
    }
    _topIndividuals[iteration] = outdatedIndividual;
  }

  public void WriteRes(string configuration, int iteration)
  {
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness,Objective");

    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      var individual = _topIndividuals[i];
      var position = _agentPosition;
      var fit = individual.fitness.ToString();

      var objective = (_agentDestination - position).normalized;
      objective = objective * _agentSpeed * SimulationManager.Instance._agentUpdateInterval;
      objective = objective + position;

      // calculate how far are we from objective
      var rotationVector = _agentForward.normalized;
      if (rotationVector.x == 0 && rotationVector.y == 0)
        rotationVector = new Vector2(1, 0);
      var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, individual.path[0].x);
      rotatedVector = rotatedVector * individual.path[0].y;
      var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, position);

      var distance = new Vector2(objective.x - rotatedAndTranslatedVector.x, objective.y - rotatedAndTranslatedVector.y).magnitude;
      builder.AppendLine(string.Format("{0},{1}", fit, distance));
    }

    File.WriteAllText(string.Format("Plotting/straightLine/out{0}.csv", iteration), builder.ToString());
  }

  public void Dispose()
  {
    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      _topIndividuals[i].Dispose();
    }
    _topIndividuals.Dispose();
  }
}

[BurstCompile]
public struct FitnessEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;


  public void LogPopulationState(ref NativeArray<BasicIndividualStruct> pop, int iteration)
  {
    var bestIndividual = pop[0];

    for (int i = 0; i < pop.Length; i++)
    {
      if (pop[i].fitness > bestIndividual.fitness)
      {
        bestIndividual = pop[i];
      }
    }

    var outdatedIndividual = _topIndividuals[iteration];
    outdatedIndividual.fitness = bestIndividual.fitness;
    for (int j = 0; j < outdatedIndividual.path.Length; j++)
    {
      outdatedIndividual.path[j] = bestIndividual.path[j];
    }
    _topIndividuals[iteration] = outdatedIndividual;
  }

  public void WriteRes(string configuration, int iteration, string scenarioName, string agentId)
  {
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      var fit = _topIndividuals[i].fitness.ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/out-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());
  }

  public void Dispose()
  {
    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      _topIndividuals[i].Dispose();
    }
    _topIndividuals.Dispose();
  }
}


[BurstCompile]
public struct BezierIndividualLogger
{
  // Arrays to hold best fitnesses in each iteration
  public NativeArray<float> individualFitness;
  public NativeArray<float> jerkFitness;
  public NativeArray<float> collisionFitness;
  public NativeArray<float> endDistanceFitness;
  public NativeArray<float> ttdFitness;

  public void LogPopulationState(ref NativeArray<float> pop,
                                 ref NativeArray<float> jerkF,
                                 ref NativeArray<float> colF,
                                 ref NativeArray<float> endF,
                                 ref NativeArray<float> ttdF,
                                 int iteration)
  {
    var bestIndividual = UtilsGA.UtilsGA.GetMinValueFromArray(ref pop);
    var bestJerkF = UtilsGA.UtilsGA.GetMinValueFromArray(ref jerkF);
    var bestColF = UtilsGA.UtilsGA.GetMinValueFromArray(ref colF);
    var bestEndF = UtilsGA.UtilsGA.GetMinValueFromArray(ref endF);
    var bestTtdF = UtilsGA.UtilsGA.GetMinValueFromArray(ref ttdF);

    individualFitness[iteration] = bestIndividual;
    jerkFitness[iteration] = bestJerkF;
    collisionFitness[iteration] = bestColF;
    endDistanceFitness[iteration] = bestEndF;
    ttdFitness[iteration] = bestTtdF;
  }

  public void WriteRes(string configuration, int iteration, string scenarioName, string agentId)
  {
    // Log Individual fitness
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < individualFitness.Length; i++)
    {
      var fit = individualFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-Individual-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log Jerk fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < jerkFitness.Length; i++)
    {
      var fit = jerkFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-Jerk-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log Collision fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < collisionFitness.Length; i++)
    {
      var fit = collisionFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-Collision-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log EndDistance fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < endDistanceFitness.Length; i++)
    {
      var fit = endDistanceFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-EndDistance-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log TTD fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < ttdFitness.Length; i++)
    {
      var fit = ttdFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-TTD-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());
  }

  public void Dispose()
  {
    individualFitness.Dispose();
    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    ttdFitness.Dispose();
  }
}