using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AgentManager : MonoBehaviour
{
  static AgentManager instance;
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

  void Start()
  {
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      SpawnAgent();
    }
    else if (Input.GetMouseButtonDown(1))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo))
      {
        foreach (var agent in _agents)
        {
          agent.SetDestination(hitInfo.point);
        }
      }
    }

    // Call update on agents
    foreach (var agent in _agents)
    {
      agent.Update();
    }
  }

  private void SpawnAgent()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hitInfo))
    {
      _agents.Add(new Agent());
      var agent = _agents[_agents.Count - 1];
      agent.id = _agents.Count;
      agent.SetPosition(hitInfo.point);
      if (agent is Agent)
      {
        ((Agent)agent).SetName();
      }
    }
  }
}
