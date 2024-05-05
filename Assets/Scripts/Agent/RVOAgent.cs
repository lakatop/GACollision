using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic parralel GA agent
/// For path planning use NavMeshAgent (A*)
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

  public RVOAgent()
  {
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    speed = 5.0f;
    _navMeshAgent.speed = speed;
    nextVel = Vector2.zero;
    inDestination = false;
    logger = new AgentLogger();
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
  }

  /// <summary>
  /// Setting new position, forward vector and destination of an agent
  /// </summary>
  public override void OnAfterUpdate()
  {
  }
}
