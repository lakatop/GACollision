using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Unity.Collections;
using System.Reflection;
using static Unity.VisualScripting.LudiqRootObjectEditor;

public class BasicIndividual
{
  public BasicIndividual()
  {
    fitness = 0f;
    path = new List<float2>();
  }

  public float fitness { get; set; }
  public List<float2> path { get; set; }
}

public class BasicPopulation : IPopulation<BasicIndividual>
{
  public BasicPopulation()
  {
    _population = new List<BasicIndividual>();
  }

  public BasicIndividual[] GetPopulation()
  {
    return _population.ToArray();
  }

  public void SetPopulation(BasicIndividual[] population)
  {
    _population = new List<BasicIndividual>(population);
  }

  private List<BasicIndividual> _population { get; set; }
}
 

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
  private int _agentIndex { get; set; }
  private float _agentRadius { get; set; }

  public void Execute()
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
        var v = CalculateRotatedVector(path[j].x, initialVector);
        v = v * path[j].y;
        Debug.DrawRay(new Vector3(initialVector.x, 0, initialVector.y), new Vector3(v.x, 0, v.y));
        initialVector = initialVector + v;
      }
    }
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 5);

    _timeDelta = (float)resources[0];
    _agentSpeed = (float)resources[1];
    _startPosition = (Vector2)resources[2];
    _agentIndex = (int)resources[3];
    _agentRadius = (float)resources[4];

    crossover.SetResources(new List<object> { _agentSpeed, _timeDelta });
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
        var v = CalculateRotatedVector(individual.path[0].x, _startPosition);
        v *= individual.path[0].y;
        _winner = v;
        maxFitness = individual.fitness;
      }
    }
  }

  /// <summary>
  /// Calculate normalized vector with degrees rotation compared to initialVector
  /// </summary>
  /// <param name="degrees">Rotation of new vector - counter clockwise</param>
  /// <param name="initialVector">Vector to which we perform rotation. New vector ahs origin where initialVector ends</param>
  /// <returns>Rotated vector</returns>
  Vector2 CalculateRotatedVector(float degrees, Vector2 initialVector)
  {
    // Convert degrees to radians for Mathf functions
    float radians = Mathf.Deg2Rad * degrees;


    // Calculate the components of the new vector using trigonometry
    float xComponent = Mathf.Cos(radians) * initialVector.x - Mathf.Sin(radians) * initialVector.y;
    float yComponent = Mathf.Sin(radians) * initialVector.x + Mathf.Cos(radians) * initialVector.y;

    // Create the new vector
    Vector2 generatedVector = new Vector2(xComponent, yComponent).normalized;

    return generatedVector;

  }

  /// <summary>
  /// Check whether my position collides with results returned by quadtree
  /// </summary>
  /// <param name="pos">Agents current position</param>
  /// <param name="queryRes">Result from query performed on quadtree</param>
  /// <param name="stepIndex">Current step index in simulation</param>
  /// <returns></returns>
  bool Collides(Vector2 pos, NativeList<NativeQuadTree.QuadElement<TreeNode>> queryRes, int stepIndex)
  {
    bool collides = false;

    foreach (var element in queryRes)
    {
      if ((element.element.agentIndex == _agentIndex)
          || (!element.element.staticObstacle && element.element.stepIndex != stepIndex))
      {
        continue;
      }

      Vector2 v = new Vector2(element.pos.x - pos.x, element.pos.y - pos.y);
      if (v.magnitude < (_agentRadius * 2))
      {
        collides = true;
        break;
      }
    }

    return collides;
  }
}

public class BasicGeneticAlgorithmBuilder : IGeneticAlgorithmBuilder<BasicIndividual>
{
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

public class BasicCrossOperator : IPopulationModifier<BasicIndividual>
{
  System.Random _rand = new System.Random();
  float _agentSpeed { get; set; }
  float _timeDelta { get; set; }

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

        // Mutation with probability 0.2
        var mutProb = _rand.NextDouble();
        if (mutProb > 0.8f)
        {
          var size = UnityEngine.Random.Range(0f, _agentSpeed) * _timeDelta;
          var off1V = off1.path[off1.path.Count - 1];
          var off2V = off2.path[off2.path.Count - 1];

          off1V.y = size;
          off2V.y = size;

          off1.path[off1.path.Count - 1] = off1V;
          off2.path[off2.path.Count - 1] = off2V;
        }

      }

      offsprings.Add(off1);
      offsprings.Add(off2);
    }

    currentPopulation.SetPopulation(offsprings.ToArray());

    return currentPopulation;
  }

  public void SetResources(List<object> resources)
  {
    Assert.IsTrue(resources.Count == 2);

    _agentSpeed = (float)resources[0];
    _timeDelta = (float)resources[1];
  }
}

public class BasicMutationOperator : IPopulationModifier<BasicIndividual>
{
  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    return new BasicPopulation();
  }

  public void SetResources(List<object> resources)
  {
    throw new System.NotImplementedException();
  }
}

public class BasicFitnessFunction : IPopulationModifier<BasicIndividual>
{
  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    return new BasicPopulation();
  }

  public void SetResources(List<object> resources)
  {
    throw new System.NotImplementedException();
  }
}

public class BasicSelectionFunction : IPopulationModifier<BasicIndividual>
{
  public IPopulation<BasicIndividual> ModifyPopulation(IPopulation<BasicIndividual> currentPopulation)
  {
    return new BasicPopulation();
  }

  public void SetResources(List<object> resources)
  {
    throw new System.NotImplementedException();
  }
}