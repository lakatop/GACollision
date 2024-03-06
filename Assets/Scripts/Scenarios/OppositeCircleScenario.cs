using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where 10 agents start in circlular shape and go into opposite directions
/// </summary>
public class OppositeCircleScenario : IScenario
{
  private const string _scenarioName = "oppositeCircleAgents";

  public int runCounter { get; set; }

  public OppositeCircleScenario(int runCount)
  {
    runCounter = runCount;
  }

  public void SetupScenario(List<IBaseAgent> agents)
  {
    for (int i = 0; i < 10; i++)
    {
      agents.Add(new BasicGAAgentParallel());
      var agent = agents[agents.Count - 1];
      agent.id = agents.Count;
      if (agent is BaseAgent)
      {
        ((BaseAgent)agent).SetName();
      }

      Vector2 spawnPosition = GetCirclePosition(i, 10, 10);
      Vector2 destination = new Vector2
      {
        x = -spawnPosition.x,
        y = -spawnPosition.y
      };

      ((BaseAgent)agent).SpawnPosition(spawnPosition);
      agent.SetDestination(destination);
      agent.SetForward((destination - spawnPosition).normalized);
      ((BaseAgent)agent).scenarioName = _scenarioName;
    }
  }

  private Vector2 GetCirclePosition(int index, int agentsCount, float radius)
  {
    var rotationAngle = 360 / agentsCount;
    var theta = index * rotationAngle;
    return new Vector2
    {
      x = radius * Mathf.Cos(theta),
      y = radius * Mathf.Sin(theta)
    };
  }
}