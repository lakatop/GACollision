using UnityEngine;
using System.Collections.Generic;

public interface IBaseCollisionAvoider
{
  /// <summary>
  /// Called before the simulation happens.
  /// This is the place to initialize all things needed before simulation.
  /// </summary>
  void OnStart();
  /// <summary>
  /// Update function called every simulation step
  /// </summary>
  void Update();
  /// <summary>
  /// Called when new agent was added to simulation
  /// </summary>
  void OnAgentAdded(IBaseAgent agent);
  /// <summary>
  /// Gets position of agent
  /// </summary>
  /// <param name="id">Id of agent</param>
  /// <returns></returns>
  Vector2 GetAgentPosition(int id);
  /// <summary>
  /// Sets agents preferred velocity
  /// </summary>
  /// <param name="id">Agents id</param>
  /// <param name="prefVelocity">Preferred velocity to be set</param>
  void SetAgentPreferredVelocity(int id, Vector2 prefVelocity);
  /// <summary>
  /// Gets agents preferred velocity
  /// </summary>
  /// <param name="id">Id of agent</param>
  /// <returns></returns>
  Vector2 GetAgentPreferredVelocity(int id);
  /// <summary>
  /// Gets agents velocity
  /// </summary>
  /// <param name="id">Id of agent</param>
  /// <returns></returns>
  Vector2 GetAgentVelocity(int id);
  /// <summary>
  /// Agent that will be using this collision avoidance algorithm
  /// </summary>
  IBaseAgent agent { get; }
  /// <summary>
  /// Register obstacles present in the simulation
  /// </summary>
  /// <param name="obstacles">Obstacles to register</param>
  void RegisterObstacles(List<Obstacle> obstacles);
}
