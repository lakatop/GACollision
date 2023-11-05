using System.Collections.Generic;
using RVO;
using UnityEngine;

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
  static SimulationManager instance;
  /// <summary>
  /// List of all agents present in the simulation
  /// </summary>
  public static List<IBaseAgent> agents = new List<IBaseAgent>();
  /// <summary>
  /// List of all obstaclees present in the simulation
  /// </summary>
  public static List<IBaseObstacle> obstacles = new List<IBaseObstacle>();
  /// <summary>
  /// K-d tree for agents and obstacles in simulation
  /// </summary>
  public KdTree kdTree = new KdTree();

  public static SimulationManager GetInstance()
  {
    return instance;
  }

  void Awake()
  {
    if (instance != null)
    {
      Destroy(gameObject);
    }
    else
    {
      instance = this;
      DontDestroyOnLoad(gameObject);
    }
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
        foreach (var agent in agents)
        {
          agent.SetDestination(new RVO.Vector2(hitInfo.point.x, hitInfo.point.z));
        }
      }
    }
    else
    {
      // Call update on agents
      foreach (var agent in agents)
      {
        agent.Update();
      }
    }

  }

  /// <summary>
  /// Create new agent according to current mouse position
  /// </summary>
  private void SpawnAgent()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hitInfo))
    {
      agents.Add(new MyNavMeshAgent());
      var agent = agents[agents.Count - 1];
      agent.id = agents.Count;
      agent.SetPosition(new RVO.Vector2(hitInfo.point.x, hitInfo.point.z));
      if (agent is MyNavMeshAgent)
      {
        ((MyNavMeshAgent)agent).SetName();
      }
    }
  }
}
