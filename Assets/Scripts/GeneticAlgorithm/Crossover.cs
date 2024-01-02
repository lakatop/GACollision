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
  [ReadOnly] public int pathSize;
  public NativeArray<BasicIndividualStruct> offsprings;
  public NativeArray<BasicIndividualStruct> parents;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    var population = currentPopulation;
    int index = 0;
    for (int i = 0; i < population.Length - 1; i += 2)
    {
      BasicIndividualStruct off1 = new BasicIndividualStruct();
      off1.Initialize(pathSize, Allocator.TempJob);
      BasicIndividualStruct off2 = new BasicIndividualStruct();
      off2.Initialize(pathSize, Allocator.TempJob);

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


public struct MeanCrossOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random _rand;
  public NativeArray<BasicIndividualStruct> offsprings;
  public NativeArray<BasicIndividualStruct> parents;
  public int pathSize;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    var population = currentPopulation;
    int index = 0;
    for (int i = 0; i < population.Length - 1; i += 2)
    {
      BasicIndividualStruct off1 = new BasicIndividualStruct();
      off1.Initialize(pathSize, Allocator.TempJob);
      BasicIndividualStruct off2 = new BasicIndividualStruct();
      off2.Initialize(pathSize, Allocator.TempJob);

      parents[0] = population[i];
      parents[1] = population[i + 1];

      // Calculate mean offspring from 2 parents
      for (int j = 0; j < parents[0].path.Length; j++)
      {
        var tempSegment = parents[0].path[j];
        tempSegment.x += parents[1].path[j].x;
        tempSegment.x /= 2;
        tempSegment.y += parents[1].path[j].y;
        tempSegment.y /= 2;
        off1.path.Add(tempSegment);
      }

      // Randomly select 1 parent from pair and 1 random parent from parents population and do the same
      var parentIndex = _rand.NextInt(2);
      var secondParentIndex = _rand.NextInt(population.Length);

      parents[0] = population[parentIndex];
      parents[1] = population[secondParentIndex];

      for (int j = 0; j < parents[0].path.Length; j++)
      {
        var tempSegment = parents[0].path[j];
        tempSegment.x += parents[1].path[j].x;
        tempSegment.x /= 2;
        tempSegment.y += parents[1].path[j].y;
        tempSegment.y /= 2;
        off2.path.Add(tempSegment);
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
