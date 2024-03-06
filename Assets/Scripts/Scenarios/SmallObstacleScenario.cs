using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scenario where agent starts at position (0,0) and has destination set to (0,40)
/// Along the straight line, there is static obstacle placed which is not registered in the navmesh
/// </summary>
public class SmallObstacleScenario : IScenario
{
  private const string _scenarioName = "smallObstacle";

  public int runCounter { get; set; }

  public SmallObstacleScenario(int runCount)
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

    Vector2 spawnPosition = new Vector2(0, 0);
    Vector2 destination = new Vector2(0, 40);

    ((BaseAgent)agent).SpawnPosition(spawnPosition);
    agent.SetDestination(destination);
    agent.SetForward((destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;
  }
}