using System.Collections.Generic;
using Unity.Collections;

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

  public string GetComponentName()
  {
    return GetType().Name;
  }
}


public struct BasicCrossOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
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

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    offsprings.Dispose();
    parents.Dispose();
  }
}
