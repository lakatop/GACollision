using Unity.Collections;
using UnityEngine;

public class GeneticAlgorithmDirector
{
  public GeneticAlgorithmDirector() { }

  public void MakeBasicGA(BasicGeneticAlgorithmBuilder builder, BaseAgent agent)
  {
    // Set crossover
    builder.SetCrossover(new BasicCrossOperator());

    // Set fitness
    var fitness = new BasicFitnessFunction();
    fitness.SetResources(new System.Collections.Generic.List<object>
    {
      agent.position,
      agent.destination,
      0.5f,
      agent.id,
      SimulationManager.Instance.GetQuadTree()
    });
    builder.SetFitness(fitness);

    // Set mutation
    var mutation = new BasicMutationOperator();
    mutation.SetResources(new System.Collections.Generic.List<object>
    {
      agent.speed,
      Time.deltaTime
    });
    builder.SetMutation(mutation);

    // Set selection
    builder.SetSelection(new BasicSelectionFunction());

    // Set population size and iterations
    var _gaAlg = builder.GetResult();
    _gaAlg.populationSize = 30;
    _gaAlg.iterations = 10;
    _gaAlg.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position
    });
  }

  public IGeneticAlgorithmParallel<BasicIndividualStruct> MakeBasicGAParallel (BaseAgent agent)
  {
    var ga = new BasicGeneticAlgorithmParallel();
    int populationSize = 50;
    int iterations = 100;
    int pathSize = 10;

    // Set crossover
    var offsprings = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    for (int i = 0; i < populationSize; i++)
    {
      var element = offsprings[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      offsprings[i] = element;
    }
    ga.cross = new MeanCrossOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      offsprings = offsprings,
      pathSize = pathSize,
      iterations = iterations,
    };

    // Set mutation
    ga.mutation = new GreedyCircleMutationOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      _destination = agent.destination,
      _agentPosition = agent.position,
      _forward = agent.GetForward(),
      _rotationAngle = 15,
      _agentSpeed = agent.speed,
      _updateInterval = SimulationManager.Instance._agentUpdateInterval
    };

    // Set fitness
    ga.fitness = new FitnessContinuousDistanceParallel()
    {
      _startPosition = agent.position,
      _destination = agent.destination,
      _agentRadius = 0.5f,
      _agentIndex = agent.id,
      _quadTree = SimulationManager.Instance.GetQuadTree(),
      _forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob)
  };

    // Set selection
    ga.selection = new NegativeSelectionParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
    };

    // Set initialization
    ga.popInitialization = new KineticFriendlyInitialization()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      populationSize = populationSize,
      agentSpeed = agent.speed,
      updateInterval = SimulationManager.Instance._agentUpdateInterval,
      pathSize = pathSize,
      startPosition = agent.position,
      forward = agent.GetForward()
    };

    //ga.popInitialization = new DebugInitialization()
    //{
    //  startPosition = agent.position,
    //  forward = new Vector2(0, 1),
    //};

    // Set logger
    //ga.logger = new StraightLineEvaluationLogger()
    //{
    //  _agentPosition = agent.position,
    //  _topIndividuals = new NativeArray<BasicIndividualStruct>(iterations + 1, Allocator.TempJob),
    //  _agentForward = agent.GetForward(),
    //  iteration = 0,
    //  _agentSpeed = agent.speed
    //};

    ga.populationSize = populationSize;
    ga.iterations = iterations;

    // Initialize population
    var population = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob);
    for (int i = 0; i < populationSize; i++)
    {
      var element = population[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      population[i] = element;
    } 
    ga.pop = new NativeBasicPopulation()
    {
      _population = population
    };

    ga._winner = new NativeArray<Vector2>(1, Allocator.TempJob);
    ga._rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime));
    ga.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position,
      agent.GetForward()
    });

    return ga;
  }
}
