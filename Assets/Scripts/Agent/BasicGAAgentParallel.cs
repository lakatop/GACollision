using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;

/// <summary>
/// Basic EA agent
/// For collision avoidance and path planning use NavMeshAgent (RVO + A*)
/// </summary>
public class BasicGAAgentParallel : BaseAgent
{
  public override IBaseCollisionAvoider collisionAlg { get; set; }
  public override IBasePathPlanner pathPlanningAlg { get; set; }
  private NavMeshAgent _navMeshAgent { get; set; }
  private NavMeshPath _path { get; set; }
  private int _cornerIndex { get; set; }
  private Vector2 nextVel { get; set; }
  private Vector2 previousLocation { get; set; }
  private GeneticAlgorithmDirector _gaDirector { get; set; }
  private JobHandle _gaJobHandle { get; set; }
  private BasicGeneticAlgorithmParallel gaJob { get; set; }
  private bool jobScheduled { get; set; }
  private float _updateTimer { get; set; }
  private bool _runGa { get; set; }
  private int iteration { get; set; }

  public BasicGAAgentParallel()
  {
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _gaDirector = new GeneticAlgorithmDirector();
    _navMeshAgent.autoBraking = false;
    _path = new NavMeshPath();
    speed = 5.0f;
    jobScheduled = false;
    _updateTimer = 0f;
    nextVel = Vector2.zero;
    _runGa = true;
    iteration = 0;
    previousLocation = position;
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
      if (_path.status == NavMeshPathStatus.PathComplete && _path.corners.Length > 0)
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

  // Run GA and get results
  public override void OnBeforeUpdate()
  {
    destination = CalculateNewDestination();

    if ((position - destination).magnitude <= 0.1f)
    {
      nextVel = Vector2.zero;
      return;
    }

    if (_runGa)
    {
      // Run GA
      gaJob = (BasicGeneticAlgorithmParallel)_gaDirector.MakeBasicGAParallel(this);

      jobScheduled = true;
      _gaJobHandle = gaJob.Schedule();

      _runGa = false;
    }

  }

  // Set agent position and start new EA cycle
  public override void OnAfterUpdate(Vector2 newPos)
  {
    _updateTimer += Time.deltaTime;
    if (jobScheduled && SimulationManager.Instance._agentUpdateInterval < _updateTimer)
    {
      _gaJobHandle.Complete();
      PathDrawer.DrawPath(previousLocation, position, nextVel);
      nextVel = gaJob._winner[0];
      Debug.Log(string.Format("Next winner {0}", nextVel));
      previousLocation = position;
      gaJob.logger.WriteRes(gaJob.GetConfiguration(), iteration);
      iteration++;
      gaJob.Dispose();

      jobScheduled = false;

      _updateTimer = 0f;
      _runGa = true;
    }

    Debug.Log(GetForward());
    PathDrawer.DrawDestination(destination);

    var vel = nextVel * Time.deltaTime;
    var pos = position + vel;

    SetPosition(pos);
    SetForward(vel.normalized);

    if ((pos - new Vector2(destination.x, destination.y)).sqrMagnitude < 1f && (_cornerIndex < (_path.corners.Length - 1)))
    {
      _cornerIndex++;
      destination = new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }
  }

  private Vector2 CalculateNewDestination()
  {
    // If there is no path, dont move really
    if (_path.status != NavMeshPathStatus.PathComplete)
    {
      return position;
    }
    else
    {
      return new Vector2(_path.corners[_cornerIndex].x, _path.corners[_cornerIndex].z);
    }
  }
}
