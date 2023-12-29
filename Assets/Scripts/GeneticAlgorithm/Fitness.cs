using UnityEngine;
using NativeQuadTree;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
using static UnityEditor.PlayerSettings;


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
      var newPos = _startPosition;
      var rotationVector = newPos.normalized;

      var stepIndex = 1;
      foreach (var pos in population[i].path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, newPos);
        rotatedAndTranslatedVector = rotatedAndTranslatedVector * pos.y;

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        AABB2D bounds = new AABB2D(newPos, new float2(_agentRadius * 1.5f, _agentRadius * 1.5f));
        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
        _quadTree.RangeQuery(bounds, queryRes);

        if (UtilsGA.UtilsGA.Collides(newPos, queryRes, stepIndex, _agentRadius, _agentIndex))
        {
          population[i].fitness = 0;
          break;
        }

        stepIndex++;
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

  public string GetComponentName()
  {
    return GetType().Name;
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


public struct BasicFitnessFunctionParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;

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
      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;
      foreach (var pos in population[i].path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = rotatedVector * pos.y;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

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
        fitness = 1000;
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

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose() { }
}


public struct FitnessContinuousDistanceParallel : IParallelPopulationModifier<BasicIndividualStruct>
{
  public Vector2 _startPosition;
  public Vector2 _destination;
  public float _agentRadius;
  public int _agentIndex;
  public NativeQuadTree<TreeNode> _quadTree;
  public Vector2 _forward;

  public NativeArray<BasicIndividualStruct> ModifyPopulation(NativeArray<BasicIndividualStruct> currentPopulation)
  {
    NativeArray<float> fitnesses = new NativeArray<float>(currentPopulation.Length, Allocator.Temp);

    var index = 0;
    foreach (var individual in currentPopulation)
    {
      var fitness = Mathf.Pow((_destination - _startPosition).magnitude, 2);

      var newPos = _startPosition;
      var rotationVector = _forward.normalized;

      var stepIndex = 1;

      foreach (var pos in individual.path)
      {
        var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, pos.x);
        var rotatedAndTranslatedVector = rotatedVector * pos.y;
        rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedAndTranslatedVector, newPos);

        newPos = rotatedAndTranslatedVector;
        rotationVector = rotatedVector;

        AABB2D bounds = new AABB2D(newPos, new float2(_agentRadius * 1.5f, _agentRadius * 1.5f));
        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
        _quadTree.RangeQuery(bounds, queryRes);

        if (UtilsGA.UtilsGA.Collides(newPos, queryRes, stepIndex, _agentRadius, _agentIndex))
        {
          fitness = fitness - (Mathf.Pow((_destination - newPos).magnitude, 2) * 2);
        }
        else
        {
          fitness -= Mathf.Pow((_destination - newPos).magnitude, 2);
        }

        stepIndex++;
      }

      fitnesses[index] = fitness;
      index++;
    }

    for (int i = 0; i < currentPopulation.Length; i++)
    {
      var temp = currentPopulation[i];
      temp.fitness = fitnesses[i];
      currentPopulation[i] = temp;
    }

    return currentPopulation;
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public void Dispose() { }
}
