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
  private IBaseAgent _agent = null;
  private NavMeshAgent _navMeshAgent = null;

  public NavMeshCollision(IBaseAgent agent)
  {
    _agent = agent;
    if(_agent is MyNavMeshAgent)
    {
      _navMeshAgent = ((MyNavMeshAgent)_agent).GetComponent<NavMeshAgent>();
    }
  }

  /// <inheritdoc cref="IBaseCollisionAvoider.Update"/>
  public void Update()
  {
    if (_agent == null || _navMeshAgent == null)
      return;

    if (Math.Abs(_navMeshAgent.remainingDistance - _navMeshAgent.stoppingDistance) > 1f)
    {
      _agent.UpdatePosition(_navMeshAgent.desiredVelocity);
    }
    else
    {
      _agent.UpdatePosition(Vector3.zero);
    }
  }
}

