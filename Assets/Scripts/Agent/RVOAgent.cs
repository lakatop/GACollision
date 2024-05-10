using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic NavMeshAgent agent
/// </summary>
public class RVOAgent : BaseAgent
{
  /// <inheritdoc cref="IBaseAgent.pathPlanningAlg"/>
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  /// <summary>
  /// Next velocity of agent
  /// </summary>
  public Vector2 nextVel { get; set; }
  /// <summary>
  /// Logger class that logs agent's data
  /// </summary>
  public override ILogger logger { get; set; }
  /// <summary>
  /// NavMeshAgent component
  /// </summary>
  private NavMeshAgent _navMeshAgent { get; set; }
  private float _updateTimer { get; set; }

  public RVOAgent()
  {
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    speed = 5.0f;
    _navMeshAgent.speed = speed;
    _navMeshAgent.acceleration = 6.25f;
    _navMeshAgent.angularSpeed = 60;
    nextVel = Vector2.zero;
    inDestination = false;
    logger = new AgentLogger();
    _updateTimer = 0f;
  }

  /// <summary>
  /// Calculates path using pathPlanningAlg and sets destination from that path
  /// </summary>
  /// <param name="des">Destination of agent</param>
  public override void SetDestination(Vector2 des)
  {
    destination = des;
    pathPlanningAlg.OnDestinationChange();
  }

  /// <inheritdoc cref="BaseAgent.OnCollisionEnter"/>
  public override void OnCollisionEnter()
  {
    ((AgentLogger)logger).AddCollisionCount();
  }

  /// <inheritdoc cref="BaseAgent.OnCollisionStay"/>
  public override void OnCollisionStay()
  {
    ((AgentLogger)logger).AddFramesInCollision();
  }

  /// <summary>
  /// Scheduling GA and checking for destination arrival
  /// </summary>
  public override void OnBeforeUpdate()
  {
    _updateTimer += Time.deltaTime;

    var nextPos = GetComponent<NavMeshAgent>().nextPosition;
    var nextPos2D = new Vector2(nextPos.x, nextPos.z);
    if ((nextPos2D - destination).magnitude < 0.1f)
    {
      ((AgentLogger)logger).SetConfigurationId("RVO_config");
      inDestination = true;
    }
  }

  /// <summary>
  /// Setting new position, forward vector and destination of an agent
  /// </summary>
  public override void OnAfterUpdate()
  {
    if ((SimulationManager.Instance.agentUpdateInterval < _updateTimer) && !inDestination)
    {
      _updateTimer = 0f;


      var velocity = GetComponent<NavMeshAgent>().velocity;
      var vel2D = new Vector2(velocity.x, velocity.z);

      ((AgentLogger)logger).AddVelocity(vel2D);
      ((AgentLogger)logger).AddGaTime(_updateTimer);
    }
  }
}
