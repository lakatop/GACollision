using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple scenario where agent start at position (0,0) and should go straight line to the destination (0,40)
/// </summary>
public class StraightLineScenario : IScenario
{
  private const string _scenarioName = "straightLine";

  public int runCounter { get; set; }

  public StraightLineScenario(int runCount)
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