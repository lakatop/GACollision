using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Unity.Collections;
using NativeQuadTree;
using UtilsGA;
using System.Linq;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

public class BasicIndividual 
{
  public BasicIndividual()
  {
    fitness = 0f;
    path = new List<float2>();
  }

  public float fitness { get; set; }
  public List<float2> path { get; set; }
}

[BurstCompile]
public struct BasicIndividualStruct
{
  public void Initialize(int length, Allocator allocator)
  {
    fitness = 0f;
    path = new UnsafeList<float2>(length, allocator);
  }

  public void Dispose()
  {
    path.Dispose();
  }

  public float fitness;
  public UnsafeList<float2> path;
}

public class BasicPopulation : IPopulation<BasicIndividual>
{
  public BasicPopulation()
  {
    _population = new List<BasicIndividual>();
  }

  public BasicIndividual[] GetPopulation()
  {
    return _population.ToArray();
  }

  public void SetPopulation(BasicIndividual[] population)
  {
    _population = new List<BasicIndividual>(population);
  }

  private List<BasicIndividual> _population { get; set; }
}

public struct NativeBasicPopulation : IPopulation<BasicIndividualStruct>
{
  public BasicIndividualStruct[] GetPopulation()
  {
    return _population.ToArray();
  }

  public void SetPopulation(BasicIndividualStruct[] population)
  {
    _population = new NativeList<BasicIndividualStruct>(population.Length, Allocator.Persistent);
  }

  private NativeList<BasicIndividualStruct> _population;
}


public class BasicGeneticAlgorithm : IGeneticAlgorithm<BasicIndividual>
{
  public BasicGeneticAlgorithm()
  {
  }

  public IPopulationModifier<BasicIndividual> crossover { get; set; }
  public IPopulationModifier<BasicIndividual> mutation { get; set; }
  public IPopulationModifier<BasicIndividual> fitness { get; set; }
  public IPopulationModifier<BasicIndividual> selection { get; set; }
  public int iterations { get; set; }
  public int populationSize { get; set; }
  public IPopulation<BasicIndividual> population { get; set; }

  private Vector2 _winner { get; set; }
  private float _timeDelta { get; set; }
  private float _agentSpeed { get; set; }
  private Vector2 _startPosition { get; set; }

  public void RunGA()
  {
    InitializePopulation();

    for (int i = 0; i < iterations; i++)
    {
      population = fitness.ModifyPopulation(population);
      population = selection.ModifyPopulation(population);
      population = crossover.ModifyPopulation(population);
      population = mutation.ModifyPopulation(population);
    }

    population = fitness.ModifyPopulation(population);
    SetWinner();
  }

  public void InitializePopulation()
  {
    population = new BasicPopulation();
    float rotationRange = 120f;
    List<BasicIndividual> pop = new List<BasicIndividual>();

    for (int i = 0; i < populationSize; i++)
    {
      var individual = new BasicIndividual();
      for (int j = 0; j < 10; j++)
      {
        var rotation = UnityEngine.Random.Range(-rotationRange, rotationRange);
        var size = UnityEngine.Random.Range(0f, _agentSpeed) * _timeDelta;
        individual.path.Add(new float2(rotation, size));
      }
      pop.Add(individual);
    }

    population.SetPopulation(pop.ToArray());


    for (int i = 0; i < pop.Count; i++)
    {
      var initialVector = _startPosition;
      var path = pop[i].path;
      for (int j = 0; j < path.Count; j++)
      {
        var v = UtilsGA.UtilsGA.CalculateRotatedVector(path[j].x, initialVector);
        v = v * path[j].y;
        Debug.DrawRay(new Vector3(initialVector.x, 0f, initialVector.y), new Vector3(v.x, 0f, v.y));
        initialVector = initialVector + v;
      }
    }
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 3);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
  }

  public Vector2 GetResult()
  {
    return _winner;
  }

  private void SetWinner()
  {
    _winner = new Vector2(0, 0);
    float maxFitness = 0.0f;
    foreach (var individual in population.GetPopulation())
    {
      if (maxFitness < individual.fitness)
      {
        var v = UtilsGA.UtilsGA.CalculateRotatedVector(individual.path[0].x, _startPosition);
        v *= individual.path[0].y;
        _winner = v;
        maxFitness = individual.fitness;
      }
    }
  }


}

public class BasicGeneticAlgorithmBuilder : IGeneticAlgorithmBuilder<BasicIndividual>
{
  public BasicGeneticAlgorithmBuilder()
  {
    _ga = new BasicGeneticAlgorithm();
  }

  private BasicGeneticAlgorithm _ga { get; set; }

  public IGeneticAlgorithm<BasicIndividual> GetResult()
  {
    return _ga;
  }

  public void SetCrossover(IPopulationModifier<BasicIndividual> cross)
  {
    _ga.crossover = cross;
  }

  public void SetFitness(IPopulationModifier<BasicIndividual> fitness)
  {
    _ga.fitness = fitness;
  }

  public void SetMutation(IPopulationModifier<BasicIndividual> mutation)
  {
    _ga.mutation = mutation;
  }

  public void SetSelection(IPopulationModifier<BasicIndividual> selection)
  {
    _ga.selection = selection;
  }
}

//public class BasicGeneticAlgorithParallelBuilder : IGeneticAlgorithmBuilder<BasicIndividualStruct>
//{
//  public BasicGeneticAlgorithParallelBuilder()
//  {
//    _ga = new BasicGeneticAlgorithmParallel();
//  }

//  private BasicGeneticAlgorithmParallel _ga;

//  public IGeneticAlgorithm<BasicIndividualStruct> GetResult()
//  {
//    return _ga;
//  }

//  public void SetCrossover(IPopulationModifier<BasicIndividualStruct> cross)
//  { 
//    var _gaCopy = _ga;
//    _gaCopy.crossover = cross;
//    _ga = _gaCopy;
//  }

//  public void SetFitness(IPopulationModifier<BasicIndividualStruct> fitness)
//  {
//    var _gaCopy = _ga;
//    _gaCopy.fitness = fitness;
//    _ga = _gaCopy;
//  }

//  public void SetMutation(IPopulationModifier<BasicIndividualStruct> mutation)
//  {
//    var _gaCopy = _ga;
//    _gaCopy.mutation = mutation;
//    _ga = _gaCopy;
//  }

//  public void SetSelection(IPopulationModifier<BasicIndividualStruct> selection)
//  {
//    var _gaCopy = _ga;
//    _gaCopy.selection = selection;
//    _ga = _gaCopy;
//  }
//}


public class BasicCrossOperator : IPopulationModifier<BasicIndividual>
{
  System.Random _rand = new System.Random();

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    List<BasicIndividual> offsprings = new List<BasicIndividual>();
    var population = currentPopulation.GetPopulation();
    for (int i = 0; i < population.Length - 1; i += 2)
    {
      BasicIndividual off1 = new BasicIndividual();
      BasicIndividual off2 = new BasicIndividual();

      BasicIndividual[] parents = { population[i], population[i + 1] };

      for (int j = 0; j < parents[0].path.Count; j++)
      {
        int prob = (int)System.Math.Round(_rand.NextDouble(), System.MidpointRounding.AwayFromZero);
        off1.path.Add(parents[prob].path[j]);
        off2.path.Add(parents[1 - prob].path[j]);
      }

      offsprings.Add(off1);
      offsprings.Add(off2);
    }

    currentPopulation.SetPopulation(offsprings.ToArray());

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
  }
}


public class BasicMutationOperator : IPopulationModifier<BasicIndividual>
{
  System.Random _rand = new System.Random();
  float _agentSpeed { get; set; }
  float _timeDelta { get; set; }

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    var population = currentPopulation.GetPopulation();
    for (int i = 0; i < population.Length; i++)
    {
      for (int j = 0; j < population[i].path.Count; j++)
      {
        // Mutation with probability 0.2
        var mutProb = _rand.NextDouble();
        if (mutProb > 0.8f)
        {
          var size = UnityEngine.Random.Range(0f, _agentSpeed) * _timeDelta;
          float2 newVal = population[i].path[j];
          newVal.y = size;
          population[i].path[j] = newVal;
        }
      }
    }

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 2);

    _agentSpeed = (float)resources[0];
    _timeDelta = (float)resources[1];
  }
}

public class BasicFitnessFunction : IPopulationModifier<BasicIndividual>
{
  Vector2 _startPosition { get; set; }
  Vector2 _destination { get; set; }
  float _agentRadius { get; set; }
  int _agentIndex { get; set; }
  NativeQuadTree<TreeNode> _quadTree { get; set; }

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
    // Call Collides
    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
    // If doesnt collide, continue on next step.
    // At the end, check how far are we from destination
    var population = currentPopulation.GetPopulation();
    for (int i = 0; i < population.Length; i++)
    {
      var initialVector = _startPosition;
      var newPos = initialVector;
      var stepIndex = 1;
      foreach (var pos in population[i].path)
      {
        var rotatedVector = UtilsGA.UtilsGA.CalculateRotatedVector(pos.x, initialVector);
        rotatedVector *= pos.y;

        newPos = newPos + rotatedVector;

        AABB2D bounds = new AABB2D(newPos, new float2(_agentRadius * 1.5f, _agentRadius * 1.5f));
        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
        _quadTree.RangeQuery(bounds, queryRes);

        if (UtilsGA.UtilsGA.Collides(newPos, queryRes, stepIndex,_agentRadius,_agentIndex))
        {
          population[i].fitness = 0;
          break;
        }

        stepIndex++;
        initialVector = newPos;
      }

      // We broke cycle before finishing - this individual is colliding
      if (stepIndex - 1 < population[i].path.Count)
      {
        continue;
      }

      var diff = (_destination - newPos).magnitude;
      float fitness;
      if (diff < 0.001f)
      {
        fitness = 1;
      }
      else
      {
        fitness = 1 / (_destination - newPos).magnitude;
      }
      population[i].fitness = fitness;
    }

    currentPopulation.SetPopulation(population);
    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 5);

    _startPosition = (Vector2)resources[0];
    _destination = (Vector2)resources[1];
    _agentRadius = (float)resources[2];
    _agentIndex = (int)resources[3];
    _quadTree = (NativeQuadTree<TreeNode>)resources[4];
  }
}

public class BasicSelectionFunction : IPopulationModifier<BasicIndividual>
{
  System.Random _rand = new System.Random();

  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    List<BasicIndividual> selectedPop = new List<BasicIndividual>();
    var population = currentPopulation.GetPopulation();

    int multiplier = 10000;

    // Apply roulette selection
    double totalFitness = population.Sum(x => System.Math.Round(x.fitness * multiplier));

    if (totalFitness == 0)
    {
      return currentPopulation;
    }

    List<double> relativeFitnesses = population.Select(x => Mathf.Round(x.fitness * multiplier) / totalFitness).ToList();

    List<double> wheel = new List<double>();
    double prob = 0f;
    foreach (var fit in relativeFitnesses)
    {
      prob += fit;
      wheel.Add(prob);
    }

    for (int i = 0; i < population.Length; i++)
    {
      double val = _rand.NextDouble();
      int index = 0;
      foreach (var wheelVal in wheel)
      {
        if (val < wheelVal)
        {
          break;
        }
        index++;
      }

      // In case we are dealing with really small fitnesses ->
      // their sum might not give 1.0 and then theres chance that index will be equal to population size ->
      // clamp in to last one
      index = Mathf.Clamp(index, 0, population.Length - 1);

      selectedPop.Add(population[index]);
    }

    currentPopulation.SetPopulation(selectedPop.ToArray());
    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
  }
}