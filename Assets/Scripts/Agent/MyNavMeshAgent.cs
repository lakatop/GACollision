using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

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
  }

  /// <inheritdoc cref="BaseAgent.Update"/>
  public override void Update()
  {
    if (Time.deltaTime < updateInterval)
    {
      return;
    }

    collisionAlg.Update();
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

  /// <inheritdoc cref="BaseAgent.UpdatePosition"/>
  public override void UpdatePosition(Vector2 newPos)
  {
    //SetPosition(newPos);
  }
}
