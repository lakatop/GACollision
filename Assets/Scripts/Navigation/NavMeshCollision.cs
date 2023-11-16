using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

/// <summary>
/// Concrete class for collision avoidance
/// Uses UnityEngine.AI.NavMeshAgent for all calculations
/// </summary>
public class NavMeshCollision : IBaseCollisionAvoider
{
  public IBaseAgent agent { get; private set; }
  private NavMeshAgent _navMeshAgent = null;

  public NavMeshCollision(IBaseAgent agent)
  {
    this.agent = agent;
    if(this.agent is MyNavMeshAgent)
    {
      _navMeshAgent = ((MyNavMeshAgent)this.agent).GetComponent<NavMeshAgent>();
    }
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.Update"/>
  public void Update()
  {
    if (agent == null || _navMeshAgent == null)
      return;

    if (Math.Abs(_navMeshAgent.remainingDistance - _navMeshAgent.stoppingDistance) > 1f)
    {
      agent.UpdatePosition(new Vector2(_navMeshAgent.desiredVelocity.x, _navMeshAgent.desiredVelocity.z));
    }
    else
    {
      agent.UpdatePosition(Vector3.zero);
    }
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.OnStart"/>
  public void OnStart()
  {
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.OnAgentAdded(IBaseAgent)"/>
  public void OnAgentAdded(IBaseAgent agent)
  {
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentPosition(int)"/>
  public Vector2 GetAgentPosition(int id)
  {
    return agent.position;
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.SetAgentPreferredVelocity(int, Vector2)"/>
  public void SetAgentPreferredVelocity(int id, Vector2 prefVelocity)
  {
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentPreferredVelocity(int)"/>
  public Vector2 GetAgentPreferredVelocity(int id)
  {
    return _navMeshAgent.desiredVelocity;
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.GetAgentVelocity(int)"/>
  public Vector2 GetAgentVelocity(int id)
  {
    return _navMeshAgent.velocity;
  }

  public void RegisterObstacles(List<Obstacle> obstacles)
  {
  }
}

