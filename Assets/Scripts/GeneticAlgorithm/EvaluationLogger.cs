using UnityEngine;
using Unity.Collections;
using System.Text;
using System.IO;
using Unity.Burst;

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