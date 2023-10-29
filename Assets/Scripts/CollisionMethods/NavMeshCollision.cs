using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using UnityStandardAssets.Characters.ThirdPerson;

/// <summary>
/// Defines interface for collision avoidance algorithms
/// </summary>
public interface IBaseCollision
{
  /// <summary>
  /// Use to trigger update on collision algorithm
  /// </summary>
  void CollisionUpdate();
  /// <summary>
  /// TODO
  /// </summary>
  void OnDestinationChange();
}

public class NavMeshCollision : IBaseCollision
{
  private IBaseAgent _agent = null;
  private NavMeshAgent _navMeshAgent = null;
  private ThirdPersonCharacter _thirdPersonCharacter = null;

  public NavMeshCollision(IBaseAgent agent)
  {
    _agent = agent;
    if(_agent is Agent)
    {
      _navMeshAgent = (NavMeshAgent)((Agent)_agent).GetComponent(GlobStrings.Components.kNavMeshAgent);
      _thirdPersonCharacter = (ThirdPersonCharacter)((Agent)_agent).GetComponent(GlobStrings.Components.kThirdPersonCharacter);
    }

  }

  public void CollisionUpdate()
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

  public void OnDestinationChange()
  {
    SetDestination();
  }

  private void SetDestination()
  {
    if (!_navMeshAgent)
      return;

    _navMeshAgent.SetDestination(_agent.destination);
  }
}

