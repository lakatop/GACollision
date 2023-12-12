public class BasicGeneticAlgorithmBuilder : IGeneticAlgorithmBuilder<BasicIndividual>
{
  public BasicGeneticAlgorithmBuilder()
  {
    _ga = new BasicGeneticAlgorithm();
  }

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