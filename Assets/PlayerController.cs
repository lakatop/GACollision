using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour
{
  public NavMeshAgent[] agents;
  public ThirdPersonCharacter[] characters;
  private void Start()
  {
    agents = FindObjectsOfType<NavMeshAgent>();
    foreach(var agent in agents)
    {
      agent.updateRotation = false;
    }
    //Debug.Log(agents.Length);
  }
  // Update is called once per frame
  void Update()
  {
    agents = FindObjectsOfType<NavMeshAgent>();
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo))
      {
        foreach (var agent in agents)
        {
          agent.SetDestination(hitInfo.point);
        }
      }
    }

    foreach (var agent in agents)
    {
      var character = agent.GetComponent<ThirdPersonCharacter>();
      if (character != null)
      {
        if (System.Math.Abs(agent.remainingDistance - agent.stoppingDistance) > 1f)
        {
          character.Move(agent.desiredVelocity, false, false);
        }
        else
        {
          character.Move(Vector3.zero, false, false);
        }
      }
    }
  }
}
