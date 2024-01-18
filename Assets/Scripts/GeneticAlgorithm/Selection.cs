using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

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

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void SetResources(List<object> resources)
  {
  }
}


public struct BasicSelectionFunctionParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  public NativeArray<double> relativeFitnesses;
  public NativeArray<double> wheel;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    // Apply roulette selection
    double totalFitness = 0;
    for (int i = 0; i < currentPopulation.Length; i++)
    {
      totalFitness += currentPopulation[i].fitness;
    }

    if (totalFitness == 0)
    {
      return;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      relativeFitnesses[i] = (currentPopulation[i].fitness) / totalFitness;
    }

    double prob = 0f;
    int index = 0;
    foreach (var fit in relativeFitnesses)
    {
      prob += fit;
      wheel[index] = prob;
      index++;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
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
      index = Mathf.Clamp(index, 0, currentPopulation.Length - 1);

      var newIndividual = currentPopulation[index];
      var outdatedIndividual = currentPopulation[i];
      outdatedIndividual.fitness = newIndividual.fitness;
      for (int j = 0; j < outdatedIndividual.path.Length; j++)
      {
        outdatedIndividual.path[j] = newIndividual.path[j];
      }
      currentPopulation[i] = outdatedIndividual;
    }

  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    relativeFitnesses.Dispose();
    wheel.Dispose();
  }
}


/// <summary>
/// Takes in consideration that fitness might be negative
/// Only takes first n best individuals
/// </summary>
public struct NegativeSelectionParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {

    currentPopulation.Sort(new BasicIndividualSortDescending());

    int n = 5;

    for (int i = n; i < currentPopulation.Length; i++)
    {
      var newIndividual = currentPopulation[i % n];
      var outdatedIndividual = currentPopulation[i];
      outdatedIndividual.fitness = newIndividual.fitness;
      for (int j = 0; j < outdatedIndividual.path.Length; j++)
      {
        outdatedIndividual.path[j] = newIndividual.path[j];
      }
      currentPopulation[i] = outdatedIndividual;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
  }
}
