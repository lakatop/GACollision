using UnityEngine;
using System;
using UnityEngine.AI;

public class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  public Vector2 prevPos { get; set; }
  public int orcaId { get; set; }
  private NavMeshAgent _navMeshAgent { get; set; }
  private NavMeshPath _path { get; set; }
  private int _cornerIndex {get;set;}
  private float _elapsedTime = 0f;

  private System.Random _random;

  public ORCAAgent()
  {
    collisionAlg = SimulationManager.Instance._collisionManager.GetOrCreateCollisionAlg<ORCACollision>(() => new ORCACollision(this));
    pathPlanningAlg = new NavMeshPathPlanner(this);
    orcaId = -1;
    _random = new System.Random();
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
    _elapsedTime += Time.deltaTime;
    if (orcaId == -1 && _elapsedTime < updateInterval)
      return;

    _elapsedTime = 0f;

    //Move agent
    Vector2 desiredDestination = CalculateNewDestination();
    var orcaPos = position;
    
    //Get next position
    Vector2 desiredVelocity = (desiredDestination - orcaPos) * speed;
    if ((desiredDestination - orcaPos).magnitude < 0.1f)
    {
      desiredVelocity = Vector2.zero;
      collisionAlg.SetAgentPreferredVelocity(orcaId, desiredVelocity);
      return;
    }
    else if (desiredVelocity.magnitude < 1.0f)
      desiredVelocity = desiredVelocity.normalized;

    collisionAlg.SetAgentPreferredVelocity(orcaId, desiredVelocity);
    ///* Perturb a little to avoid deadlocks due to perfect symmetry. */
    float angle = (float)_random.NextDouble() * 2.0f * (float)Math.PI;
    float dist = (float)_random.NextDouble() * 0.0001f;
    collisionAlg.SetAgentPreferredVelocity(orcaId, collisionAlg.GetAgentPreferredVelocity(orcaId) +
                    dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
  }

  public override void OnAfterUpdate(Vector2 desiredVelocity)
  {
    var pos = collisionAlg.GetAgentPosition(orcaId);
    var vel = collisionAlg.GetAgentPreferredVelocity(orcaId);

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