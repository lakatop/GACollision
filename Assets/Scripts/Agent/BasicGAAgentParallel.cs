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
  public Vector2 nextVel { get; set; }
  private NavMeshAgent _navMeshAgent { get; set; }
  private NavMeshPath _path { get; set; }
  private int _cornerIndex { get; set; }
  private Vector2 previousLocation { get; set; }
  private GeneticAlgorithmDirector _gaDirector { get; set; }
  private JobHandle _gaJobHandle { get; set; }
  private BezierGeneticAlgorithmParallel gaJob { get; set; }
  Unity.Collections.NativeArray<Vector2> _winner { get; set; }
  private bool jobScheduled { get; set; }
  private float _updateTimer { get; set; }
  private bool _runGa { get; set; }
  private int iteration { get; set; }
  private float pathSize = 12.5f;
  private int maxSkipDestinations = 1;

  public BasicGAAgentParallel()
  {
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _gaDirector = new GeneticAlgorithmDirector();
    _navMeshAgent.autoBraking = false;
    _path = new NavMeshPath();
    speed = 5.0f;
    jobScheduled = false;
    _updateTimer = SimulationManager.Instance._agentUpdateInterval;
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
      if (_path.status == NavMeshPathStatus.PathComplete && _path.corners.Length > 1)
      {
        destination = new Vector2(_path.corners[1].x, _path.corners[1].z); // on index 0 there is current agents position
        _cornerIndex = 1;

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
            if(totalSize + interSize > pathSize)
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
      else
      {
        destination = position;
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
    //destination = CalculateNewDestination();
    _updateTimer += Time.deltaTime;

    if ((position - destination).magnitude <= 0.1f)
    {
      nextVel = Vector2.zero;
      return;
    }

    SetDestination(new Vector2(_path.corners[_path.corners.Length - 1].x, _path.corners[_path.corners.Length - 1].z));

    if (_runGa && SimulationManager.Instance._agentUpdateInterval < _updateTimer)
    {
      // Run GA
      gaJob = (BezierGeneticAlgorithmParallel)_gaDirector.MakeBezierGAParallel(this);

      _winner = gaJob._winner;

      jobScheduled = true;
      _gaJobHandle = gaJob.Schedule();

      _runGa = false;
    }
  }

  // Set agent position and start new EA cycle
  public override void OnAfterUpdate(Vector2 newPos)
  {
    if (jobScheduled)
    {
      _gaJobHandle.Complete();
      jobScheduled = false;
      Debug.Log(string.Format("Position {0}", position));
      Debug.Log(string.Format("Previous position {0}", previousLocation));
      PathDrawer.DrawPath(previousLocation, position, nextVel * SimulationManager.Instance._agentUpdateInterval);
      nextVel = _winner[0];
      Debug.Log(string.Format("Next winner {0}", nextVel));
      previousLocation = position;
      //gaJob.logger.WriteRes(gaJob.GetConfiguration(), iteration, scenarioName, id.ToString());
      iteration++;
      gaJob.Dispose();

      jobScheduled = false;

      _updateTimer = 0f;
      _runGa = true;
    }

    //Debug.Log(string.Format("Forward {0}", GetForward()));
    if(_path.status == NavMeshPathStatus.PathComplete)
    {
      foreach (var corner in _path.corners)
      {
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

  void OnDestroy()
  {
    if (jobScheduled)
    {
      _gaJobHandle.Complete();
      gaJob.Dispose();
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
