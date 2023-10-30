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
  private ThirdPersonCharacter _thirdPersonCharacter = null;

  public MyNavMeshAgent()
  {
    collisionAlg = new NavMeshCollision(this);
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
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
  public override void SetDestination(Vector3 des)
  {
    destination = des;
    pathPlanningAlg.OnDestinationChange();
  }

  /// <inheritdoc cref="BaseAgent.UpdatePosition"/>
  public override void UpdatePosition(Vector3 newPos)
  {
    if (!_thirdPersonCharacter)
      return;

    _thirdPersonCharacter.Move(newPos, false, false);
  }
}
