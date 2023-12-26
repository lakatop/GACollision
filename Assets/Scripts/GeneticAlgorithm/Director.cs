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
    int populationSize = 30;
    int iterations = 10;

    // Set crossover
    ga.cross = new BasicCrossOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      offsprings = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob),
      parents = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob)
    };

    // Set mutation
    ga.mutation = new BasicMutationOperatorParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      _agentSpeed = agent.speed,
      _timeDelta = SimulationManager.Instance._agentUpdateInterval
    };

    // Set fitness
    ga.fitness = new BasicFitnessFunctionParallel()
    {
      _startPosition = agent.position,
      _destination = agent.destination,
      _agentRadius = 0.5f,
      _agentIndex = agent.id,
      _quadTree = SimulationManager.Instance.GetQuadTree(),
      _forward = agent.GetForward()
    };

    // Set selection
    ga.selection = new BasicSelectionFunctionParallel()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      selectedPop = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob),
      relativeFitnesses = new NativeArray<double>(populationSize, Allocator.TempJob),
      wheel = new NativeArray<double>(populationSize, Allocator.TempJob)
    };

    // Set initialization
    ga.popInitialization = new GlobeInitialization()
    {
      _rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      populationSize = populationSize,
      agentSpeed = agent.speed,
      timeDelta = SimulationManager.Instance._agentUpdateInterval,
      pathSize = 50,
      startPosition = agent.position,
      forward = agent.GetForward()
    };

    // Set logger
    ga.logger = new StraightLineEvaluationLogger()
    {
      _agentPosition = agent.position,
      _topIndividuals = new NativeArray<BasicIndividualStruct>(iterations + 1, Allocator.TempJob),
      _agentForward = agent.GetForward(),
      iteration = 0,
      _agentSpeed = agent.speed
    };

    ga.populationSize = populationSize;
    ga.iterations = iterations;
    ga.pop = new NativeBasicPopulation()
    {
      _population = new NativeArray<BasicIndividualStruct>(populationSize, Allocator.TempJob)
    };
    ga._winner = new NativeArray<Vector2>(populationSize, Allocator.TempJob);
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
