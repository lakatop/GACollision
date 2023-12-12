using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Burst;
using NativeQuadTree;
using System.Linq;
using Unity.Collections;



[BurstCompile]
public struct BasicGeneticAlgorithmParallel : IJob, IGeneticAlgorithmParallel<BasicIndividualStruct>
{
  public BasicCrossOperatorParallel cross;
  public BasicMutationOperatorParallel mutation;
  public BasicFitnessFunctionParallel fitness;
  public BasicSelectionFunctionParallel selection;
  public NativeBasicPopulation pop;


  public int iterations { get; set; }
  public int populationSize { get; set; }

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
      pop.SetPopulation(fitness.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(selection.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(cross.ModifyPopulation(pop.GetPopulation()));
      pop.SetPopulation(mutation.ModifyPopulation(pop.GetPopulation()));
    }

    pop.SetPopulation(fitness.ModifyPopulation(pop.GetPopulation()));
    SetWinner();
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
      pop.SetIndividual(individual, i);
    }


    for (int i = 0; i < pop.GetPopulation().Length; i++)
    {
      var initialVector = _startPosition;
      var path = pop.GetPopulation()[i].path;
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
    foreach (var individual in pop.GetPopulation())
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
    cross.Dispose();
    mutation.Dispose();
    fitness.Dispose();
    selection.Dispose();
    foreach (var ind in pop.GetPopulation())
    {
      ind.Dispose();
    }
  }
}


