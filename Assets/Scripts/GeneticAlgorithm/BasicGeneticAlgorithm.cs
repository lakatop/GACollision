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








