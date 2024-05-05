using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where 10 agents go into opposite directions - 5 vs 5
/// </summary>
public class OppositeMultipleScenario : IScenario
{
  /// <summary>
  /// Name of scenario
  /// </summary>
  private const string _scenarioName = "oppositeMultipleAgents";

  /// <summary>
  /// Counter how many times scenario should be run
  /// </summary>
  public int runCounter { get; set; }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="runCount">sets runCounter</param>
  public OppositeMultipleScenario(int runCount)
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

      Vector2 spawnPosition = Vector2.zero;
      Vector2 destination = Vector2.zero;
      if (i < 5)
      {
        spawnPosition = new Vector2(i, -20);
        destination = new Vector2(i, 30);
      }
      else
      {
        spawnPosition = new Vector2(i - 5, 20);
        destination = new Vector2(i - 5, -30);
      }

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
