using System.Collections.Generic;
using UnityEngine;

public interface IScenario
{
  /// <summary>
  /// Populates scene with agents, gives them their destination
  /// </summary>
  /// <param name="agents">SimulationManager's list of agents</param>
  public void SetupScenario(List<IBaseAgent> agents);
  /// <summary>
  /// Counter how many times this scenario should run
  /// </summary>
  public int runCounter { get; set; }
}