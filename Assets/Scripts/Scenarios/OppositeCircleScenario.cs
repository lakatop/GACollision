using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where 10 agents start in circlular shape and go into opposite directions
/// </summary>
public class OppositeCircleScenario : IScenario
{
  /// <summary>
  /// Name of scenario
  /// </summary>
  private const string _scenarioName = "oppositeCircleAgents";

  /// <summary>
  /// Counter how many times scenario should be run
  /// </summary>
  public int runCounter { get; set; }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="runCount">sets runCounter</param>
  public OppositeCircleScenario(int runCount)
  {
    runCounter = runCount;
  }

  /// <inheritdoc cref="IScenario.SetupScenario(List{IBaseAgent})"/>
  public void SetupScenario<T>(List<IBaseAgent> agents) where T : IBaseAgent, new()
  {
    for (int i = 0; i < 10; i++)
    {
      agents.Add(new T());
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

    AdditionalAgentSetup(agents);
  }

  /// <inheritdoc cref="IScenario.ClearScenario(List{IBaseAgent})"/>
  public void ClearScenario(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {
      agent.logger.SetEndTime(Time.realtimeSinceStartupAsDouble);
      agent.logger.CreateCsv();
      agent.logger.AppendCsvLog();
    }
  }

  /// <summary>
  /// Calculate agents position on circle
  /// </summary>
  /// <param name="index">Index of agent</param>
  /// <param name="agentsCount">Number of agents</param>
  /// <param name="radius">Radius of circle</param>
  /// <returns>Position of agent on circle</returns>
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

  /// <summary>
  /// Add additional setup for agents
  /// </summary>
  /// <param name="agents">List of agents present in scenario</param>
  private void AdditionalAgentSetup(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {
      agent.logger.SetAgentId(agent.id.ToString());
      agent.logger.SetScenarioId(_scenarioName);
      agent.logger.SetStartTime(Time.realtimeSinceStartupAsDouble);
    }
  }
}
