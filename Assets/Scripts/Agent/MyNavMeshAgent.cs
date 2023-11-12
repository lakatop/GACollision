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
  public override void SetDestination(Vector2 des)
  {
    destination = des;
    pathPlanningAlg.OnDestinationChange();
  }

  /// <inheritdoc cref="BaseAgent.UpdatePosition"/>
  public override void UpdatePosition(Vector2 newPos)
  {
    //SetPosition(newPos);
    Debug.Log(GetPos());
    if (!_thirdPersonCharacter)
      return;

    _thirdPersonCharacter.Move(new Vector3(newPos.x, 0, newPos.y), false, false);
  }
}
