using UnityEngine;
using System;
using UnityEngine.AI;

public class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  public Vector2 prevPos { get; set; }
  public int _orcaId { get; set; }
  private NavMeshAgent _navMeshAgent { get; set; }
  private NavMeshPath _path { get; set; }
  private int _cornerIndex {get;set;}
  private float elapsedTime = 0f;

  private System.Random random;

  public ORCAAgent()
  {
    collisionAlg = SimulationManager.Instance._collisionManager.GetOrCreateCollisionAlg<ORCACollision>(() => new ORCACollision(this));
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _orcaId = -1;
    random = new System.Random();
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _navMeshAgent.autoBraking = false;
    _path = new NavMeshPath();
    speed = 5.0f;
  }

  public override void SetDestination(Vector2 des)
  {
    destination = des;
    pathPlanningAlg.OnDestinationChange();
    _path = _navMeshAgent.path;
    while (_navMeshAgent.pathPending)
    {
      continue;
    }

    if (_path.status == NavMeshPathStatus.PathComplete)
    {
      if(_path.status == NavMeshPathStatus.PathComplete && _path.corners.Length > 0)
      {
        destination = new Vector2(_path.corners[0].x, _path.corners[0].z);
        _cornerIndex = 0;
      }
    }
    else
    {
      destination = position;
    }
  }

  public override void OnBeforeUpdate()
  {
    elapsedTime += Time.deltaTime;
    if (_orcaId == -1 && elapsedTime < updateInterval)
      return;

    elapsedTime = 0f;

    //Move agent
    Vector2 desiredDestination = CalculateNewDestination();
    var orcaPos = position;
    
    //Get next position
    Vector2 desiredVelocity = (desiredDestination - orcaPos) * speed;
    if ((desiredDestination - orcaPos).magnitude < 0.1f)
    {
      desiredVelocity = Vector2.zero;
      collisionAlg.SetAgentPreferredVelocity(_orcaId, desiredVelocity);
      return;
    }
    else if (desiredVelocity.magnitude < 1.0f)
      desiredVelocity = desiredVelocity.normalized;

    collisionAlg.SetAgentPreferredVelocity(_orcaId, desiredVelocity);
    ///* Perturb a little to avoid deadlocks due to perfect symmetry. */
    float angle = (float)random.NextDouble() * 2.0f * (float)Math.PI;
    float dist = (float)random.NextDouble() * 0.0001f;
    collisionAlg.SetAgentPreferredVelocity(_orcaId, collisionAlg.GetAgentPreferredVelocity(_orcaId) +
                    dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
  }

  public override void OnAfterUpdate(Vector2 desiredVelocity)
  {
    var pos = collisionAlg.GetAgentPosition(_orcaId);
    var vel = collisionAlg.GetAgentPreferredVelocity(_orcaId);

    SetPosition(pos);
    SetForward(vel);

    if ((pos - new Vector2(destination.x, destination.y)).sqrMagnitude < 1f && (_cornerIndex < (_path.corners.Length - 1)))
    {
      _cornerIndex++;
      destination = new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }
  }

  private Vector2 CalculateNewDestination()
  {
    // If there is no path, dont move really
    if(_path.status != NavMeshPathStatus.PathComplete)
    {
      return position;
    }
    else
    {
      return new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }
  }
}