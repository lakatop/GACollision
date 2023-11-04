using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Agent component represented in game
/// Implements IBaseAgent interface
/// </summary>
public abstract class BaseAgent : IBaseAgent
{

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

  public BaseAgent()
  {
    _object = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _object.AddComponent<NavMeshAgent>();
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
  }

  // IBaseAgent interface ---------------------------------------------------------

  /// <inheritdoc cref="IBaseAgent.Update"/>
  public abstract void Update();
  /// <inheritdoc cref="IBaseAgent.UpdatePosition"/>
  public abstract void UpdatePosition(Vector2 newPos);
  /// <inheritdoc cref="IBaseAgent.SetDestination(Vector3)"/>
  public abstract void SetDestination(Vector2 des);
  /// <inheritdoc cref="IBaseAgent.id"/>
  public int id { get; set; }
  /// <inheritdoc cref="IBaseAgent._position"/>
  public Vector2 position { get; protected set; }
  /// <inheritdoc cref="IBaseAgent._destination"/>
  public Vector2 destination { get; protected set; }
  /// <inheritdoc cref="IBaseAgent.updateInterval"/>
  public float updateInterval { get; set; }
  /// <inheritdoc cref="IBaseAgent.collisionAlg"/>
  public abstract IBaseCollisionAvoider collisionAlg { get; set; }
  /// <inheritdoc cref="IBaseAgent.pathPlanningAlg"/>
  public abstract IBasePathPlanner pathPlanningAlg { get; set; }
  /// <inheritdoc cref="IBaseAgent.SetPosition(Vector3)"/>
  public void SetPosition(Vector2 pos)
  {
    position = pos;
    if (_object != null)
    {
      _object.transform.position = new Vector3(pos.x, 0, pos.y);
    }
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
  /// Get component attached to agent's GameObject
  /// </summary>
  /// <typeparam name="T">Component type</typeparam>
  /// <returns>Component of type T attached to agent</returns>
  public T GetComponent<T>()
  {
    return _object.GetComponent<T>();
  }

  /// <summary>
  /// Add component to agent's GameObject
  /// </summary>
  /// <typeparam name="T">Component type</typeparam>
  /// <returns>Added component</returns>
  public T AddComponent<T>() where T : MonoBehaviour
  {
    return _object.AddComponent<T>();
  }
}
