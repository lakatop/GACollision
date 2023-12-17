using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Assertions;




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
      var placeOrigin = _startPosition;
      var rotationVector = placeOrigin.normalized;
      var path = pop[i].path;

      for (int j = 0; j < path.Count; j++)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, path[j].x);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, placeOrigin);
        rotatedAndTranslatedVector = rotatedAndTranslatedVector * path[j].y;
        Debug.DrawRay(new Vector3(placeOrigin.x, 0f, placeOrigin.y), new Vector3(rotatedAndTranslatedVector.x, 0f, rotatedAndTranslatedVector.y));
        placeOrigin = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;
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
        var v = UtilsGA.UtilsGA.RotateVector(_startPosition.normalized, individual.path[0].x);
        v *= individual.path[0].y;
        //v = UtilsGA.UtilsGA.MoveToOrigin(v, _startPosition);
        _winner = new Vector2(v.x, v.y);
        maxFitness = individual.fitness;
      }
    }
  }


}
