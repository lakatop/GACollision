using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
  static SpawnManager instance;
  static List<GameObject> _agents = new List<GameObject>();

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
    Debug.Log("START CALLED");
  }

  void Update()
  {
    //if (Input.GetKeyDown("1"))
    //{
    //  SceneManager.LoadScene("SampleScene");
    //}
    //else if (Input.GetKeyDown("2"))
    //{
    //  SceneManager.LoadScene("Scene2");
    //}
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo))
      {
        _agents.Add(new GameObject());
        var agent = _agents[_agents.Count - 1];
        agent.AddComponent<Agent>();
        agent.GetComponent<Agent>().SetPosition(hitInfo.point);
      }
    }
  }
}
