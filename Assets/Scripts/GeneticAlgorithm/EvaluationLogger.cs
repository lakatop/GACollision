using UnityEngine;
using Unity.Collections;
using System.Text;

public struct StraightLineEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;
  public NativeArray<Vector2> _agentPositions;
  public int iteration;
  public float _agentSpeed;

  public void LogPopulationState(NativeArray<BasicIndividualStruct> pop, Vector2 agentPosition)
  {
    _agentPositions[iteration] = agentPosition;

    var bestIndividual = pop[0];

    foreach (var individual in pop)
    {
      if(individual.fitness > bestIndividual.fitness)
      {
        bestIndividual = individual;
      }
    }

    _topIndividuals[iteration] = bestIndividual;
    iteration++;
  }

  public void WriteRes(string configuration)
  {
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness,Objective");

    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      var individual = _topIndividuals[i];
      var position = _agentPositions[i];
      var fit = individual.fitness.ToString();

      var objective = new Vector2(1, 0);
      objective = objective * _agentSpeed * SimulationManager.Instance._agentUpdateInterval;
      objective = objective + position;

      // calculate how far are we from objective
    }
  }

  public void Dispose()
  {
    _topIndividuals.Dispose();
  }
}