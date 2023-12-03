/// <summary>
/// Interface for population. Population consists of set of individuals
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IPopulation<T> where T : class
{
  /// <summary>
  /// Getter for population individuals
  /// </summary>
  /// <returns>Individuals array</returns>
  public T[] GetPopulation();
  /// <summary>
  /// Setter for population individuals
  /// </summary>
  /// <param name="population">New population to be set</param>
  public void SetPopulation(T[] population);
}

/// <summary>
/// Population modifier classes - cross, mutation, selection and fitness
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IPopulationModifier<T> where T : class
{
  /// <summary>
  /// Modifies population passed as parameter and returns result.
  /// </summary>
  /// <param name="currentPopulation">Population to be modified</param>
  /// <returns>New modified population</returns>
  public IPopulation<T> ModifyPopulation(IPopulation<T> currentPopulation);
}

/// <summary>
/// Builder for genetic algorithm. Sets concrete population modifiers.
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IGeneticAlgorithmBuilder<T> where T : class
{
  /// <summary>
  /// Setter for crossover operator
  /// </summary>
  /// <param name="cross">Crossover operator</param>
  public void SetCrossover(IPopulationModifier<T> cross);
  /// <summary>
  /// Setter for mutation operator
  /// </summary>
  /// <param name="mutation">Mutation operator</param>
  public void SetMutation(IPopulationModifier<T> mutation);
  /// <summary>
  /// Setter for fitness function
  /// </summary>
  /// <param name="fitness">Fitness function</param>
  public void SetFitness(IPopulationModifier<T> fitness);
  /// <summary>
  /// Setter for selection function
  /// </summary>
  /// <param name="selection">Selection function</param>
  public void SetSelection(IPopulationModifier<T> selection);
  /// <summary>
  /// Getter for created GA
  /// </summary>
  /// <returns>New GA created from specified operators and functions</returns>
  public IGeneticAlgorithm<T> GetResult();
}

/// <summary>
/// Interface for genetic algorithm.
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IGeneticAlgorithm<T> where T : class
{
  /// <summary>
  /// Crossover operator
  /// </summary>
  public IPopulationModifier<T> crossover { get; set; }
  /// <summary>
  /// Mutation operator
  /// </summary>
  public IPopulationModifier<T> mutation { get; set; }
  /// <summary>
  /// Fitness function
  /// </summary>
  public IPopulationModifier<T> fitness { get; set; }
  /// <summary>
  /// Selection function
  /// </summary>
  public IPopulationModifier<T> selection { get; set; }
  /// <summary>
  /// Number of iteration loops for this GA
  /// </summary>
  public int iterations { get; set; }
  /// <summary>
  /// Function to initialize population
  /// </summary>
  public void InitializePopulation();
  /// <summary>
  /// Population of this GA
  /// </summary>
  public IPopulation<T> population { get; set; }
  /// <summary>
  /// Set any required resources for GA to function (e.g. assign quadtree)
  /// </summary>
  public void SetResources();
  /// <summary>
  /// Perform GA
  /// </summary>
  public void Execute();
}