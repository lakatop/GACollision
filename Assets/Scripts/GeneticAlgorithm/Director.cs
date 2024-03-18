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
    int iterations = 50;
    int pathSize = 10;
    float maxAcc = 1f;

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
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      offsprings = offsprings,
      pathSize = pathSize,
      iterations = iterations,
    };

    // Set mutation
    ga.mutation = new BasicMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      agentSpeed = agent.speed,
      updateInterval = SimulationManager.Instance.agentUpdateInterval,
      rotationRange = 15, 
    };

    // Set fitnesses
    ga.collisionFitness = new FitnessCollisionParallel()
    {
      startPosition = agent.position,
      destination = agent.destination,
      agentRadius = 0.5f,
      agentIndex = agent.id,
      quadTree = SimulationManager.Instance.GetQuadTree(),
      forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.6f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.endDistanceFitness = new FitnessEndDistanceParallel()
    {
      startPosition = agent.position,
      destination = agent.destination,
      forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.3f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.jerkFitness = new FitnessJerkCostParallel()
    {
      startPosition = agent.position,
      forward = agent.GetForward(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.1f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };

    // Set ranking
    ga.ranking = new WeightedSumRanking()
    {
      resultingFitnesses = new NativeArray<float>(populationSize, Allocator.TempJob)
    };

    // Set selection
    ga.selection = new ElitistSelectionParallel()
    {
    };

    // Set initialization
    ga.popInitialization = new KineticFriendlyInitialization()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      populationSize = populationSize,
      pathSize = pathSize,
    };


    //ga.popInitialization = new DebugInitialization()
    //{
    //  startPosition = agent.position,
    //  forward = agent.GetForward(),
    //  previousVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude
    //};

    // Set logger
    var topIndividuals = new NativeArray<BasicIndividualStruct>(iterations + 1, Allocator.TempJob);
    for (int i = 0; i < topIndividuals.Length; i++)
    {
      var element = topIndividuals[i];
      element.path = new Unity.Collections.LowLevel.Unsafe.UnsafeList<Unity.Mathematics.float2>(pathSize, Allocator.TempJob);
      element.path.Resize(pathSize);
      element.fitness = 0;
      topIndividuals[i] = element;
    }
    ga.logger = new FitnessEvaluationLogger()
    {
      topIndividuals = topIndividuals,
    };

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

    // Set population drawer
    ga.popDrawer = new BezierPopulationDrawer()
    {
    };

    ga.winner = new NativeArray<Vector2>(1, Allocator.TempJob);
    ga.rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime));
    ga.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position,
      agent.GetForward(),
      ((BasicGAAgentParallel)agent).nextVel.magnitude,
      maxAcc,
      SimulationManager.Instance.agentUpdateInterval,
    });

    return ga;
  }

  public IGeneticAlgorithmParallel<BezierIndividualStruct> MakeBezierGAParallel(BaseAgent agent)
  {
    var ga = new BezierGeneticAlgorithmParallel();
    int populationSize = 100; //100
    int iterations = 20; //20
    int pathSize = 7;
    float maxAcc = 1f;

    // Set crossover
    ga.cross = new UniformBezierCrossOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      parents = new NativeArray<BezierIndividualStruct>(2, Allocator.TempJob),
      crossProb = 0.1f,
    };

    // Set mutation
    ga.straightFinishMutation = new BezierStraightFinishMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      agentSpeed = agent.speed,
      updateInterval = SimulationManager.Instance.agentUpdateInterval,
      startPos = agent.position,
      destination = agent.destination,
      forward = agent.GetForward(),
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      mutationProb = 1.0f,
    };
    ga.clampVelocityMutation = new BezierClampVelocityMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      agentSpeed = agent.speed,
      updateInterval = SimulationManager.Instance.agentUpdateInterval,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      mutationProb = 1.0f,
    };
    ga.shuffleMutation = new BezierShuffleAccMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      mutationProb = 0.3f,
    };
    ga.smoothMutation = new BezierSmoothAccMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      mutationProb = 0.9f,
    };
    ga.controlPointsMutation = new BezierShuffleControlPointsMutationOperatorParallel()
    {
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime)),
      startPosition = agent.position,
      endPosition = agent.destination,
      forward = agent.GetForward(),
      mutationProb = 0.3f,
    };

    // Set fitnesses
    ga.collisionFitness = new BezierFitnessCollisionParallel()
    {
      startPosition = agent.position,
      agentRadius = 0.5f,
      agentIndex = agent.id,
      quadTree = SimulationManager.Instance.GetQuadTree(),
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.5f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };

    ga.endDistanceFitness = new BezierFitnessEndDistanceParallel()
    {
      startPosition = agent.position,
      destination = agent.destination,
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.2f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.jerkFitness = new BezierFitnessJerkCostParallel()
    {
      startPosition = agent.position,
      destination = agent.destination,
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.2f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };
    ga.ttdFitness = new BezierFitnessTimeToDestinationParallel()
    {
      startPosition = agent.position,
      destination = agent.destination,
      fitnesses = new NativeArray<float>(populationSize, Allocator.TempJob),
      weight = 0.1f,
      startVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc = maxAcc,
      updateInteraval = SimulationManager.Instance.agentUpdateInterval,
      maxAgentSpeed = agent.speed
    };

    // Set ranking
    ga.ranking = new BezierWeightedSumRanking()
    {
      resultingFitnesses = new NativeArray<float>(populationSize, Allocator.TempJob)
    };

    // Set selection
    ga.selection = new BezierElitistSelectionParallel()
    {
    };

    // Set initialization
    ga.popInitialization = new BezierInitialization()
    {
      populationSize = populationSize,
      pathSize = pathSize,
      startPosition = agent.position,
      endPosition = agent.destination,
      forward = agent.GetForward(),
      rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime))
    };

    // Set logger
    ga.logger = new BezierIndividualLogger()
    {
      individualFitness = new NativeArray<float>(iterations + 1, Allocator.TempJob),
      jerkFitness = new NativeArray<float>(iterations + 1, Allocator.TempJob),
      collisionFitness = new NativeArray<float>(iterations + 1, Allocator.TempJob),
      endDistanceFitness = new NativeArray<float>(iterations + 1, Allocator.TempJob),
      ttdFitness = new NativeArray<float>(iterations + 1, Allocator.TempJob)
    };

    //ga.popInitialization = new DebugInitialization()
    //{
    //  startPosition = agent.position,
    //  forward = agent.GetForward(),
    //  previousVelocity = ((BasicGAAgentParallel)agent).nextVel.magnitude
    //};

    ga.populationSize = populationSize;
    ga.iterations = iterations;

    // Initialize population
    var population = new NativeArray<BezierIndividualStruct>(populationSize, Allocator.TempJob);
    for (int i = 0; i < populationSize; i++)
    {
      var element = population[i];
      element.Initialize(pathSize, Allocator.TempJob); // *3 for 1 anchor point and 2 control points
      population[i] = element;
    }
    ga.pop = new NativeBezierPopulation()
    {
      population = population
    };

    // Set population drawer
    ga.popDrawer = new BezierPopulationDrawer()
    {
    };

    ga.winner = new NativeArray<Vector2>(1, Allocator.TempJob);
    ga.rand = new Unity.Mathematics.Random((uint)(uint.MaxValue * Time.deltaTime));
    ga.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position,
      agent.GetForward(),
      ((BasicGAAgentParallel)agent).nextVel.magnitude * SimulationManager.Instance.agentUpdateInterval,
      maxAcc,
      SimulationManager.Instance.agentUpdateInterval,
    });

    return ga;
  }
}
