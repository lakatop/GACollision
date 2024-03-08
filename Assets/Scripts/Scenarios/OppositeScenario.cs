using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where 2 agents go into opposite directions
/// </summary>
public class OppositeScenario : IScenario
{
  private const string _scenarioName = "oppositeAgents";

  public int runCounter { get; set; }

  public OppositeScenario(int runCount)
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

    Vector2 spawnPosition = new Vector2(0, -20);
    //Vector2 destination = new Vector2(0, 30);
    Vector2 destination = new Vector2(0, -20);

    ((BaseAgent)agent).SpawnPosition(spawnPosition);
    agent.SetDestination(destination);
    agent.SetForward((destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;


    agents.Add(new BasicGAAgentParallel());
    agent = agents[agents.Count - 1];
    agent.id = agents.Count;
    if (agent is BaseAgent)
    {
      ((BaseAgent)agent).SetName();
    }

    spawnPosition = new Vector2(0, 20);
    destination = new Vector2(0, -30);

    ((BaseAgent)agent).SpawnPosition(spawnPosition);
    agent.SetDestination(destination);
    agent.SetForward((destination - spawnPosition).normalized);
    ((BaseAgent)agent).scenarioName = _scenarioName;
  }
}