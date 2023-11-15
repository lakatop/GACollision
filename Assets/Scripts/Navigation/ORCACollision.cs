using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Concrete class for collision avoidance
/// Uses third party RVO2 library for all calculations
/// </summary>
public class ORCACollision : IBaseCollisionAvoider
{
  public IBaseAgent agent { get; private set; }
  private RVOAdapter _adapter { get; set; }
  private SimulationManager _simManager { get; set; }
  private Dictionary<int, int> _agentIdToOrcaIDMap { get; set; }

  private readonly float _timeStep = 0.01f;
  private float _lastUpdate = 0.0f;

  public ORCACollision() { }

  public ORCACollision(IBaseAgent agent)
  {
    this.agent = agent;
    this.agent.updateInterval = _timeStep;
    _adapter = RVOAdapter.Instance;
    _simManager = SimulationManager.Instance;
    _simManager.RegisterCollisionListener(this);
    _agentIdToOrcaIDMap = new Dictionary<int, int>();
    OnStart();
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.OnStart"/>
  public void OnStart()
  {
    _adapter.SetAgentDefaults(15.0f, 10, 5.0f, 5.0f, 0.5f, 8.0f, new RVO.Vector2(0.0f, 0.0f));
    _adapter.SetTimeStep(_timeStep);
    RegisterObstacles(_simManager._obstacles);
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.Update"/>
  public void Update()
  {
    _lastUpdate += Time.deltaTime;
    if (_lastUpdate < _timeStep)
      return;

    _lastUpdate = 0.0f;

    // Get all agents from simManager and update position (in ORCA simulation) to those which are
    // not of type ORCAAgent - this will ensure correct collision in the next calculation
    foreach (var agent in _simManager.GetAgents())
    {
      if (_agentIdToOrcaIDMap.TryGetValue(agent.id, out var orcaId))
      {
        _adapter.SetAgentPosition(orcaId, agent.position);
      }
      else
      {
        _adapter.SetAgentPosition(((ORCAAgent)agent)._orcaId, agent.position);
      }
    }

    _adapter.DoStep();
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.OnAgentAdded(IBaseAgent)"/>
  public void OnAgentAdded(IBaseAgent agent)
  {
    int id = _adapter.AddAgent(agent);
    if(agent is ORCAAgent)
    {
      ((ORCAAgent)agent)._orcaId = id;
    }
    else
    {
      // We need to keep track of agents other that ORCAAgent type
      // and update their postiion before every update step.
      // ORCAAgents are updating their position in ORCA simulation themselves.
      _agentIdToOrcaIDMap.Add(agent.id, id);
    }
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentPosition(int)"/>
  public Vector2 GetAgentPosition(int id)
  {
    return _adapter.GetAgentPosition(id);
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.SetAgentPreferredVelocity(int, Vector2)"/>
  public void SetAgentPreferredVelocity(int id, Vector2 prefVelocity)
  {
    _adapter.SetAgentPrefVelocity(id, prefVelocity);
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentPreferredVelocity(int)"/>
  public Vector2 GetAgentPreferredVelocity(int id)
  {
    return _adapter.GetAgentPreferredVelocity(id);
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentVelocity(int)"/>
  public Vector2 GetAgentVelocity(int id)
  {
    return _adapter.GetAgentVelocity(id);
  }

  public void RegisterObstacles(List<Obstacle> obstacles)
  {
    foreach (var obstacle in obstacles)
    {
      _adapter.AddObstacle(obstacle.vertices);
    }

    _adapter.ProcessObstacles();
  }
}