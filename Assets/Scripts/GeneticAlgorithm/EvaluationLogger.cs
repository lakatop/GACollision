using UnityEngine;
using Unity.Collections;
using System.Text;
using System.IO;
using Unity.Burst;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Logger class for logging during parallel GA run
/// </summary>
[BurstCompile]
public struct BezierIndividualLogger
{
  /// <summary>
  /// Array to hold best individual fitness of each GA iteration
  /// </summary>
  public NativeArray<float> individualFitness;
  /// <summary>
  /// Array to hold best Jerk fitness of each GA iteration
  /// </summary>
  public NativeArray<float> jerkFitness;
  /// <summary>
  /// Array to hold best Collision fitness of each GA iteration
  /// </summary>
  public NativeArray<float> collisionFitness;
  /// <summary>
  /// Array to hold best EndDistance fitness of each GA iteration
  /// </summary>
  public NativeArray<float> endDistanceFitness;
  /// <summary>
  /// Array to hold best TimeToDestination fitness of each GA iteration
  /// </summary>
  public NativeArray<float> ttdFitness;

  /// <summary>
  /// Capture current state of the population
  /// </summary>
  /// <param name="pop">Population's fitness array</param>
  /// <param name="jerkF">Jerk fitnesses of the population</param>
  /// <param name="colF">Collision fitnesses of the population</param>
  /// <param name="endF">EndDestiantion fitnesses of the population</param>
  /// <param name="ttdF">TimeToDestination fitnesses of the population</param>
  /// <param name="iteration">Iteration of GA</param>
  public void LogPopulationState(ref NativeArray<float> pop,
                                 ref NativeArray<float> jerkF,
                                 ref NativeArray<float> colF,
                                 ref NativeArray<float> endF,
                                 ref NativeArray<float> ttdF,
                                 int iteration)
  {
    var bestIndividual = UtilsGA.UtilsGA.GetMinValueFromArray(ref pop);
    var bestJerkF = UtilsGA.UtilsGA.GetMinValueFromArray(ref jerkF);
    var bestColF = UtilsGA.UtilsGA.GetMinValueFromArray(ref colF);
    var bestEndF = UtilsGA.UtilsGA.GetMinValueFromArray(ref endF);
    var bestTtdF = UtilsGA.UtilsGA.GetMinValueFromArray(ref ttdF);

    individualFitness[iteration] = bestIndividual;
    jerkFitness[iteration] = bestJerkF;
    collisionFitness[iteration] = bestColF;
    endDistanceFitness[iteration] = bestEndF;
    ttdFitness[iteration] = bestTtdF;
  }

  /// <summary>
  /// Writing results into a csv file
  /// </summary>
  /// <param name="configuration">Configuration of the GA</param>
  /// <param name="iteration">Holds GA run counter</param>
  /// <param name="scenarioName">Name of scenario that GA was run in</param>
  /// <param name="agentId">Id of agent that run this GA</param>
  public void WriteRes(string configuration, int iteration, string scenarioName, string agentId)
  {
    // Log Individual fitness
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < individualFitness.Length; i++)
    {
      var fit = individualFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    FileInfo file = new FileInfo(string.Format("Plotting/{0}/fitness-Individual-{1}-{2}.csv", scenarioName, agentId, iteration));
    file.Directory.Create(); // If the directory already exists, this method does nothing.

    File.WriteAllText(file.FullName, builder.ToString());

    // Log Jerk fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < jerkFitness.Length; i++)
    {
      var fit = jerkFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-Jerk-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log Collision fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < collisionFitness.Length; i++)
    {
      var fit = collisionFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-Collision-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log EndDistance fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < endDistanceFitness.Length; i++)
    {
      var fit = endDistanceFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-EndDistance-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());

    // Log TTD fitness
    builder.Clear();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < ttdFitness.Length; i++)
    {
      var fit = ttdFitness[i].ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/fitness-TTD-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());
  }

  /// <summary>
  /// Clear resources
  /// </summary>
  public void Dispose()
  {
    individualFitness.Dispose();
    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    ttdFitness.Dispose();
  }
}


/// <summary>
/// Class for logging agent related metrics into a csv file
/// </summary>
public class AgentLogger
{
  /// <summary>
  /// List of velocittes that agent used that created his path
  /// </summary>
  private List<Vector2> _velocities { get; set; }
  /// <summary>
  /// List of times how long each GA run took
  /// </summary>
  private List<double> _gaTimes { get; set; }
  /// <summary>
  /// Number of collisions during agents path
  /// </summary>
  private uint _collisionCount { get; set; }
  /// <summary>
  /// How many frames agent spent in collision during his path
  /// </summary>
  private uint _framesInCollision { get; set; }
  /// <summary>
  /// Time when agent started moving on his path
  /// </summary>
  private double _pathStartTime { get; set; }
  /// <summary>
  /// Time when agent arrived at the end of his path
  /// </summary>
  private double _pathEndTime { get; set; }
  /// <summary>
  /// Id of a scenario
  /// </summary>
  private string _scenarioId { get; set; }
  /// <summary>
  /// Id of an agent
  /// </summary>
  private string _agentId { get; set; }
  /// <summary>
  /// configuration id created from hyperparameters
  /// </summary>
  private string _configurationId { get; set; }
  /// <summary>
  /// Path to output csv file
  /// </summary>
  private string _csvFile { get; set; }
  /// <summary>
  /// Columns of csv file
  /// </summary>
  private static List<string> _columns = new List<string>
  {
    "PathLength", // Number of segments in path
    "PathDuration", // Duration (in seconds) how long it took agent to go from start to end position
    "CollisionCount", // Collision count
    "FramesInCollision", // How many frames agent was in collision
    "PathJerk", // Jerk value of path
    "GaTimes" // How long (in ms) it took for algorithm to run in average
  };

  public AgentLogger()
  {
    _velocities = new List<Vector2>();
    _gaTimes = new List<double>();
    _collisionCount = 0;
    _framesInCollision = 0;
    _pathStartTime = 0f;
    _pathEndTime = 0f;
  }

  /// <summary>
  /// Add new velocity to array
  /// </summary>
  /// <param name="vel">Velocity that will be added</param>
  public void AddVelocity(Vector2 vel)
  {
    _velocities.Add(vel);
  }

  /// <summary>
  /// Add new GA run time
  /// </summary>
  /// <param name="time">GA run time</param>
  public void AddGaTime(double time)
  {
    _gaTimes.Add(time);
  }

  /// <summary>
  /// Create new csv if it doesnt exist already
  /// </summary>
  public void CreateCsv()
  {
    _csvFile = "Plotting/Runs/" + _configurationId + "/" + _scenarioId + "/" + _agentId + ".csv";
    FileInfo fileInfo = new FileInfo(_csvFile);
    fileInfo.Directory.Create();

    if (File.Exists(_csvFile))
    {
      return;
    }

    using (var file = File.CreateText(_csvFile))
    {
      file.WriteLine(string.Join(",", _columns));
    }
  }

  /// <summary>
  /// Append new log into a csv
  /// </summary>
  public void AppendCsvLog()
  {
    var pathLength = _velocities.Count;
    var pathDuration = _pathEndTime - _pathStartTime;
    var collisionCount = _collisionCount;
    var framesInCollision = _framesInCollision;
    var pathJerk = UtilsGA.UtilsGA.CalculatePathJerk(_velocities);
    var gaTimes = _gaTimes.Average();

    using (var file = File.AppendText(_csvFile))
    {
      file.WriteLine("{0},{1},{2},{3},{4},{5}", pathLength, pathDuration, collisionCount, framesInCollision, pathJerk, gaTimes);
    }
  }

  /// <summary>
  /// Create a configuration file holding info about the GA
  /// </summary>
  /// <param name="configuration"></param>
  public void CreateConfigurationFile(string configuration)
  {
    var confFile = "Plotting/Runs/" + _configurationId + "/" + "config.txt";
    FileInfo fileInfo = new FileInfo(confFile);
    fileInfo.Directory.Create();

    if (File.Exists(confFile))
    {
      return;
    }

    using (var file = File.CreateText(confFile))
    {
      file.WriteLine(configuration);
    }
  }

  /// <summary>
  /// Increase _collisionCount counter
  /// </summary>
  public void AddCollisionCount() { _collisionCount++; }
  /// <summary>
  /// Increase _framesInCollision counter
  /// </summary>
  public void AddFramesInCollision() { _framesInCollision++; }
  /// <summary>
  /// Setter for _pathEndTime
  /// </summary>
  /// <param name="endTime">End time</param>
  public void SetEndTime(double endTime) { _pathEndTime = endTime; }
  /// <summary>
  /// Setter for _pathStartTime
  /// </summary>
  /// <param name="startTime">Start time</param>
  public void SetStartTime(double startTime) { _pathStartTime = startTime; }
  /// <summary>
  /// Setter for _scenarioId
  /// </summary>
  /// <param name="name">Scenario id</param>
  public void SetScenarioId(string name) { _scenarioId = name; }
  /// <summary>
  /// Setter for _agentId
  /// </summary>
  /// <param name="name">Agent id</param>
  public void SetAgentId(string name) { _agentId = name; }
  /// <summary>
  /// Setter for _configurationId
  /// </summary>
  /// <param name="confId">Configuration id</param>
  public void SetConfigurationId(string confId) { _configurationId = confId; }
}
