//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Jobs;
//using UnityEngine;

//public class GeneticAlgorithm : IResourceManager
//{
//  /// <summary>
//  /// List holding all GA job handles
//  /// </summary>
//  public List<JobHandle> _gaJobHandles { get; private set; }
//  /// <summary>
//  /// List holding all GA jobs
//  /// </summary>
//  public List<GeneticAlgorithmJob> _gaJobs { get; private set; }
//  /// <summary>
//  /// Agents' positions used for GA alg
//  /// </summary>
//  public NativeArray<Vector2> _gaPositions { get; private set; }
//  /// <summary>
//  /// Agents' speed used for GA alg
//  /// </summary>
//  public NativeArray<float> _gaSpeeds { get; private set; }
//  /// <summary>
//  /// Agents' paths used for GA alg
//  /// </summary>
//  public NativeArray<UnsafeList<Vector2>> _gaPaths { get; private set; }
//  /// <summary>
//  /// Count for loop iterations in GA alg
//  /// </summary>
//  public int _gaLoopIterations { get; private set; }
//  /// <summary>
//  /// Whether allocation of resources is needed
//  /// This should typically be set to true after GA ended
//  /// and we need to create list with updated positions, speeds, ...
//  /// </summary>
//  private bool _needsAllocation { get; set; }

//  public GeneticAlgorithm()
//  {
//    _gaLoopIterations = 10;
//    _needsAllocation = true;
//    _gaJobHandles = new List<JobHandle>();
//    _gaJobs = new List<GeneticAlgorithmJob>();
//    SimulationManager.Instance.RegisterResourceListener(this);
//  }

//  public void OnAfterUpdate()
//  {
//    bool allJobsEnded = true;
//    foreach (var jobHandle in _gaJobHandles)
//    {
//      if (!jobHandle.IsCompleted)
//      {
//        allJobsEnded = false;
//      }
//    }

//    // Check if all jobs are done
//    // If yes, we need to free resources
//    if (allJobsEnded)
//    {
//      _gaPositions.Dispose();
//      _gaSpeeds.Dispose();
//      foreach (var path in _gaPaths)
//      {
//        path.Dispose();
//      }
//      _gaPaths.Dispose();

//      _needsAllocation = true;

//      // Clear lists holding all jobs (handles)
//      _gaJobHandles.Clear();
//      _gaJobs.Clear();
//    }
//  }

//  public void OnBeforeUpdate()
//  {
//    if (_needsAllocation)
//    {
//      _gaPositions = new NativeArray<Vector2>(SimulationManager.Instance.GetAgents().Count, Allocator.TempJob);
//      _gaSpeeds = new NativeArray<float>(SimulationManager.Instance.GetAgents().Count, Allocator.TempJob);
//      _gaPaths = new NativeArray<UnsafeList<Vector2>>(SimulationManager.Instance.GetAgents().Count, Allocator.TempJob);

//      _needsAllocation = false;
//    }
//  }

//  public void OnStart()
//  {
//  }

//  public GeneticAlgorithmJob CreateGeneticAlgorithmJob(BaseAgent agent)
//  {
//    var job = new GeneticAlgorithmJob()
//    {
//      index = agent.id,
//      loopIterations = _gaLoopIterations,
//      timeDelta = Time.deltaTime,
//      positions = _gaPositions,
//      speeds = _gaSpeeds,
//      paths = _gaPaths
//    };
//    _gaJobs.Add(job);

//    return job;
//  }

//  public JobHandle ScheduleGeneticAlgorithmJob (GeneticAlgorithmJob gaJob)
//  {
//    var handle = gaJob.Schedule();
//    _gaJobHandles.Add(handle);

//    return handle;
//  }
//}

class GeneticAlgorithmDirector
{
  public GeneticAlgorithmDirector() { }

  public void MakeBasicGA(BasicGeneticAlgorithmBuilder builder)
  {
    builder.SetCrossover(new BasicCrossOperator());
    builder.SetFitness(new BasicFitnessFunction());
    builder.SetMutation(new BasicMutationOperator());
    builder.SetSelection(new BasicSelectionFunction());
  }
}