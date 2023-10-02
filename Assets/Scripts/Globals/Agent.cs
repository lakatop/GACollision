using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Defines interface for base agent and properties related to its behaviour
/// </summary>
public interface IBaseAgent
{
  /// <summary>
  /// Use to update agents position
  /// </summary>
  void UpdatePosition();
  /// <summary>
  /// Sets agents position
  /// </summary>
  /// <param name="pos">Position that will be set</param>
  void SetPosition(Vector3 pos);
  /// <summary>
  /// Agents position
  /// </summary>
  Vector3 position { get; }
  /// <summary>
  /// Agents desired destination
  /// </summary>
  Vector3 destination { get; }
}

/// <summary>
/// Agent component represented in game
/// Implements IBaseAgent interface
/// </summary>
public class Agent : MonoBehaviour, IBaseAgent
{

  // IBaseAgent implementation ----------------------------------------------------

  /// <inheritdoc cref="IBaseAgent.UpdatePosition"/>
  public void UpdatePosition()
  {

  }
  public void SetPosition(Vector3 pos)
  {
    position = pos;
    if (_capsule != null)
    {
      _capsule.transform.position = pos;
    }
  }
  /// <inheritdoc cref="IBaseAgent._position"/>
  public Vector3 position { get; private set; }
  /// <inheritdoc cref="IBaseAgent._destination"/>
  public Vector3 destination { get; private set; }

  // ------------------------------------------------------------------------------


  // Unity related stuff ----------------------------------------------------------

  // Agent body
  private GameObject _capsule = null;
  private string _fbxPath = "lowman/lowman/models/lowbody";
  private string _materialPath = "lowman/lowman/materials/lowbody";
  private string _animControllerPath = "lowman/lowman/animation/humanoid";

  void Awake()
  {
    _capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _capsule.AddComponent<NavMeshAgent>();
    Destroy(_capsule.GetComponent<MeshFilter>());
    GameObject newModel = Instantiate(Resources.Load<GameObject>(_fbxPath), _capsule.transform.position, _capsule.transform.rotation);
    newModel.transform.parent = _capsule.transform;
    _capsule.GetComponent<NavMeshAgent>().baseOffset = 0;
    _capsule.GetComponent<MeshRenderer>().material = Resources.Load<Material>(_materialPath);
    _capsule.AddComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
    var center = _capsule.GetComponent<CapsuleCollider>().center;
    center.y = 1f;
    _capsule.GetComponent<CapsuleCollider>().center = center;
    _capsule.GetComponent<NavMeshAgent>().speed = 1;
    _capsule.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(_animControllerPath);
    _capsule.GetComponent<Animator>().avatar = newModel.GetComponent<Animator>().avatar;
  }

  // Update is called once per frame
  void Update()
  {
   
  }

  // ------------------------------------------------------------------------------


}
