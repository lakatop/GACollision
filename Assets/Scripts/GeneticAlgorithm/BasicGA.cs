using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class BasicGA
{
  struct Individual
  {
    public List<Unity.Mathematics.float2> path;
    public float fitness;
  };
  NativeQuadTree.NativeQuadTree<TreeNode> _quadTree { get; set; }
  Vector2 positionVector2 { get; set; }
  Vector2 destination { get; set; }
  Vector2 winner { get; set; }
  List<Individual> population { get; set; }
  float timeDelta { get; set; }
  float agentSpeed { get; set; }
  System.Random rand { get; set; }
  int agentIndex { get; set; }
  float agentRadius { get; set; }

  public BasicGA(NativeQuadTree.NativeQuadTree<TreeNode> quadTree,
    ref Vector2 winner,
    float timeDelta,
    float agentSpeed,
    int agentIndex,
    float agentRadius)
  {
    _quadTree = quadTree;
    this.winner = winner;
    this.timeDelta = timeDelta;
    this.agentSpeed = agentSpeed;
    population = new List<Individual>();
    rand = new System.Random();
    this.agentIndex = agentIndex;
    this.agentRadius = agentRadius;
  }

  public void Execute(int loopIterations)
  {
    InitializePopulation();

    for (int i = 0; i < loopIterations; i++)
    {
      CalculateFitness();
      ApplySelection();
      ApplyOperators();
    }

    SetWinner();
  }

  private void InitializePopulation()
  {
    for (int i = 0; i < 10; i++)
    {
      var individual = new Individual();
      for (int j = 0; j < 10; j++)
      {
        var rotation = Random.Range(-30f, 30f);
        var size = Random.Range(0f, agentSpeed) * timeDelta;
        individual.path.Add(new Unity.Mathematics.float2(rotation, size));
      }
      population.Add(individual);
    }
  }

  private void CalculateFitness()
  {
    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
    // Call Collides
    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
    // If doesnt collide, continue on next step.
    // At the end, check how far are we from destination
  }

  private void ApplySelection()
  {
    List<Individual> selectedPop = new List<Individual>();

    // Apply roulette selection
    double totalFitness = population.Sum(x => x.fitness);

    List<double> relativeFitnesses = population.Select(x => x.fitness / totalFitness).ToList();

    List<double> wheel = new List<double>();
    double prob = 0f;
    foreach(var fit in relativeFitnesses)
    {
      prob += fit;
      wheel.Add(prob);
    }

    for(int i = 0; i < 10; i++)
    {
      double val = rand.NextDouble();
      int index = 0;
      foreach (var wheelVal in wheel)
      {
        if (val < wheelVal)
        {
          break;
        }
        index++;
      }

      selectedPop.Add(population[index]);
    }

    population = selectedPop;
  }

  private void ApplyOperators()
  {
    List<Individual> offsprings = new List<Individual>();
    for (int i = 0; i < population.Count - 1; i += 2)
    {
      Individual off1 = new Individual();
      Individual off2 = new Individual();

      Individual[] parents = { population[i], population[i + 1]};

      for (int j = 0; j < parents[0].path.Count; j++)
      {
        int prob = (int)System.Math.Round(rand.NextDouble(), System.MidpointRounding.AwayFromZero);
        off1.path.Add(parents[prob].path[j]);
        off2.path.Add(parents[1 - prob].path[j]);

        // Mutation with probability 0.2
        var mutProb = rand.NextDouble();
        if(mutProb > 0.8f)
        {
          var size = Random.Range(0f, agentSpeed) * timeDelta;
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

    population = offsprings;
  }

  /// <summary>
  /// Set return velocity.
  /// </summary>
  private void SetWinner()
  {

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

    foreach(var element in queryRes)
    {
      if ((element.element.agentIndex == agentIndex)
          || (!element.element.staticObstacle && element.element.stepIndex != stepIndex))
      {
        continue;
      }

      Vector2 v = new Vector2(element.pos.x - pos.x, element.pos.y - pos.y);
      if (v.magnitude < (agentRadius * 2))
      {
        collides = true;
        break;
      }
    }

    return collides;
  }
}