using System;
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

  public void OnStart()
  {
  }

  public void OnAgentAdded(IBaseAgent agent)
  {
  }

  public Vector2 GetAgentPosition(int id)
  {
    return agent.position;
  }

  public void SetAgentPreferredVelocity(int id, Vector2 prefVelocity)
  {
  }

  public Vector2 GetAgentPreferredVelocity(int id)
  {
    return _navMeshAgent.desiredVelocity;
  }

  public Vector2 GetAgentVelocity(int id)
  {
    return _navMeshAgent.velocity;
  }
}

