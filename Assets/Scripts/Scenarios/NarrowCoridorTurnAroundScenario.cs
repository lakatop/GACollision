using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple scenario where agent start needs to turn into the opposite direction in a very narrow corridor
/// </summary>
public class NarrowCoridorTurnAroundScenario : IScenario
{
  private const string _scenarioName = "narrowCoridorTurnAround";

  public int runCounter { get; set; }

  public NarrowCoridorTurnAroundScenario(int runCount)
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
    agent.SetForward(-1 * (destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;
  }
}