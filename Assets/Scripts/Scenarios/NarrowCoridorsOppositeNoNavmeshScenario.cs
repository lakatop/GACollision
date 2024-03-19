using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Scenario where multiple agents are headed oposite to each other but there are obstacles along the way
/// that create narrow corridors and are not registered in navmesh
/// </summary>
public class NarrowCoridorsOppositeNoNavmeshScenario : IScenario
{
  private const string _scenarioName = "narrowCoridorsOppositeNoNavmeshScenario";

  public int runCounter { get; set; }

  public NarrowCoridorsOppositeNoNavmeshScenario(int runCount)
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

    AdditionalAgentSetup(agents);
  }

  public void ClearScenario(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {
      ((BasicGAAgentParallel)agent).logger.SetEndTime(Time.realtimeSinceStartupAsDouble);
      ((BasicGAAgentParallel)agent).logger.CreateCsv();
      ((BasicGAAgentParallel)agent).logger.AppendCsvLog();
    }
  }

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
