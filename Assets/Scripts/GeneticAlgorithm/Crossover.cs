using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;

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


[BurstCompile]
public struct UniformBezierCrossOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random rand;
  public NativeArray<BezierIndividualStruct> parents;
  [ReadOnly] public float crossProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length - 1; i += 2)
    {
      var crossProb = rand.NextFloat();
      // Do cross only with small probability
      if (crossProb > this.crossProb)
        return;

      var parent1 = currentPopulation[i];
      var nextParentIndex = rand.NextInt(currentPopulation.Length);
      while (nextParentIndex == i)
      {
        nextParentIndex = rand.NextInt(currentPopulation.Length);
      }

      parents[0] = currentPopulation[i];
      parents[1] = currentPopulation[nextParentIndex];

      UnityEngine.Vector2 P1 = UnityEngine.Vector2.zero;
      UnityEngine.Vector2 P2 = UnityEngine.Vector2.zero;



      int prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P1.x = parents[prob].bezierCurve.points[1].x;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P1.y = parents[prob].bezierCurve.points[1].y;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P2.x = parents[prob].bezierCurve.points[2].x;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P2.y = parents[prob].bezierCurve.points[2].y;

      for (int j = 0; j < parent1.accelerations.Length; j++)
      {
        prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
        parent1.accelerations[j] = parents[prob].accelerations[j];
      }

      parent1.bezierCurve.points[1] = P1;
      parent1.bezierCurve.points[2] = P2;
      currentPopulation[i] = parent1;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetCrossProbability()
  {
    return crossProb;
  }

  public void Dispose()
  {
    parents.Dispose();
  }
}


/// -------------------- Invalidated because of different individual representation --------------------


[BurstCompile]
public struct BasicCrossOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random rand;
  [ReadOnly] public int pathSize;
  public NativeArray<BasicIndividualStruct> offsprings;
  public NativeArray<BasicIndividualStruct> parents;
  public int iterations;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    int index = 0;
    for (int i = 0; i < currentPopulation.Length - 1; i += 2)
    {
      BasicIndividualStruct off1 = offsprings[index];
      BasicIndividualStruct off2 = offsprings[index + 1];

      parents[0] = currentPopulation[i];
      parents[1] = currentPopulation[i + 1];

      for (int j = 0; j < parents[0].path.Length; j++)
      {
        int prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
        off1.path[j] = parents[prob].path[j];
        off2.path[j] = parents[1 - prob].path[j];
      }

      offsprings[index] = off1;
      offsprings[index + 1] = off2;
      index += 2;
    }

    for (int i = 0; i < offsprings.Length; i++)
    {
      if (iteration < iterations)
      {
        currentPopulation[i].Dispose();
      }
      currentPopulation[i] = offsprings[i];
    }
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


[BurstCompile]
public struct MeanCrossOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random rand;
  public NativeArray<BasicIndividualStruct> offsprings;
  public int pathSize;
  public int iterations;

  public void ModifyPopulation(ref NativeArray<BasicIndividualStruct> currentPopulation, int iteration)
  {
    int index = 0;
    for (int i = 0; i < currentPopulation.Length - 1; i += 2)
    {
      BasicIndividualStruct off1 = offsprings[index];
      BasicIndividualStruct off2 = offsprings[index + 1];

      var parent1 = currentPopulation[i];
      var parent2 = currentPopulation[i + 1];

      // Calculate mean offspring from 2 parents
      for (int j = 0; j < parent1.path.Length; j++)
      {
        var tempSegment = parent1.path[j];
        tempSegment.x += parent2.path[j].x;
        tempSegment.x /= 2;
        tempSegment.y += parent2.path[j].y;
        tempSegment.y /= 2;
        off1.path[j] = tempSegment;
      }

      // Randomly select 1 parent from pair and 1 random parent from parents population and do the same
      var parentIndex = rand.NextInt(2);
      var secondParentIndex = rand.NextInt(currentPopulation.Length);

      parent1 = currentPopulation[parentIndex];
      parent2 = currentPopulation[secondParentIndex];

      for (int j = 0; j < parent1.path.Length; j++)
      {
        var tempSegment = parent1.path[j];
        tempSegment.x += parent2.path[j].x;
        tempSegment.x /= 2;
        tempSegment.y += parent2.path[j].y;
        tempSegment.y /= 2;
        off2.path[j] = tempSegment;
      }

      offsprings[index] = off1;
      offsprings[index + 1] = off2;

      index += 2;
    }

    for (int i = 0; i < offsprings.Length; i++)
    {
      // We are replacing elements in population array to point to elements in offsprings array
      // We need to dealocate individuals from currentPopulation
      if (iteration == 0)
        currentPopulation[i].Dispose();

      currentPopulation[i] = offsprings[i];
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose()
  {
    offsprings.Dispose();
  }
}
