using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CreationScript : MonoBehaviour
{
  public GroupsGlobal groups;
  public string fbxPath = "lowman/lowman/models/lowbody";
  public string materialPath = "lowman/lowman/materials/lowbody";
  public string animControllerPath = "lowman/lowman/animation/humanoid";
  // Start is called before the first frame update
  void Start()
  {
    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    float x = Random.Range(0, 5);
    float z = Random.Range(0, 5);
    capsule.transform.position = new Vector3(x, 0.5f, z);
    capsule.AddComponent<NavMeshAgent>();
    Destroy(capsule.GetComponent<MeshFilter>());
    Debug.Log(fbxPath);
    var low = Resources.Load<GameObject>(fbxPath);
    Debug.Log(low);
    GameObject newModel = Instantiate(low, capsule.transform.position, capsule.transform.rotation);
    newModel.transform.parent = capsule.transform;
    capsule.GetComponent<NavMeshAgent>().baseOffset = 0;
    Material material = Resources.Load<Material>(materialPath);
    capsule.GetComponent<MeshRenderer>().material = material;
    capsule.AddComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
    var center = capsule.GetComponent<CapsuleCollider>().center;
    center.y = 1f;
    capsule.GetComponent<CapsuleCollider>().center = center;
    capsule.GetComponent<NavMeshAgent>().speed = 1;
    var controller = Resources.Load<RuntimeAnimatorController>(animControllerPath);
    capsule.GetComponent<Animator>().runtimeAnimatorController = controller;
    capsule.GetComponent<Animator>().avatar = newModel.GetComponent<Animator>().avatar;


    var agents = GameObject.FindObjectsOfType<NavMeshAgent>();
    foreach(var agent in agents)
    {
      Debug.Log(agent.transform.position);
    }
    var capsuleColliders = FindObjectsOfType<CapsuleCollider>();
    foreach(var col in capsuleColliders)
    {
      Debug.Log(col.transform.position);
    }
    var objcts = SceneManager.GetActiveScene().GetRootGameObjects();
    foreach (var ob in objcts)
    {
      var type = ob.GetComponent<NavMeshAgent>();
      if (type != null)
      {
        Debug.Log("AGENT");
        Debug.Log(ob);
      }
    }
    GameObject emptyGroups = GameObject.Find("Groups");
    if (emptyGroups != null)
    {
      Debug.Log("EMPTY GROUPS");
      Debug.Log(emptyGroups);
      groups = emptyGroups.GetComponent<GroupsGlobal>();
      Debug.Log(groups.groups[0].spawn);
      Debug.Log(groups.groups[0].size);
    }
  }

  // Update is called once per frame
  void Update()
  {
        
  }
}
