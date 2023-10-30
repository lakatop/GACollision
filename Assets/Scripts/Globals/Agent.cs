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
  /// Update function for an agent
  /// Called every simulation step
  /// </summary>
  void Update();
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
  /// Sets destination for an agent.
  /// Agent will navigate towards this point.
  /// </summary>
  /// <param name="des">Destination to be set</param>
  void SetDestination(Vector3 des);
  /// <summary>
  /// Agents identifier
  /// </summary>
  int id { get; set; }
  /// <summary>
  /// Interval for how often should agent call Update on itself
  /// Defaults to 0, meaning it will be updated every simulation step
  /// </summary>
  float updateInterval { get { return 0f; } set { this.updateInterval = value; } }
  /// <summary>
  /// Agents position
  /// </summary>
  Vector3 position { get; }
  /// <summary>
  /// Agents desired destination
  /// </summary>
  Vector3 destination { get; }
  /// <summary>
  /// Collision algorithm that agent uses for collision avoidance.
  /// </summary>
  IBaseCollision collisionAlg { get; set; }
}

/// <summary>
/// Agent component represented in game
/// Implements IBaseAgent interface
/// </summary>
public class Agent : IBaseAgent
{

  // IBaseAgent implementation ----------------------------------------------------

  /// <inheritdoc cref="IBaseAgent.Update"/>
  public void Update()
  {
    if(Time.deltaTime < updateInterval)
    {
      return;
    }

    collisionAlg.CollisionUpdate();
  }
  /// <inheritdoc cref="IBaseAgent.UpdatePosition"/>
  public void UpdatePosition()
  { 
  }
  /// <inheritdoc cref="IBaseAgent.SetPosition(Vector3)"/>
  public void SetPosition(Vector3 pos)
  {
    position = pos;
    if (_object != null)
    {
      _object.transform.position = pos;
    }
  }
  /// <inheritdoc cref="IBaseAgent.SetDestination(Vector3)"/>
  public void SetDestination(Vector3 des)
  {
    destination = des;
    collisionAlg.OnDestinationChange();
  }
  /// <inheritdoc cref="IBaseAgent.id"/>
  public int id { get; set; }
  /// <inheritdoc cref="IBaseAgent._position"/>
  public Vector3 position { get; private set; }
  /// <inheritdoc cref="IBaseAgent._destination"/>
  public Vector3 destination { get; private set; }
  /// <inheritdoc cref="IBaseAgent.updateInterval"/>
  public float updateInterval { get; set; }
  /// <inheritdoc cref="IBaseAgent.collisionAlg"/>
  public IBaseCollision collisionAlg { get; set; }
  // ------------------------------------------------------------------------------


  // Class initialization ---------------------------------------------------------

  // Agent body
  /// <summary>
  /// GameObject representing Agent
  /// </summary>
  private GameObject _object = null;
  /// <summary>
  /// Path to fbx model of agent
  /// </summary>
  private string _fbxPath = "lowman/lowman/models/lowbody";
  /// <summary>
  /// Path to material of agent
  /// </summary>
  private string _materialPath = "lowman/lowman/materials/lowbody";
  /// <summary>
  /// Path to animation controller of agent
  /// </summary>
  private string _animControllerPath = "lowman/lowman/animation/humanoid";

  public Agent()
  {
    _object = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _object.AddComponent<NavMeshAgent>();
    _object.AddComponent<PlayerController>();
    Object.Destroy(_object.GetComponent<MeshFilter>());
    GameObject newModel = Object.Instantiate(Resources.Load<GameObject>(_fbxPath), _object.transform.position, _object.transform.rotation);
    newModel.transform.parent = _object.transform;
    _object.GetComponent<NavMeshAgent>().baseOffset = 0;
    _object.GetComponent<MeshRenderer>().material = Resources.Load<Material>(_materialPath);
    _object.AddComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
    var center = _object.GetComponent<CapsuleCollider>().center;
    center.y = 1f;
    _object.GetComponent<CapsuleCollider>().center = center;
    _object.GetComponent<NavMeshAgent>().speed = 1;
    _object.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(_animControllerPath);
    _object.GetComponent<Animator>().avatar = newModel.GetComponent<Animator>().avatar;

    /// TODO: consider creating new class derived from Agent to assign specific collision algorithm
    collisionAlg = new NavMeshCollision(this);
  }

  // Other methods ----------------------------------------------------------------

  /// <summary>
  /// Set Agents name
  /// </summary>
  /// <param name="name">Agents name.
  /// If empty, Agents name defaults to "Agent<id>"</id></param>
  public void SetName(string name = "")
  {
    if (name == "")
    {
      _object.name = "Agent" + id;
    }
    else
    {
      _object.name = name;
    }
  }

  /// <summary>
  /// Return component attached to agent's GameObject
  /// </summary>
  /// <param name="componentName">Name of component to return</param>
  /// <returns>Component defined by componentName that is attached to this agent</returns>
  public Component GetComponent(string componentName)
  {
    return _object.GetComponent(componentName);
  }
}
