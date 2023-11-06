using UnityEngine;
using System;

public class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  public int _orcaId { get; set; }

  private System.Random random;

  public ORCAAgent()
  {
    collisionAlg = new ORCACollision(this);
    _orcaId = -1;
    random = new System.Random();
  }

  public override void SetDestination(Vector2 des)
  {
    destination = des;
  }

  public override void Update()
  {
    if (_orcaId == -1)
      return;

    var pos = collisionAlg.GetAgentPosition(_orcaId);
    SetPosition(pos);

    var goalVector = destination - pos;
    if(goalVector.sqrMagnitude > 1.0f)
    {
      goalVector = goalVector.normalized;
    }
    collisionAlg.SetAgentPreferredVelocity(_orcaId, goalVector);
    /* Perturb a little to avoid deadlocks due to perfect symmetry. */
    float angle = (float)random.NextDouble() * 2.0f * (float)Math.PI;
    float dist = (float)random.NextDouble() * 0.0001f;
    collisionAlg.SetAgentPreferredVelocity(_orcaId, collisionAlg.GetAgentPreferredVelocity(_orcaId) +
                    dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
  }

  public override void UpdatePosition(Vector2 newPos)
  {
    SetPosition(newPos);
  }
}