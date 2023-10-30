using UnityEngine;

/// <summary>
/// Defines interface for base agent and properties related to its behaviour
/// </summary>
public interface IBaseAgent
{
  /// <summary>
  /// Update function for an agent
  /// Called every simulation step
  /// </summary>
  void Update();
  /// <summary>
  /// Use to update agents position
  /// </summary>
  void UpdatePosition();
  /// <summary>
  /// Sets agents position
  /// </summary>
  /// <param name="pos">Position that will be set</param>
  void SetPosition(Vector3 pos);
  /// <summary>
  /// Sets destination for an agent.
  /// Agent will navigate towards this point.
  /// </summary>
  /// <param name="des">Destination to be set</param>
  void SetDestination(Vector3 des);
  /// <summary>
  /// Agents identifier
  /// </summary>
  int id { get; set; }
  /// <summary>
  /// Interval for how often should agent call Update on itself
  /// Defaults to 0, meaning it will be updated every simulation step
  /// </summary>
  float updateInterval { get { return 0f; } set { this.updateInterval = value; } }
  /// <summary>
  /// Agents position
  /// </summary>
  Vector3 position { get; }
  /// <summary>
  /// Agents desired destination
  /// </summary>
  Vector3 destination { get; }
  /// <summary>
  /// Collision avoidance algorithm that agent uses
  /// </summary>
  IBaseCollisionAvoider collisionAlg { get; set; }
  /// <summary>
  /// Path planning algorithm that agent uses
  /// </summary>
  IBasePathPlanner pathPlanningAlg { get; set; }
}
