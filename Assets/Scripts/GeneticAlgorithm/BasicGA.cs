//#define SHOW_DEBUG_LINES

//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.Collections;
//using NativeQuadTree;

//public class BasicGA
//{
//  class Individual
//  {
//    public Individual()
//    {
//      path = new List<Unity.Mathematics.float2>();
//    }
//    public List<Unity.Mathematics.float2> path;
//    public float fitness;
//  };
//  NativeQuadTree.NativeQuadTree<TreeNode> _quadTree { get; set; }
//  Vector2 startPosition { get; set; }
//  Vector2 destination { get; set; }
//  List<Individual> population { get; set; }
//  float timeDelta { get; set; }
//  float agentSpeed { get; set; }
//  System.Random rand { get; set; }
//  int agentIndex { get; set; }
//  float agentRadius { get; set; }
//  BaseAgent agent { get; set; }
//  int populationSize { get; set; }
//  float rotationRange { get; set; }

//  public BasicGA(NativeQuadTree.NativeQuadTree<TreeNode> quadTree,
//    float timeDelta,
//    float agentSpeed,
//    int agentIndex,
//    float agentRadius,
//    Vector2 startPos,
//    Vector2 destination,
//    BaseAgent agent)
//  {
//    _quadTree = quadTree;
//    this.timeDelta = timeDelta;
//    this.agentSpeed = agentSpeed;
//    population = new List<Individual>();
//    rand = new System.Random();
//    this.agentIndex = agentIndex;
//    this.agentRadius = agentRadius;
//    startPosition = startPos;
//    this.destination = destination;
//    this.agent = agent;

//    this.populationSize = 30;
//    this.rotationRange = 120f;

//  }

//  public void Execute(int loopIterations, out Vector2 winner)
//  {
//    InitializePopulation();

//    for (int i = 0; i < loopIterations; i++)
//    {
//      CalculateFitness();
//      ApplySelection();
//      ApplyOperators();
//    }

//    CalculateFitness();
//    SetWinner(out winner);
//  }

//  private void InitializePopulation()
//  {
//    for (int i = 0; i < populationSize; i++)
//    {
//      var individual = new Individual();
//      for (int j = 0; j < 10; j++)
//      {
//        var rotation = Random.Range(-rotationRange, rotationRange);
//        var size = Random.Range(0f, agentSpeed) * timeDelta;
//        individual.path.Add(new Unity.Mathematics.float2(rotation, size));
//      }
//      population.Add(individual);
//    }


//    for (int i = 0; i < population.Count; i++)
//    {
//      var initialVector = startPosition;
//      var path = population[i].path;
//      for (int j = 0; j < path.Count; j++)
//      {
//        var v = CalculateRotatedVector(path[j].x, initialVector);
//        v = v * path[j].y;
//        Debug.DrawRay(new Vector3(initialVector.x, 0, initialVector.y), new Vector3(v.x, 0, v.y));
//        initialVector = initialVector + v;
//      }
//    }
//  }

//  private void CalculateFitness()
//  {
//    // Create bounds from current position (stretch should be agentRadius or agentRadius * 2)
//    // Call Collides
//    // If collides, fitness must be 0 and continue to another individual (we certainly dont want to choose this individual)
//    // If doesnt collide, continue on next step.
//    // At the end, check how far are we from destination
//    for (int i = 0; i < population.Count; i++)
//    {
//      var initialVector = startPosition;
//      var newPos = initialVector;
//      var stepIndex = 1;
//      foreach(var pos in population[i].path)
//      {
//        var rotatedVector = CalculateRotatedVector(pos.x, initialVector);
//        rotatedVector *= pos.y;

//        newPos = newPos + rotatedVector;

//        NativeQuadTree.AABB2D bounds = new NativeQuadTree.AABB2D(newPos, new Unity.Mathematics.float2(agentRadius * 1.5f, agentRadius * 1.5f));
//        NativeList<QuadElement<TreeNode>> queryRes = new NativeList<QuadElement<TreeNode>>(100, Allocator.Temp);
//        _quadTree.RangeQuery(bounds, queryRes);

//        if (Collides(newPos, queryRes, stepIndex))
//        {
//          Individual temp = population[i];
//          temp.fitness = 0;
//          population[i] = temp;
//          break;
//        }

//        stepIndex++;
//        initialVector = newPos;
//      }

//      // We broke cycle before finishing - this individual is colliding
//      if(stepIndex - 1 < population[i].path.Count)
//      {
//        continue;
//      }

//      var diff = (destination - newPos).magnitude;
//      float fitness;
//      if (diff < 0.001f)
//      {
//        fitness = 1;
//      }
//      else
//      {
//        fitness = 1 / (destination - newPos).magnitude;
//      }
//      Individual temp2 = population[i];
//      temp2.fitness = fitness;
//      population[i] = temp2;
//    }
//  }

//  private void ApplySelection()
//  {
//    List<Individual> selectedPop = new List<Individual>();

//    int multiplier = 10000;

//    // Apply roulette selection
//    double totalFitness = population.Sum(x => System.Math.Round(x.fitness * multiplier));

//    if (totalFitness == 0)
//    {
//      return;
//    }

//    List<double> relativeFitnesses = population.Select(x => Mathf.Round(x.fitness * multiplier)/ totalFitness).ToList();

//    List<double> wheel = new List<double>();
//    double prob = 0f;
//    foreach(var fit in relativeFitnesses)
//    {
//      prob += fit;
//      wheel.Add(prob);
//    }

//    for(int i = 0; i < populationSize; i++)
//    {
//      double val = rand.NextDouble();
//      int index = 0;
//      foreach (var wheelVal in wheel)
//      {
//        if (val < wheelVal)
//        {
//          break;
//        }
//        index++;
//      }

//      Debug.Log("INDEX " + index.ToString());

//      // In case we are dealing with really small fitnesses ->
//      // their sum might not give 1.0 and then theres chance that index will be equal to population size ->
//      // clamp in to last one
//      index = Mathf.Clamp(index, 0, population.Count - 1);

//      selectedPop.Add(population[index]);
//    }

//    population = selectedPop;
//  }

//  private void ApplyOperators()
//  {
//    List<Individual> offsprings = new List<Individual>();
//    for (int i = 0; i < population.Count - 1; i += 2)
//    {
//      Individual off1 = new Individual();
//      Individual off2 = new Individual();

//      Individual[] parents = { population[i], population[i + 1]};

//      for (int j = 0; j < parents[0].path.Count; j++)
//      {
//        int prob = (int)System.Math.Round(rand.NextDouble(), System.MidpointRounding.AwayFromZero);
//        off1.path.Add(parents[prob].path[j]);
//        off2.path.Add(parents[1 - prob].path[j]);

//        // Mutation with probability 0.2
//        var mutProb = rand.NextDouble();
//        if(mutProb > 0.8f)
//        {
//          var size = Random.Range(0f, agentSpeed) * timeDelta;
//          var off1V = off1.path[off1.path.Count - 1];
//          var off2V = off2.path[off2.path.Count - 1];

//          off1V.y = size;
//          off2V.y = size;

//          off1.path[off1.path.Count - 1] = off1V;
//          off2.path[off2.path.Count - 1] = off2V;
//        }

//      }

//      offsprings.Add(off1);
//      offsprings.Add(off2);
//    }

//    population = offsprings;
//  }

//  /// <summary>
//  /// Set return velocity.
//  /// </summary>
//  private void SetWinner(out Vector2 winner)
//  {
//    winner = new Vector2(0,0);
//    float maxFitness = 0.0f;
//    foreach (var individual in population)
//    {
//      if (maxFitness < individual.fitness)
//      {
//        var v = CalculateRotatedVector(individual.path[0].x, startPosition);
//        v *= individual.path[0].y;
//        winner = v;
//        maxFitness = individual.fitness;
//      }
//    }
//  }

//  /// <summary>
//  /// Calculate normalized vector with degrees rotation compared to initialVector
//  /// </summary>
//  /// <param name="degrees">Rotation of new vector - counter clockwise</param>
//  /// <param name="initialVector">Vector to which we perform rotation. New vector ahs origin where initialVector ends</param>
//  /// <returns>Rotated vector</returns>
//  Vector2 CalculateRotatedVector(float degrees, Vector2 initialVector)
//  {
//    // Convert degrees to radians for Mathf functions
//    float radians = Mathf.Deg2Rad * degrees;


//    // Calculate the components of the new vector using trigonometry
//    float xComponent = Mathf.Cos(radians) * initialVector.x - Mathf.Sin(radians) * initialVector.y;
//    float yComponent = Mathf.Sin(radians) * initialVector.x + Mathf.Cos(radians) * initialVector.y;

//    // Create the new vector
//    Vector2 generatedVector = new Vector2(xComponent, yComponent).normalized;

//    return generatedVector;

//  }

//  /// <summary>
//  /// Check whether my position collides with results returned by quadtree
//  /// </summary>
//  /// <param name="pos">Agents current position</param>
//  /// <param name="queryRes">Result from query performed on quadtree</param>
//  /// <param name="stepIndex">Current step index in simulation</param>
//  /// <returns></returns>
//  bool Collides(Vector2 pos, NativeList<NativeQuadTree.QuadElement<TreeNode>> queryRes, int stepIndex)
//  {
//    bool collides = false;

//    foreach(var element in queryRes)
//    {
//      if ((element.element.agentIndex == agentIndex)
//          || (!element.element.staticObstacle && element.element.stepIndex != stepIndex))
//      {
//        continue;
//      }

//      Vector2 v = new Vector2(element.pos.x - pos.x, element.pos.y - pos.y);
//      if (v.magnitude < (agentRadius * 2))
//      {
//        collides = true;
//        break;
//      }
//    }

//    return collides;
//  }
//}