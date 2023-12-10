using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;
using Unity.Collections;

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
  private float _elapsedTime { get; set; }
  private Vector2 nextVel { get; set; }
  private GeneticAlgorithmDirector _gaDirector { get; set; }
  private int populationSize { get; set; }
  //private BasicGeneticAlgorithParallelBuilder _gaBuilder { get; set; }
  private JobHandle _gaJobHandle { get; set; }
  private BasicGeneticAlgorithmParallel gaJob { get; set; }
  private NativeArray<Vector2> winner { get; set; }
  private bool jobScheduled { get; set; }


  NativeArray<BasicIndividualStruct> offsprings;
  NativeArray<BasicIndividualStruct> parents;
  NativeArray<BasicIndividualStruct> selectedPop;
  NativeArray<double> relativeFit;
  NativeArray<double> wheel;
  NativeArray<BasicIndividualStruct> pop;

  public BasicGAAgentParallel()
  {
    collisionAlg = SimulationManager.Instance._collisionManager.GetOrCreateCollisionAlg<ORCACollision>(() => new ORCACollision(this));
    //_geneticAlgorithm = SimulationManager.Instance._collisionManager.GetOrCreateGeneticAlgorithm();
    pathPlanningAlg = new NavMeshPathPlanner(this);
    _navMeshAgent = GetComponent<NavMeshAgent>();
    _gaDirector = new GeneticAlgorithmDirector();
    //_gaBuilder = new BasicGeneticAlgorithParallelBuilder();
    _navMeshAgent.autoBraking = false;
    _path = new NavMeshPath();
    _elapsedTime = 0.0f;
    speed = 5.0f;
    populationSize = 10;
    jobScheduled = false;
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
    //_elapsedTime += Time.deltaTime;
    //if (_elapsedTime < updateInterval)
    //  return;

    //_elapsedTime = 0.0f;
    destination = CalculateNewDestination();

    if ((position - destination).magnitude <= 0.1f)
    {
      nextVel = Vector2.zero;
      return;
    }

    // Run GA
    // Create cross
    offsprings = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    parents = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    BasicCrossOperatorParallel cross = new BasicCrossOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      offsprings = offsprings,
      parents = parents
    };

    // Create mutation
    BasicMutationOperatorParallel mut = new BasicMutationOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      _agentSpeed = speed,
      _timeDelta = Time.deltaTime
    };

    // Create fitness
    BasicFitnessFunctionParallel fit = new BasicFitnessFunctionParallel()
    {
      _startPosition = position,
      _destination = destination,
      _agentRadius = 0.5f,
      _agentIndex = id,
      //_quadTree = SimulationManager.Instance.GetQuadTree()
    };

    // Create selection
    selectedPop = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    relativeFit = new NativeArray<double>(populationSize, Allocator.TempJob);
    wheel = new NativeArray<double>(populationSize, Allocator.TempJob);
    BasicSelectionFunctionParallel sel = new BasicSelectionFunctionParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      //selectedPop = selectedPop,
      //relativeFitnesses = relativeFit,
      //wheel = wheel
    };

    // Create GA
    pop = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    winner = new NativeArray<Vector2>(1, Allocator.TempJob);
    gaJob = new BasicGeneticAlgorithmParallel()
    {
      cross = cross,
      mut = mut,
      fit = fit,
      sel = sel,
      pop = pop,
      iterations = 10,
      populationSize = populationSize,
      _winner = winner,
      _timeDelta = Time.deltaTime,
      _agentSpeed = speed,
      _startPosition = position,
      //offsprings = offsprings,
      //parents = parents,
      _quadTree = SimulationManager.Instance.GetQuadTree(),
      selectedPop = selectedPop,
      relativeFitnesses = relativeFit,
      wheel = wheel,
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime))
    };

    jobScheduled = true;
    _gaJobHandle = gaJob.Schedule();

  }

  // Set agent position and start new EA cycle
  public override void OnAfterUpdate(Vector2 newPos)
  {
    nextVel = Vector2.zero;

    if (jobScheduled)
    {
      _gaJobHandle.Complete();

      offsprings.Dispose();
      parents.Dispose();
      selectedPop.Dispose();
      relativeFit.Dispose();
      wheel.Dispose();
      pop.Dispose();

      nextVel = gaJob._winner[0];

      jobScheduled = false;
    }

    var vel = nextVel;
    var pos = position + nextVel;

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