using UnityEngine;
using System;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

public class ORCAAgent : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  public Vector2 prevPos { get; set; }
  public int _orcaId { get; set; }
  private ThirdPersonCharacter _thirdPersonCharacter = null;
  private NavMeshAgent _navMeshAgent { get; set; }
  private float speed = 5.0f;
  private NavMeshPath _path { get; set; }
  private int _cornerIndex {get;set;}

  private System.Random random;

  public ORCAAgent()
  {
    collisionAlg = SimulationManager.Instance._collisionManager.GetOrCreateCollisionAlg<ORCACollision>(() => new ORCACollision(this));
    _orcaId = -1;
    random = new System.Random();
    _thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _navMeshAgent.autoBraking = false;
    _path = new NavMeshPath();
  }

  public override void SetDestination(Vector2 des)
  {
    //destination = des;
    //pathPlanningAlg.OnDestinationChange();
    _navMeshAgent.CalculatePath(new Vector3(des.x, 0, des.y), _path);
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

  public override void Update()
  {
    if (_orcaId == -1)
      return;

    //Move agent
    Vector2 desiredDestination = CalculateNewDestination();
    var orcaPos = collisionAlg.GetAgentPosition(_orcaId);
    
    //Get next postiion
    Vector2 desiredVelocity = (desiredDestination - orcaPos) * speed;
    if ((desiredDestination - orcaPos).magnitude < 0.01f)
    {
      desiredVelocity = Vector2.zero;
      collisionAlg.SetAgentPreferredVelocity(_orcaId, desiredVelocity);
      return;
    }
    else if (desiredVelocity.magnitude < 1.0f)
      desiredVelocity = desiredVelocity.normalized;
    //UpdatePosition(desiredVelocity);

    //var velocity = collisionAlg.GetAgentVelocity(_orcaId);
    //var prefVelocity = collisionAlg.GetAgentPreferredVelocity(_orcaId);
    //var pos = collisionAlg.GetAgentPosition(_orcaId);
    ////prevPos = prevPos + velocity;
    //UpdatePosition(prefVelocity);

    collisionAlg.SetAgentPreferredVelocity(_orcaId, desiredVelocity);
    ///* Perturb a little to avoid deadlocks due to perfect symmetry. */
    float angle = (float)random.NextDouble() * 2.0f * (float)Math.PI;
    float dist = (float)random.NextDouble() * 0.0001f;
    collisionAlg.SetAgentPreferredVelocity(_orcaId, collisionAlg.GetAgentPreferredVelocity(_orcaId) +
                    dist * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
  }

  public override void UpdatePosition(Vector2 desiredVelocity)
  {
    var pos = collisionAlg.GetAgentPosition(_orcaId);
    var vel = collisionAlg.GetAgentPreferredVelocity(_orcaId);
    Vector2 newPos = position + vel; //((desiredVelocity * Time.deltaTime) + position);

    SetPosition(pos);
    SetForward(vel);

    if ((pos - new Vector2(destination.x, destination.y)).sqrMagnitude < 1f && (_cornerIndex < (_path.corners.Length - 1)))
    {
      _cornerIndex++;
      destination = new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }

    //if (Math.Abs((newPos - destination).sqrMagnitude) > 1f)
    //{
    //  _thirdPersonCharacter.Move(new Vector3(vel.x, 0, vel.y), false, false);
    //  SetPosition(newPos);
    //}
    //else if (_cornerIndex < (_path.corners.Length - 1))
    //{
    //  destination = _path.corners[_cornerIndex + 1];
    //  _cornerIndex++;
    //  _thirdPersonCharacter.Move(new Vector3(vel.x, 0, vel.y), false, false);
    //  SetPosition(newPos);
    //}
    //else
    //{
    //  _thirdPersonCharacter.Move(Vector3.zero, false, false);
    //}
  }

  private Vector2 CalculateNewDestination()
  {
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