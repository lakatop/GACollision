using UnityEngine;
using Unity.Collections;
using System.Text;
using System.IO;
using Unity.Burst;
using System.Collections.Generic;
using System.Linq;

[BurstCompile]
public struct StraightLineEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;
  public Vector2 _agentPosition;
  public Vector2 _agentDestination;
  public Vector2 _agentForward;
  public float _agentSpeed;


  public void LogPopulationState(ref NativeArray<BasicIndividualStruct> pop, int iteration)
  {
    var bestIndividual = pop[0];

    for (int i = 0; i < pop.Length; i++)
    {
      if (pop[i].fitness > bestIndividual.fitness)
      {
        bestIndividual = pop[i];
      }
    }

    var outdatedIndividual = _topIndividuals[iteration];
    outdatedIndividual.fitness = bestIndividual.fitness;
    for (int j = 0; j < outdatedIndividual.path.Length; j++)
    {
      outdatedIndividual.path[j] = bestIndividual.path[j];
    }
    _topIndividuals[iteration] = outdatedIndividual;
  }

  public void WriteRes(string configuration, int iteration)
  {
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness,Objective");

    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      var individual = _topIndividuals[i];
      var position = _agentPosition;
      var fit = individual.fitness.ToString();

      var objective = (_agentDestination - position).normalized;
      objective = objective * _agentSpeed * SimulationManager.Instance._agentUpdateInterval;
      objective = objective + position;

      // calculate how far are we from objective
      var rotationVector = _agentForward.normalized;
      if (rotationVector.x == 0 && rotationVector.y == 0)
        rotationVector = new Vector2(1, 0);
      var rotatedVector = UtilsGA.UtilsGA.RotateVector(rotationVector, individual.path[0].x);
      rotatedVector = rotatedVector * individual.path[0].y;
      var rotatedAndTranslatedVector = UtilsGA.UtilsGA.MoveToOrigin(rotatedVector, position);

      var distance = new Vector2(objective.x - rotatedAndTranslatedVector.x, objective.y - rotatedAndTranslatedVector.y).magnitude;
      builder.AppendLine(string.Format("{0},{1}", fit, distance));
    }

    File.WriteAllText(string.Format("Plotting/straightLine/out{0}.csv", iteration), builder.ToString());
  }

  public void Dispose()
  {
    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      _topIndividuals[i].Dispose();
    }
    _topIndividuals.Dispose();
  }
}

[BurstCompile]
public struct FitnessEvaluationLogger
{
  public NativeArray<BasicIndividualStruct> _topIndividuals;


  public void LogPopulationState(ref NativeArray<BasicIndividualStruct> pop, int iteration)
  {
    var bestIndividual = pop[0];

    for (int i = 0; i < pop.Length; i++)
    {
      if (pop[i].fitness > bestIndividual.fitness)
      {
        bestIndividual = pop[i];
      }
    }

    var outdatedIndividual = _topIndividuals[iteration];
    outdatedIndividual.fitness = bestIndividual.fitness;
    for (int j = 0; j < outdatedIndividual.path.Length; j++)
    {
      outdatedIndividual.path[j] = bestIndividual.path[j];
    }
    _topIndividuals[iteration] = outdatedIndividual;
  }

  public void WriteRes(string configuration, int iteration, string scenarioName, string agentId)
  {
    var builder = new StringBuilder();
    builder.AppendLine(configuration);

    builder.AppendLine("Fitness");

    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      var fit = _topIndividuals[i].fitness.ToString();
      builder.AppendLine(string.Format("{0}", fit));
    }

    File.WriteAllText(string.Format("Plotting/{0}/out-{1}-{2}.csv", scenarioName, agentId, iteration), builder.ToString());
  }

  public void Dispose()
  {
    for (int i = 0; i < _topIndividuals.Length; i++)
    {
      _topIndividuals[i].Dispose();
    }
    _topIndividuals.Dispose();
  }
}


[BurstCompile]
public struct BezierIndividualLogger
{
  // Arrays to hold best fitnesses in each iteration
  public NativeArray<float> individualFitness;
  public NativeArray<float> jerkFitness;
  public NativeArray<float> collisionFitness;
  public NativeArray<float> endDistanceFitness;
  public NativeArray<float> ttdFitness;

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

  public void Dispose()
  {
    individualFitness.Dispose();
    jerkFitness.Dispose();
    collisionFitness.Dispose();
    endDistanceFitness.Dispose();
    ttdFitness.Dispose();
  }
}


public class ScenarioLogger
{

}

public class AgentLogger
{
  private List<Vector2> _velocities { get; set; }
  private List<double> _gaTimes { get; set; }
  private uint _collisionCount { get; set; }
  private uint _framesInCollision { get; set; }
  private double _pathStartTime { get; set; }
  private double _pathEndTime { get; set; }
  private string _scenarioId { get; set; }
  private string _agentId { get; set; }
  private string _configurationId { get; set; }
  private string _csvFile { get; set; }
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

  public void AddVelocity(Vector2 vel)
  {
    _velocities.Add(vel);
  }

  public void AddGaTime(double time)
  {
    _gaTimes.Add(time);
  }

  public void CreateCsv()
  {
    _csvFile = "Plotting/" + _configurationId + "/" + _scenarioId + "/" + _agentId + ".csv";
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

  public void CreateConfigurationFile(string configuration)
  {
    var confFile = _csvFile = "Plotting/" + _configurationId + "/" + "config.txt";
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

  public void AddCollisionCount() { _collisionCount++; }
  public void AddFramesInCollision() { _framesInCollision++; }
  public void SetEndTime(double endTime) { _pathEndTime = endTime; }
  public void SetStartTime(double startTime) { _pathStartTime = startTime; }
  public void SetScenarioId(string name) { _scenarioId = name; }
  public void SetAgentId(string name) { _agentId = name; }
  public void SetConfigurationId(string confId) { _configurationId = confId; }
}