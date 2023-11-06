using RVO;
using System.Collections.Generic;

public class RVOAdapter
{
  private Simulator _adaptee;
  private static RVOAdapter _instance = new RVOAdapter();

  public RVOAdapter()
  {
    _adaptee = Simulator.Instance;
  }

  public static RVOAdapter Instance
  {
    get
    {
      return _instance;
    }
  }

  /// <inheritdoc cref="Simulator.setTimeStep(float)"/>
  public void SetTimeStep(float timeStep)
  {
    _adaptee.setTimeStep(timeStep);
  }

  /// <inheritdoc cref="Simulator.setAgentDefaults(float, int, float, float, float, float, Vector2)"/>
  public void SetAgentDefaults(float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst, float radius, float maxSpeed, Vector2 velocity)
  {
    _adaptee.setAgentDefaults(neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);
  }

  /// <summary>
  /// Call <inheritdoc cref="Simulator.addAgent(Vector2)" on each agent in agents list/>
  /// </summary>
  /// <param name="agents">Agents to be added to RVO simulation</param>
  public void AddAgents(List<IBaseAgent> agents)
  {
    foreach (var agent in agents)
    {
      _adaptee.addAgent(new Vector2(agent.position.x, agent.position.y));
    }
  }

  /// <inheritdoc cref="Simulator.addAgent(Vector2)"/>
  public int AddAgent(IBaseAgent agent)
  {
    return _adaptee.addAgent(new Vector2(agent.position.x, agent.position.y));
  }

  /// <inheritdoc cref="Simulator.doStep"/>
  public void DoStep()
  {
    _adaptee.doStep();
  }

  /// <inheritdoc cref="Simulator.getAgentPosition(int)"/>
  public UnityEngine.Vector2 GetAgentPosition(int id)
  {
    var res = _adaptee.getAgentPosition(id);
    return new UnityEngine.Vector2(res.x(), res.y());
  }

  /// <inheritdoc cref="Simulator.setAgentPrefVelocity(int, Vector2)"/>
  public void SetAgentPrefVelocity(int id, UnityEngine.Vector2 velocity)
  {
    _adaptee.setAgentPrefVelocity(id, new Vector2(velocity.x, velocity.y));
  }

  public UnityEngine.Vector2 GetAgentPreferredVelocity(int id)
  {
    var res = _adaptee.getAgentPrefVelocity(id);
    return new UnityEngine.Vector2(res.x(), res.y());
  }

  public void setAgentPosition(int id, UnityEngine.Vector2 pos)
  {
    _adaptee.setAgentPosition(id, new Vector2(pos.x, pos.y));
  }
}