using UnityEngine;
using Unity.Collections;
using System.Text;
using System.IO;

public struct StraightLineEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;
  public Vector2 _agentPosition;
  public Vector2 _agentDestination;
  public Vector2 _agentForward;
  public int iteration;
  public float _agentSpeed;


  public void LogPopulationState(NativeArray<BasicIndividualStruct> pop)
  {
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
      var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, position);
      rotatedAndTranslatedVector = rotatedAndTranslatedVector * individual.path[0].y;

      var distance = new Vector2(objective.x - rotatedAndTranslatedVector.x, objective.y - rotatedAndTranslatedVector.y).magnitude;
      builder.AppendLine(string.Format("{0},{1}", fit, distance));
    }

    File.WriteAllText(string.Format("Plotting/straightLine/out{0}.csv", iteration), builder.ToString());
  }

  public void Dispose()
  {
    _topIndividuals.Dispose();
  }
}