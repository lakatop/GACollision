using Unity.Collections;

/// <summary>
/// Interface for population. Population consists of set of individuals
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IPopulation<T>
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
/// Interface for population used in parallel GA. Population consists of set of individuals
/// </summary>
/// <typeparam name="T">Concrete individual struct</typeparam>
public interface IParallelPopulation<T> where T : struct
{
  /// <summary>
  /// Getter for population individuals
  /// </summary>
  /// <returns>Individuals NativeArray</returns>
  public NativeArray<T> GetPopulation();
  /// <summary>
  /// Setter for population individuals
  /// </summary>
  /// <param name="population">New population to be set</param>
  public void SetPopulation(NativeArray<T> population);
  /// <summary>
  /// Setter for concrete individual
  /// </summary>
  /// <param name="individual">New individual that will be set</param>
  /// <param name="index">Index at which new individual will be placed in population</param>
  public void SetIndividual(T individual, int index);
  /// <summary>
  /// Used to dispose individuals in population
  /// </summary>
  public void Dispose();
}

/// <summary>
/// Population modifier classes - cross, mutation, selection and fitness
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IPopulationModifier<T>
{
  /// <summary>
  /// Modifies population passed as parameter and returns result.
  /// </summary>
  /// <param name="currentPopulation">Population to be modified</param>
  /// <returns>New modified population</returns>
  public IPopulation<T> ModifyPopulation(IPopulation<T> currentPopulation);
  /// <summary>
  /// Set any required resources for GA to function (e.g. assign quadtree)
  /// </summary>
  /// <param name="resources">List of resources</param>
  public void SetResources(System.Collections.Generic.List<object> resources);
  /// <summary>
  /// Get name of this component - used for logging purposes in various experiments
  /// </summary>
  /// <returns>Struct declaration name of this component</returns>
  public string GetComponentName();
}

/// <summary>
/// Population modifier classes - cross, mutation, selection and fitness
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IParallelPopulationModifier<T> where T : struct
{
  /// <summary>
  /// Modifies population passed as parameter and returns result.
  /// </summary>
  /// <param name="currentPopulation">Population to be modified</param>
  /// <returns>New modified population</returns>
  public void ModifyPopulation(ref NativeArray<T> currentPopulation);
  /// <summary>
  /// Dispose any allocated resources
  /// </summary>
  public void Dispose();
  /// <summary>
  /// Get name of this component - used for logging purposes in various experiments
  /// </summary>
  /// <returns>Struct declaration name of this component</returns>
  public string GetComponentName();
}

/// <summary>
/// Builder for genetic algorithm. Sets concrete population modifiers.
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IGeneticAlgorithmBuilder<T>
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

public interface IGeneticAlgorithmParallelBuilder<T> where T : struct
{
  /// <summary>
  /// Setter for crossover operator
  /// </summary>
  /// <param name="cross">Crossover operator</param>
  public void SetCrossover(IParallelPopulationModifier<T> cross);
  /// <summary>
  /// Setter for mutation operator
  /// </summary>
  /// <param name="mutation">Mutation operator</param>
  public void SetMutation(IParallelPopulationModifier<T> mutation);
  /// <summary>
  /// Setter for fitness function
  /// </summary>
  /// <param name="fitness">Fitness function</param>
  public void SetFitness(IParallelPopulationModifier<T> fitness);
  /// <summary>
  /// Setter for selection function
  /// </summary>
  /// <param name="selection">Selection function</param>
  public void SetSelection(IParallelPopulationModifier<T> selection);
  /// <summary>
  /// Getter for created GA
  /// </summary>
  /// <returns>New GA created from specified operators and functions</returns>
  public IGeneticAlgorithmParallel<T> GetResult();
}

/// <summary>
/// Interface for genetic algorithm.
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IGeneticAlgorithm<T>
{
  /// <summary>
  /// Crossover operator
  /// </summary>
  public IPopulationModifier<T> crossover { get; set; }
  /// <summary>
  /// Mutation operator
  /// </summary>
  //public IPopulationModifier<T> mutation { get; set; }
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
  /// Number of individuals that 1 population should have
  /// </summary>
  public int populationSize { get; set; }
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
  /// <param name="resources">List of resources</param>
  public void SetResources(System.Collections.Generic.List<object> resources);
  /// <summary>
  /// Perform GA
  /// </summary>
  public void RunGA();
  /// <summary>
  /// Getter for last run result
  /// </summary>
  /// <returns>Vector that is ranked as best among results of last GA run</returns>
  public UnityEngine.Vector2 GetResult();
}


/// <summary>
/// Interface for genetic algorithm that will run in parallel.
/// </summary>
/// <typeparam name="T">Concrete individual class</typeparam>
public interface IGeneticAlgorithmParallel<T>
{
  public int iterations { get; set; }
  /// <summary>
  /// Number of individuals that 1 population should have
  /// </summary>
  public int populationSize { get; set; }
  /// <summary>
  /// Set any required resources for GA to function (e.g. assign quadtree)
  /// </summary>
  /// <param name="resources">List of resources</param>
  public void SetResources(System.Collections.Generic.List<object> resources);
  /// <summary>
  /// Getter for last run result
  /// </summary>
  /// <returns>Vector that is ranked as best among results of last GA run</returns>
  public UnityEngine.Vector2 GetResult();
}
