using UnityEngine;

class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }

  public override void SetDestination(Vector2 des)
  {
    throw new System.NotImplementedException();
  }

  public override void Update()
  {
    throw new System.NotImplementedException();
  }

  public override void UpdatePosition(Vector2 newPos)
  {
    throw new System.NotImplementedException();
  }
}