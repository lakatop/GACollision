using UnityEngine;
using System.Collections.Generic;

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
  /// <param name="newPos">New position</param>
  void UpdatePosition(RVO.Vector2 newPos);
  /// <summary>
  /// Sets agents position
  /// </summary>
  /// <param name="pos">Position that will be set</param>
  void SetPosition(RVO.Vector2 pos);
  /// <summary>
  /// Sets destination for an agent
  /// Agent will navigate towards this point
  /// </summary>
  /// <param name="des">Destination to be set</param>
  void SetDestination(RVO.Vector2 des);
  /// <summary>
  /// Compute neighbors of this agent
  /// </summary>
  void ComputeNeighbors();
  /// <summary>
  /// Computes the new velocity of this agent
  /// </summary>
  void ComputeNewVelocity();
  /// <summary>
  /// Add new agent to neighbors list
  /// </summary>
  /// <param name="agent">Agent to be added</param>
  /// <param name="rangeSq">Squared range around this agent</param>
  void AddAgentNeighbor(IBaseAgent agent, ref float rangeSq);
  /// <summary>
  /// Add new obstacle to neighbors list
  /// </summary>
  /// <param name="obstacle">Obstacle to be added</param>
  /// <param name="rangeSq">Squared range around this agent</param>
  void AddObstacleNeighbor(IBaseObstacle obstacle, float rangeSq);
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
  RVO.Vector2 position { get; }
  /// <summary>
  /// Agents desired destination
  /// </summary>
  RVO.Vector2 destination { get; }
  /// <summary>
  /// Preferred velocity of the agent
  /// </summary>
  RVO.Vector2 preferredVelocity { get; set; }
  /// <summary>
  /// Velocity of the agent
  /// </summary>
  RVO.Vector2 velocity { get; set; }
  /// <summary>
  /// Radius of this agent
  /// </summary>
  float radius { get; set; }
  /// <summary>
  /// The minimal amount of time for which
  /// this agent's velocities that are computed by the simulation are safe
  /// with respect to obstacles.The larger this number, the sooner this
  /// agent will respond to the presence of obstacles, but the less freedom
  /// this agent has in choosing its velocities.Must be positive
  /// </summary>
  float timeHorizonObst { get; set; }
  /// <summary>
  /// Maximum neighbors of this agent
  /// </summary>
  int maxNeighbors { get; set; }
  /// <summary>
  /// Maximum speed of this agent
  /// </summary>
  float maxSpeed { get; set; }
  /// <summary>
  /// Neighbors of this agent
  /// Pair is consisting of neighbor agent and squared distance from that agent
  /// </summary>
  IList<KeyValuePair<float, IBaseAgent>> neighbors { get; set; }
  /// <summary>
  /// Neighbor obstacles of this agent
  /// Pair is consisting of neighbor obstacles and squared distance from that obstacle
  /// </summary>
  IList<KeyValuePair<float, IBaseObstacle>> neighborObstacles { get; set; }
  /// <summary>
  /// Collision avoidance algorithm that agent uses
  /// </summary>
  IBaseCollisionAvoider collisionAlg { get; set; }
  /// <summary>
  /// Path planning algorithm that agent uses
  /// </summary>
  IBasePathPlanner pathPlanningAlg { get; set; }
}
