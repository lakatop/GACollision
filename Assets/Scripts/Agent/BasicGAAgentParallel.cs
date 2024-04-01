using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;

/// <summary>
/// Basic parralel GA agent
/// For path planning use NavMeshAgent (A*)
/// </summary>
public class BasicGAAgentParallel : BaseAgent
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
  public AgentLogger logger { get; set; }
  /// <summary>
  /// NavMeshAgent component
  /// </summary>
  private NavMeshAgent _navMeshAgent { get; set; }
  /// <summary>
  /// Path representing result of path planning algorithm
  /// </summary>
  private NavMeshPath _path { get; set; }
  /// <summary>
  /// Index of which corner is currently used for destination
  /// </summary>
  private int _cornerIndex { get; set; }
  /// <summary>
  /// Keeps previous position of an agent
  /// </summary>
  private Vector2 previousLocation { get; set; }
  /// <summary>
  /// Director class for creating the GA
  /// </summary>
  private GeneticAlgorithmDirector _gaDirector { get; set; }
  /// <summary>
  /// Job handle of running GA
  /// </summary>
  private JobHandle _gaJobHandle { get; set; }
  /// <summary>
  /// Job struct that represents the GA
  /// </summary>
  private BezierGeneticAlgorithmParallel gaJob { get; set; }
  /// <summary>
  /// Winner velocity of the last GA run
  /// </summary>
  private Unity.Collections.NativeArray<Vector2> _winner { get; set; }
  /// <summary>
  /// Flag whether some job is scheduled
  /// </summary>
  private bool jobScheduled { get; set; }
  /// <summary>
  /// Temporary Time.deltaTime accumulator
  /// </summary>
  private float _updateTimer { get; set; }
  /// <summary>
  /// Flag whether we should run the GA or not
  /// </summary>
  private bool _runGa { get; set; }
  /// <summary>
  /// Maximum path agent can have in the GA
  /// </summary>
  private float pathSize = 17.5f;
  /// <summary>
  /// Maximum number of destinations from path that we can skip
  /// </summary>
  private int maxSkipDestinations = 1;
  /// <summary>
  /// Keeping track of how long the GA run
  /// </summary>
  private double _gaStartRunTime { get; set; }

  public BasicGAAgentParallel()
  {
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _navMeshAgent.autoBraking = false;
    _gaDirector = new GeneticAlgorithmDirector();
    _path = new NavMeshPath();
    speed = 5.0f;
    jobScheduled = false;
    _updateTimer = SimulationManager.Instance.agentUpdateInterval;
    nextVel = Vector2.zero;
    _runGa = true;
    previousLocation = position;
    inDestination = false;
    logger = new AgentLogger();
    _gaStartRunTime = 0f;
  }

  /// <summary>
  /// Calculates path using pathPlanningAlg and sets destination from that path
  /// </summary>
  /// <param name="des">Destination of agent</param>
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
      if (_path.status == NavMeshPathStatus.PathComplete && _path.corners.Length > 1)
      {
        destination = new Vector2(_path.corners[1].x, _path.corners[1].z); // on index 0 there is current agents position
        _cornerIndex = 1;
        CheckToSkipDestination();
      }
      else
      {
        destination = position;
      }
    }
    else
    {
      destination = position;
    }

    // This is to reset NavMeshAgent's default behaviour to start navigation after path planinng
    GetComponent<NavMeshAgent>().Warp(_object.transform.position);
  }

  private void CheckToSkipDestination()
  {
    // We are closer to destination than maximum path size
    // Check whether we can set destination further
    if ((position - destination).magnitude < pathSize && _path.corners.Length > 2)
    {
      int skippedDestinations = 0;
      var totalSize = (position - destination).magnitude;
      Vector3 newDestination = new Vector3(destination.x, 0.58f, destination.y);
      while (skippedDestinations < maxSkipDestinations && (_cornerIndex + skippedDestinations + 1 < _path.corners.Length))
      {
        var interDest = _path.corners[_cornerIndex + skippedDestinations + 1];
        var interSize = (_path.corners[_cornerIndex + skippedDestinations] - interDest).magnitude;
        if (totalSize + interSize > pathSize)
        {
          // find point on line
          var newDestDir = (_path.corners[_cornerIndex + skippedDestinations] - interDest).normalized;
          var moveBackSize = (totalSize + interSize) - pathSize;
          newDestination = interDest + (newDestDir * moveBackSize);
          break;
        }
        else
        {
          totalSize += interSize;
          newDestination = _path.corners[_cornerIndex + skippedDestinations + 1];
          skippedDestinations++;
        }
      }

      destination = new Vector2(newDestination.x, newDestination.z);
    }
  }

  /// <inheritdoc cref="BaseAgent.OnCollisionEnter"/>
  public override void OnCollisionEnter()
  {
    logger.AddCollisionCount();
  }

  /// <inheritdoc cref="BaseAgent.OnCollisionStay"/>
  public override void OnCollisionStay()
  {
    logger.AddFramesInCollision();
  }

  /// <summary>
  /// Scheduling GA and checking for destination arrival
  /// </summary>
  public override void OnBeforeUpdate()
  {
    _updateTimer += Time.deltaTime;

    if ((position - destination).magnitude <= 0.1f && nextVel.magnitude < 2f)
    {
      nextVel = Vector2.zero;
      inDestination = true;
      logger.SetConfigurationId(gaJob.GetHyperparametersId());
      logger.CreateConfigurationFile(gaJob.GetConfiguration());
      return;
    }

    inDestination = false;

    if (_runGa && SimulationManager.Instance.agentUpdateInterval < _updateTimer)
    {
      CheckToSkipDestination();
      // Run GA
      gaJob = (BezierGeneticAlgorithmParallel)_gaDirector.MakeBezierGAParallel(this);

      _winner = gaJob.winner;
      _gaStartRunTime = Time.realtimeSinceStartupAsDouble * 1000;

      jobScheduled = true;
      _gaJobHandle = gaJob.Schedule();

      _runGa = false;
    }
  }

  /// <summary>
  /// Setting new position, forward vector and destination of an agent
  /// </summary>
  public override void OnAfterUpdate()
  {
    if (jobScheduled)
    {
      _gaJobHandle.Complete();
      jobScheduled = false;
      PathDrawer.DrawPath(previousLocation, position, nextVel * SimulationManager.Instance.agentUpdateInterval);
      nextVel = _winner[0];
      previousLocation = position;
      logger.AddVelocity(nextVel);
      logger.AddGaTime((Time.realtimeSinceStartupAsDouble * 1000) - _gaStartRunTime);
      gaJob.Dispose();

      jobScheduled = false;

      _updateTimer = 0f;
      _runGa = true;
    }

    //Debug.Log(string.Format("Forward {0}", GetForward()));
    if(_path.status == NavMeshPathStatus.PathComplete)
    {
      for (int i = 1; i < _path.corners.Length; i++)
      {
        var corner = _path.corners[i];
        PathDrawer.DrawDestination(new Vector2(corner.x, corner.z), Color.white);
      }
      PathDrawer.DrawDestination(destination, Color.yellow);
    }

    var vel = nextVel * Time.deltaTime;
    var pos = position + vel;

    SetPosition(pos);
    SetForward((vel.normalized).magnitude == 0 ? GetForward() : vel.normalized);

    if ((pos - new Vector2(destination.x, destination.y)).magnitude < 1f && (_cornerIndex < (_path.corners.Length - 1)))
    {
      _cornerIndex++;
      destination = new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }
  }
}
