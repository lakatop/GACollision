using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgentManager : MonoBehaviour
{
  static AgentManager instance;
  static List<Agent> _agents = new List<Agent>();

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
      agent.SetName();
    }
  }
}
