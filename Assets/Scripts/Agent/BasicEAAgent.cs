using UnityEngine;

/// <summary>
/// Basic EA agent
/// For collision avoidance and path planning use NavMeshAgent (RVO + A*)
/// </summary>
public class BasicEAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }

  public BasicEAAgent()
  {
    collisionAlg = SimulationManager.Instance._collisionManager.GetOrCreateCollisionAlg<ORCACollision>(() => new ORCACollision(this));
    pathPlanningAlg = new NavMeshPathPlanner(this);
  }

  public override void SetDestination(Vector2 des)
  {
  }

  // Stop EA evaluation and set desired direction so that collision avoidance alg can process it
  public override void OnBeforeUpdate()
  {
  }

  // Set agent position and start new EA cycle
  public override void OnAfterUpdate(Vector2 newPos)
  {
  }
}