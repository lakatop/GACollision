using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where agent is required to go around the corner to be able to get to the destination
/// </summary>
public class CornerScenario : IScenario
{
  private const string _scenarioName = "cornerSingle";

  public int runCounter { get; set; }

  public CornerScenario(int runCount)
  {
    runCounter = runCount;
  }

  public void SetupScenario(List<IBaseAgent> agents)
  {
    agents.Add(new BasicGAAgentParallel());
    var agent = agents[agents.Count - 1];
    agent.id = agents.Count;
    if (agent is BaseAgent)
    {
      ((BaseAgent)agent).SetName();
    }

    Vector2 spawnPosition = new Vector2(-40, 20);
    Vector2 destination = new Vector2(-40, 30);

    ((BaseAgent)agent).SpawnPosition(spawnPosition);
    agent.SetDestination(destination);
    agent.SetForward((destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;
  }
}