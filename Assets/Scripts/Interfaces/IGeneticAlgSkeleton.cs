using Unity.Collections;


/// <summary>
/// Interface for population used in parallel GA. Population consists of set of individuals
/// </summary>
/// <typeparam name="T">Concrete individual struct</typeparam>
public interface IParallelPopulation<T> where T : struct
{
  /// <summary>
  /// Used to dispose individuals in population
  /// </summary>
  public void Dispose();
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
  public void ModifyPopulation(ref NativeArray<T> currentPopulation, int iteration);
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
