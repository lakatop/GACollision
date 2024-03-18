using UnityEngine;

/// <summary>
/// Defines interface for base agent and properties related to its behaviour
/// </summary>
public interface IBaseAgent
{
  /// <summary>
  /// Update function for an agent
  /// Called every simulation step before other updates
  /// </summary>
  void OnBeforeUpdate();
  /// <summary>
  /// Updatefunction for an agent
  /// Called every simulation step after other updates
  /// </summary>
  /// <param name="newPos">New position</param>
  void OnAfterUpdate(Vector2 newPos);
  /// <summary>
  /// Sets agents position
  /// </summary>
  /// <param name="pos">Position that will be set</param>
  void SetPosition(Vector2 pos);
  /// <summary>
  /// Sets agents forward vector
  /// </summary>
  /// <param name="forw">Forward vector</param>
  void SetForward(Vector2 forw);
  /// <summary>
  /// Sets destination for an agent.
  /// Agent will navigate towards this point.
  /// </summary>
  /// <param name="des">Destination to be set</param>
  void SetDestination(Vector2 des);
  /// <summary>
  /// Getter for agents forward vector
  /// </summary>
  /// <returns>Agents forward vector</returns>
  Vector2 GetForward();
  /// <summary>
  /// Getter for agents current velocity
  /// </summary>
  /// <returns>Current agents velocity</returns>
  Vector2 GetVelocity();
  /// <summary>
  /// Agents identifier
  /// </summary>
  int id { get; set; }
  /// <summary>
  /// Speed of the agent
  /// </summary>
  float speed { get; set; }
  /// <summary>
  /// Interval for how often should agent call Update on itself
  /// Defaults to 0, meaning it will be updated every simulation step
  /// </summary>
  float updateInterval { get { return 0f; } set { this.updateInterval = value; } }
  /// <summary>
  /// Returns whether agent is in its final destination
  /// </summary>
  bool inDestination { get; set; }
  /// <summary>
  /// Agents position
  /// </summary>
  Vector2 position { get; }
  /// <summary>
  /// Agents desired destination
  /// </summary>
  Vector2 destination { get; }
  /// <summary>
  /// Path planning algorithm that agent uses
  /// </summary>
  IBasePathPlanner pathPlanningAlg { get; set; }
}
