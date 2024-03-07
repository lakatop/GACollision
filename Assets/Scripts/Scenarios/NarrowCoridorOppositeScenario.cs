using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple scenario where agent start needs to turn into the opposite direction in a very narrow corridor
/// </summary>
public class NarrowCoridorOppositeScenario : IScenario
{
  private const string _scenarioName = "narrowCoridorOpposite";

  public int runCounter { get; set; }

  public NarrowCoridorOppositeScenario(int runCount)
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

      Vector2 spawnPosition = Vector2.zero;
      Vector2 destination = Vector2.zero;
      if (i < 5)
      {
        spawnPosition = new Vector2(-5 + (i * 2), -40);
        destination = new Vector2(-5 + (i * 2), 50);
      }
      else
      {
        spawnPosition = new Vector2(-5 + ((i - 5) * 2), 40);
        destination = new Vector2(-5 + ((i - 5) * 2), -50);
      }

  ((BaseAgent)agent).SpawnPosition(spawnPosition);
      agent.SetDestination(destination);
      agent.SetForward((destination - spawnPosition).normalized);
      ((BaseAgent)agent).scenarioName = _scenarioName;
    }
  }
}