using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
  static SpawnManager instance;

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
    if (Input.GetKeyDown("1"))
    {
      SceneManager.LoadScene("SampleScene");
    }
    else if (Input.GetKeyDown("2"))
    {
      SceneManager.LoadScene("Scene2");
    }
  }
}
