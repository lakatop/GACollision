using UnityEngine;

public class MyNavMeshAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }

  public override void Update()
  {
    if (Time.deltaTime < updateInterval)
    {
      return;
    }

    collisionAlg.Update();
  }

  public override void UpdatePosition()
  {
  }

  public MyNavMeshAgent()
  {
    collisionAlg = new NavMeshCollision(this);
    pathPlanningAlg = new NavMeshPathPlanner(this);
  }
}
