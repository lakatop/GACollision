using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using NativeQuadTree;
using System.Linq;
using Unity.Collections;

/// TODO: Pravdepodobne budem musiet modifikovat alg tak aby mal vsetko len structy
/// Vyvtorit NativeBasicPopulation kde to nebude List ale NativeList
/// Z execute staci len zavolat - predtym ale treba nainicializovat vsetky operatory
[BurstCompile]
public struct BasicGeneticAlgorithmParallel : IJob
{
  public BasicCrossOperatorParallel cross;
  public BasicMutationOperatorParallel mut;
  public BasicFitnessFunctionParallel fit;
  public BasicSelectionFunctionParallel sel;
  public NativeArray<BasicIndividualStruct> pop;

  public int iterations;
  public int populationSize;

  public NativeArray<Vector2> _winner;
  public float _timeDelta;
  public float _agentSpeed;
  public Vector2 _startPosition;

  public Unity.Mathematics.Random _rand;

  public void Execute()
  {
    RunGA();
  }

  public void RunGA()
  {
    InitializePopulation();

    for (int i = 0; i < iterations; i++)
    {
      pop = fit.ModifyPopulation(pop);
      pop = sel.ModifyPopulation(pop);
      pop = cross.ModifyPopulation(pop);
      pop = mut.ModifyPopulation(pop);
    }

    pop = fit.ModifyPopulation(pop);
    SetWinner();

    Dispose();
  }

  public void InitializePopulation()
  {
    float rotationRange = 120f;

    for (int i = 0; i < populationSize; i++)
    {
      var individual = new BasicIndividualStruct();
      individual.Initialize(10, Allocator.TempJob);
      for (int j = 0; j < 10; j++)
      {
        var rotation = _rand.NextFloat(-rotationRange, rotationRange + 0.001f);
        var size = _rand.NextFloat(_agentSpeed + 0.001f) * _timeDelta;
        individual.path.Add(new float2(rotation, size));
      }
      pop[i] = individual;
    }


    for (int i = 0; i < pop.Length; i++)
    {
      var initialVector = _startPosition;
      var path = pop[i].path;
      for (int j = 0; j < path.Length; j++)
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
    return _winner[0];
  }

  private void SetWinner()
  {
    _winner[0] = new Vector2(0, 0);
    float maxFitness = 0.0f;
    foreach (var individual in pop)
    {
      if (maxFitness < individual.fitness)
      {
        var v = UtilsGA.UtilsGA.CalculateRotatedVector(individual.path[0].x, _startPosition);
        v *= individual.path[0].y;
        _winner[0] = new Vector2(v.x, v.y);
        maxFitness = individual.fitness;
      }
    }
  }

  public void Dispose()
  {
    foreach (var ind in pop)
    {
      ind.Dispose();
    }
  }
}



public struct BasicCrossOperatorParallel
{
  public Unity.Mathematics.Random _rand;
  public NativeArray<BasicIndividualStruct> offsprings;
  public NativeArray<BasicIndividualStruct> parents;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    var population = currentPopulation;
    int index = 0;
    for (int i = 0; i < population.Length - 1; i += 2)
    {
      BasicIndividualStruct off1 = new BasicIndividualStruct();
      off1.Initialize(10, Allocator.TempJob);
      BasicIndividualStruct off2 = new BasicIndividualStruct();
      off2.Initialize(10, Allocator.TempJob);

      parents[0] = population[i];
      parents[1] = population[i + 1];

      for (int j = 0; j < parents[0].path.Length; j++)
      {
        int prob = (int)System.Math.Round(_rand.NextFloat(), System.MidpointRounding.AwayFromZero);
        off1.path.Add(parents[prob].path[j]);
        off2.path.Add(parents[1 - prob].path[j]);
      }

      offsprings[index] = off1;
      offsprings[index + 1] = off2;
      index += 2;
    }

    for (int i = 0; i < offsprings.Length; i++)
    {
      currentPopulation[i] = offsprings[i];
    }

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
  }
}

public struct BasicMutationOperatorParallel
{
  public Unity.Mathematics.Random _rand;
  public float _agentSpeed;
  public float _timeDelta;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      for (int j = 0; j < currentPopulation[i].path.Length; j++)
      {
        // Mutation with probability 0.2
        var mutProb = _rand.NextFloat();
        if (mutProb > 0.8f)
        {
          var size = _rand.NextFloat(_agentSpeed + 0.001f) * _timeDelta;
          float2 newVal = currentPopulation[i].path[j];
          newVal.y = size;
          var tempPop = currentPopulation;
          var tempPath = tempPop[i].path;
          tempPath[j] = newVal;
          currentPopulation = tempPop;
        }
      }
    }

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
  }
}

public struct BasicFitnessFunctionParallel
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  [ReadOnly] public NativeQuadTree<TreeNode> _quadTree;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
    // Call Collides
    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
    // If doesnt collide, continue on next step.
    // At the end, check how far are we from destination
    var population = currentPopulation;
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

        if (UtilsGA.UtilsGA.Collides(newPos, queryRes, stepIndex, _agentRadius, _agentIndex))
        {
          var temp = population[i];
          temp.fitness = 0;
          population[i] = temp;
          break;
        }

        stepIndex++;
        initialVector = newPos;
      }

      // We broke cycle before finishing - this individual is colliding
      if (stepIndex - 1 < population[i].path.Length)
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
      var temp2 = population[i];
      temp2.fitness = fitness;
      population[i] = temp2;
    }

    for (int i = 0; i < population.Length; i++)
    {
      currentPopulation[i] = population[i];
    }

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
  }
}

public struct BasicSelectionFunctionParallel
{
  public Unity.Mathematics.Random _rand;
  public NativeArray<BasicIndividualStruct> selectedPop;
  public NativeArray<double> relativeFitnesses;
  public NativeArray<double> wheel;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    var population = currentPopulation;

    int multiplier = 10000;

    // Apply roulette selection
    double totalFitness = 0;
    for (int i = 0; i < population.Length; i++)
    {
      totalFitness += population[i].fitness * multiplier;
    }

    if (totalFitness == 0)
    {
      return currentPopulation;
    }

    for (int i = 0; i < population.Length; i++)
    {
      relativeFitnesses[i] = (population[i].fitness * multiplier) / totalFitness;
    }

    double prob = 0f;
    int index = 0;
    foreach (var fit in relativeFitnesses)
    {
      prob += fit;
      wheel[index] = prob;
      index++;
    }

    for (int i = 0; i < population.Length; i++)
    {
      double val = _rand.NextFloat();
      index = 0;
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

      selectedPop[i] = population[index];
    }

    return selectedPop;
  }

  public void SetResources(List<object> resources)
  {
  }
}
