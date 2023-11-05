using RVO;
using System.Collections.Generic;

public class RVOAdapter
{
  private Simulator adaptee;

  public RVOAdapter()
  {
    adaptee = Simulator.Instance;
  }

  /// <inheritdoc cref="Simulator.setTimeStep(float)"/>
  public void SetTimeStep(float timeStep)
  {
    adaptee.setTimeStep(timeStep);
  }

  /// <inheritdoc cref="Simulator.setAgentDefaults(float, int, float, float, float, float, Vector2)"/>
  public void SetAgentDefaults(float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst, float radius, float maxSpeed, Vector2 velocity)
  {
    adaptee.setAgentDefaults(neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);
  }

  /// <summary>
  /// Call <inheritdoc cref="Simulator.addAgent(Vector2)" on each agent in agents list/>
  /// </summary>
  /// <param name="agents">Agents to be added to RVO simulation</param>
  public void AddAgents(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {

    }
  }
}