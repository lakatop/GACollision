using RVO;
using UnityEngine;

class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }

  public override void SetDestination(RVO.Vector2 des)
  {
    throw new System.NotImplementedException();
  }

  public override void Update()
  {
    throw new System.NotImplementedException();
  }

  public override void UpdatePosition(RVO.Vector2 newPos)
  {
    throw new System.NotImplementedException();
  }

  public override void ComputeNeighbors()
  {
    neighborObstacles.Clear();
    float rangeSq = RVOMath.sqr(timeHorizonObst * maxSpeed + radius);
    AgentManager.GetInstance().kdTree.computeObstacleNeighbors(this, rangeSq);

    neighbors.Clear();

    if (maxNeighbors > 0)
    {
      rangeSq = RVOMath.sqr(neighborDist_);
      Simulator.Instance.kdTree_.computeAgentNeighbors(this, ref rangeSq);
    }
  }
}