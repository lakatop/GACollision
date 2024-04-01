using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple scenario where agent start at position (0,0) and should go straight line to the destination (0,40)
/// </summary>
public class StraightLineScenario : IScenario
{
  /// <summary>
  /// Name of scenario
  /// </summary>
  private const string _scenarioName = "straightLine";

  /// <summary>
  /// Counter how many times scenario should be run
  /// </summary>
  public int runCounter { get; set; }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="runCount">sets runCounter</param>
  public StraightLineScenario(int runCount)
  {
    runCounter = runCount;
  }

  /// <inheritdoc cref="IScenario.SetupScenario(List{IBaseAgent})"/>
  public void SetupScenario(List<IBaseAgent> agents)
  {
    agents.Add(new BasicGAAgentParallel());
    var agent = agents[agents.Count - 1];
    agent.id = agents.Count;
    if (agent is BaseAgent)
    {
      ((BaseAgent)agent).SetName();
    }

    Vector2 spawnPosition = new Vector2(0, 0);
    Vector2 destination = new Vector2(0, 40);

    ((BaseAgent)agent).SpawnPosition(spawnPosition);
    agent.SetDestination(destination);
    agent.SetForward((destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;

    AdditionalAgentSetup(agents);
  }

  /// <inheritdoc cref="IScenario.ClearScenario(List{IBaseAgent})"/>
  public void ClearScenario(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {
      ((BasicGAAgentParallel)agent).logger.SetEndTime(Time.realtimeSinceStartupAsDouble);
      ((BasicGAAgentParallel)agent).logger.CreateCsv();
      ((BasicGAAgentParallel)agent).logger.AppendCsvLog();
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
      ((BasicGAAgentParallel)agent).logger.SetAgentId(agent.id.ToString());
      ((BasicGAAgentParallel)agent).logger.SetScenarioId(_scenarioName);
      ((BasicGAAgentParallel)agent).logger.SetStartTime(Time.realtimeSinceStartupAsDouble);
    }
  }
}
