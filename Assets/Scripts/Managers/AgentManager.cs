using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton
/// Agent manager class
/// Keeps track of all agents in the simulation and is responsible for updating them
/// </summary>
public class AgentManager : MonoBehaviour
{
  /// <summary>
  /// Static instance of this class
  /// </summary>
  static AgentManager instance;
  /// <summary>
  /// List of all agents present in the simulation
  /// </summary>
  static List<IBaseAgent> _agents = new List<IBaseAgent>();

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
      _agents.Add(new MyNavMeshAgent());
      var agent = _agents[_agents.Count - 1];
      agent.id = _agents.Count;
      agent.SetPosition(new Vector2(hitInfo.point.x, hitInfo.point.z));
      if (agent is MyNavMeshAgent)
      {
        ((MyNavMeshAgent)agent).SetName();
      }
    }
  }
}
