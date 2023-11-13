using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton
/// Agent manager class
/// Keeps track of all agents in the simulation and is responsible for updating them
/// </summary>
public class SimulationManager : MonoBehaviour
{
  /// <summary>
  /// Static instance of this class
  /// </summary>
  public static SimulationManager Instance { get; private set; }
  /// <summary>
  /// List of all agents present in the simulation
  /// </summary>
  public List<IBaseAgent> _agents { get; private set; }
  /// <summary>
  /// List of all collision avoidance algorithms that registered themselves to SimulationManager
  /// </summary>
  private List<IBaseCollisionAvoider> _collisionListeners { get; set; }
  /// <summary>
  /// Manager for collision avoidance algorithms
  /// This should be the only instance in entire simulation
  /// </summary>
  public CollisionManager _collisionManager { get; private set; }

  void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
    }
    else
    {
      Instance = this;
    }

    _agents = new List<IBaseAgent>();
    _collisionListeners = new List<IBaseCollisionAvoider>();
    _collisionManager = new CollisionManager();
  }

  /// <summary>
  /// Called every simulation step
  /// Check for user input and update agents
  /// </summary>
  void Update()
  {
    // TODO: will probably require refactor in the future:
    //    more robust user input, setting destination to just some client(s) etc.

    // On left mouse click - spawn agent
    if (Input.GetMouseButtonDown(0))
    {
      SpawnAgent();
    }
    // On right mouse click - set new destination for all agents
    else if (Input.GetMouseButtonDown(1))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo))
      {
        foreach (var agent in _agents)
        {
          agent.SetDestination(new Vector2(hitInfo.point.x, hitInfo.point.z));
        }
      }
    }
    else
    {
      // Call update on agents
      foreach (var agent in _agents)
      {
        agent.Update();
      }
      foreach (var collisionAvoider in _collisionListeners)
      {
        collisionAvoider.Update();
      }
      foreach (var agent in _agents)
      {
        agent.UpdatePosition(Vector2.zero);
      }
    }

  }

  /// <summary>
  /// Add collision avoidance algorithm instance to list of listeners.
  /// This listener will receive updating calls
  /// </summary>
  /// <param name="collisionAvoider">Instance to be added</param>
  public void RegisterCollisionListener(IBaseCollisionAvoider collisionAvoider)
  {
    _collisionListeners.Add(collisionAvoider);
  }

  public List<IBaseAgent> GetAgents()
  {
    return _agents;
  }

  /// <summary>
  /// Create new agent according to current mouse position
  /// </summary>
  private void SpawnAgent()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hitInfo))
    {
      _agents.Add(new ORCAAgent());
      var agent = _agents[_agents.Count - 1];
      agent.id = _agents.Count;
      agent.SetPosition(new Vector2(hitInfo.point.x, hitInfo.point.z));
      ((ORCAAgent)agent).prevPos = new Vector2(hitInfo.point.x, hitInfo.point.z);
      agent.SetDestination(agent.position);
      if (agent is BaseAgent)
      {
        ((BaseAgent)agent).SetName();
      }

      foreach (var collisionAvoider in _collisionListeners)
      {
        collisionAvoider.OnAgentAdded(agent);
      }
    }
  }
}
