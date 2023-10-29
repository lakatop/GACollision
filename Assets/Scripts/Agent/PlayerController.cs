using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour
{
  private void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    //if (Input.GetMouseButtonDown(1))
    //{
    //  Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //  if (Physics.Raycast(ray, out var hitInfo))
    //  {
    //    gameObject.GetComponent<NavMeshAgent>().SetDestination(hitInfo.point);
    //  }
    //}
    //var character = gameObject.GetComponent<ThirdPersonCharacter>();
    //if (character != null)
    //{
    //  if (System.Math.Abs(gameObject.GetComponent<NavMeshAgent>().remainingDistance - gameObject.GetComponent<NavMeshAgent>().stoppingDistance) > 1f)
    //  {
    //    character.Move(gameObject.GetComponent<NavMeshAgent>().desiredVelocity, false, false);
    //  }
    //  else
    //  {
    //    character.Move(Vector3.zero, false, false);
    //  }
    //}
  }
}
