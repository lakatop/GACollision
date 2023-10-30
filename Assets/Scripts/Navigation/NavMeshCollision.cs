using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityStandardAssets.Characters.ThirdPerson;


public class NavMeshCollision : IBaseCollisionAvoider
{
  private IBaseAgent _agent = null;
  private NavMeshAgent _navMeshAgent = null;
  private ThirdPersonCharacter _thirdPersonCharacter = null;

  public NavMeshCollision(IBaseAgent agent)
  {
    _agent = agent;
    if(_agent is MyNavMeshAgent)
    {
      _navMeshAgent = ((MyNavMeshAgent)_agent).GetComponent<NavMeshAgent>();
      _thirdPersonCharacter = ((MyNavMeshAgent)_agent).GetComponent<ThirdPersonCharacter>();
    }
  }

  /// <inheritdoc cref="IBaseCollision.CollisionUpdate"/>
  public void Update()
  {
    if (!_thirdPersonCharacter)
      return;

    if (Math.Abs(_navMeshAgent.remainingDistance - _navMeshAgent.stoppingDistance) > 1f)
    {
      _thirdPersonCharacter.Move(_navMeshAgent.desiredVelocity, false, false);
    }
    else
    {
      _thirdPersonCharacter.Move(Vector3.zero, false, false);
    }
  }
}

