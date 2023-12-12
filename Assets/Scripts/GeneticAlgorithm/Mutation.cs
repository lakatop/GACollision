using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;

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

public struct BasicMutationOperatorParallel : IParallelPopulationModifier<BasicIndividualStruct>
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

  public void Dispose()
  {
  }
}
