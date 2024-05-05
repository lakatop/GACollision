using System.Collections.Generic;

/// <summary>
/// Defines interface for scenarios
/// </summary>
public interface IScenario
{
  /// <summary>
  /// Populates scene with agents, gives them their destination
  /// </summary>
  /// <param name="agents">SimulationManager's list of agents</param>
  public void SetupScenario<T>(List<IBaseAgent> agents) where T: IBaseAgent, new();
  /// <summary>
  /// Perform any remaining actions before scenario resources will be cleared
  /// </summary>
  /// <param name="agents">SimulationManager's list of agents that were present in scenario</param>
  public void ClearScenario(List<IBaseAgent> agents);
  /// <summary>
  /// Counter how many times this scenario should run
  /// </summary>
  public int runCounter { get; set; }
}
