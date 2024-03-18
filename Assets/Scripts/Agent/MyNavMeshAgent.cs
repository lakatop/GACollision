using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Agent class with collision and path planning handled by Unity NavMeshAgent
/// </summary>
public class MyNavMeshAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }

  private NavMeshAgent _navMeshAgent { get; set; }


  public MyNavMeshAgent()
  {
    collisionAlg = new NavMeshCollision(this);
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    speed = 5;
  }

  /// <inheritdoc cref="BaseAgent.OnBeforeUpdate"/>
  public override void OnBeforeUpdate()
  {
  }

  /// <inheritdoc cref="BaseAgent.SetDestination(Vector3)"/>
  public override void SetDestination(Vector2 des)
  {
    destination = des;
    pathPlanningAlg.OnDestinationChange();
    while (_navMeshAgent.pathPending)
    {
      continue;
    }

    if (_navMeshAgent.path.status == NavMeshPathStatus.PathComplete)
    {
      _navMeshAgent.SetDestination(new Vector3(destination.x, 0, destination.y));
    }
  }

  /// <inheritdoc cref="BaseAgent.OnAfterUpdate"/>
  public override void OnAfterUpdate(Vector2 newPos)
  {
    SetPosition(new Vector2(_navMeshAgent.transform.position.x, _navMeshAgent.transform.position.z));
    SetForward(new Vector2(_navMeshAgent.transform.forward.x, _navMeshAgent.transform.forward.z));
    Debug.Log(GetVelocity());
  }
}
