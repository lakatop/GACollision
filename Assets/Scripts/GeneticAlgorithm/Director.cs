using UnityEngine;

class GeneticAlgorithmDirector
{
  public GeneticAlgorithmDirector() { }

  public void MakeBasicGA(BasicGeneticAlgorithmBuilder builder, BaseAgent agent)
  {
    // Set crossover
    builder.SetCrossover(new BasicCrossOperator());

    // Set fitness
    var fitness = new BasicFitnessFunction();
    fitness.SetResources(new System.Collections.Generic.List<object>
    {
      agent.position,
      agent.destination,
      0.5f,
      agent.id,
      SimulationManager.Instance.GetQuadTree()
    });
    builder.SetFitness(fitness);

    // Set mutation
    var mutation = new BasicMutationOperator();
    mutation.SetResources(new System.Collections.Generic.List<object>
    {
      agent.speed,
      Time.deltaTime
    });
    builder.SetMutation(mutation);

    // Set selection
    builder.SetSelection(new BasicSelectionFunction());

    // Set population size and iterations
    var _gaAlg = builder.GetResult();
    _gaAlg.populationSize = 30;
    _gaAlg.iterations = 10;
    _gaAlg.SetResources(new System.Collections.Generic.List<object>
    {
      Time.deltaTime,
      agent.speed,
      agent.position
    });
  }
}