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
  protected GameObject _object = null;
  /// <summary>
  /// Path to material of agent
  /// </summary>
  private string _materialPath = "group1";
  /// <summary>
  /// Previous agents position
  /// </summary>
  protected Vector3 _lastPosition { get; set; }

  public BaseAgent()
  {
    _object = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _object.GetComponent<CapsuleCollider>().radius = 0.5f;
    _object.AddComponent<Rigidbody>();
    _object.AddComponent<NavMeshAgent>();
    _object.GetComponent<NavMeshAgent>().baseOffset = 1f;
    _object.GetComponent<MeshRenderer>().material = Resources.Load<Material>(_materialPath);
    _object.AddComponent<DirectionArrowGizmo>();
    _object.AddComponent<LineRenderer>();
    _object.AddComponent<AgentCollisionDetectionHandler>();
    _object.GetComponent<AgentCollisionDetectionHandler>().agent = this;
  }

  // IBaseAgent interface ---------------------------------------------------------

  /// <inheritdoc cref="IBaseAgent.OnBeforeUpdate"/>
  public abstract void OnBeforeUpdate();
  /// <inheritdoc cref="IBaseAgent.OnAfterUpdate"/>
  public abstract void OnAfterUpdate();
  /// <inheritdoc cref="IBaseAgent.SetDestination(Vector3)"/>
  public abstract void SetDestination(Vector2 des);
  /// <inheritdoc cref="IBaseAgent.id"/>
  public int id { get; set; }
  /// <inheritdoc cref="IBaseAgent.speed"/>
  public float speed { get; set; }
  /// <inheritdoc cref="IBaseAgent._position"/>
  public Vector2 position { get; protected set; }
  /// <inheritdoc cref="IBaseAgent._destination"/>
  public Vector2 destination { get; protected set; }
  /// <inheritdoc cref="IBaseAgent.inDestination"/>
  public bool inDestination { get; set; }
  /// <inheritdoc cref="IBaseAgent.pathPlanningAlg"/>
  public abstract IBasePathPlanner pathPlanningAlg { get; set; }
  /// <inheritdoc cref="IBaseAgent.logger"/>
  public abstract ILogger logger { get; set; }
  /// <inheritdoc cref="IBaseAgent.SetPosition(Vector2)"/>
  public void SetPosition(Vector2 pos)
  {
    _lastPosition = _object.transform.position;
    if (_object.transform.position.y > 1.59f)
    {
      _object.transform.position = new Vector3(pos.x, 1.58f, pos.y);
    }

    // Set current position
    position = pos;

    // Also transform gameobject
    if (_object != null)
    {
      _object.transform.position = new Vector3(position.x, 1.58f, position.y);
    }
  }
  /// <inheritdoc cref="IBaseAgent.SetForward(Vector2)"/>
  public void SetForward(Vector2 forw)
  {
    _object.transform.forward = new Vector3(forw.x, _object.transform.forward.y, forw.y);
    _object.transform.rotation = Quaternion.LookRotation(_object.transform.forward);
    //float singleStep = speed * Time.deltaTime;
    //var direction = Vector3.RotateTowards(_object.transform.forward, new Vector3(forw.x, 0, forw.y), singleStep, 0.0f);
    //_object.transform.rotation = Quaternion.LookRotation(direction);
  }

  /// <inheritdoc cref="IBaseAgent.GetForward"/>
  public Vector2 GetForward()
  {
    return new Vector2(_object.transform.forward.x, _object.transform.forward.z);
  }

  /// <inheritdoc cref="IBaseAgent.GetVelocity"/>
  public Vector2 GetVelocity()
  {
    return new Vector2(position.x - _lastPosition.x, position.y - _lastPosition.z);
  }
  
  // Other methods ----------------------------------------------------------------

  /// <summary>
  /// Represents scenario name in which agent is present
  /// </summary>
  public string scenarioName { get; set; }

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
  /// Setter for spawn position.
  /// </summary>
  /// <param name="pos">Position where to put agent</param>
  public void SpawnPosition(Vector2 pos)
  {
    if (_object.transform.position.y > 1.59f)
    {
      _object.transform.position = new Vector3(pos.x, 1.58f, pos.y);
    }

    _object.transform.position = new Vector3(pos.x, 1.58f, pos.y);
    GetComponent<NavMeshAgent>().Warp(_object.transform.position);
    position = pos;
    _lastPosition = position;
  }

  /// <summary>
  /// Triggered by AgentCollisionDetectionHandler.OnCollisionEnter
  /// </summary>
  public virtual void OnCollisionEnter()
  {
  }

  /// <summary>
  /// Triggered by AgentCollisionDetectionHandler.OnCollisionStay
  /// </summary>
  public virtual void OnCollisionStay()
  {
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

  /// <summary>
  /// Getter for agents position
  /// </summary>
  /// <returns>Position of agent</returns>
  public Vector3 GetPos()
  {
    return _object.transform.position;
  }

  /// <summary>
  /// Getter for gameobject of agent
  /// </summary>
  /// <returns>Agent's gameobject</returns>
  public GameObject GetGameObject()
  {
    return _object;
  }
}

/// <summary>
/// Class for collision detection inside the simulation
/// </summary>
public class AgentCollisionDetectionHandler : MonoBehaviour
{
  /// <summary>
  /// Reference to agent to which this component is attached
  /// </summary>
  public BaseAgent agent;

  /// <summary>
  /// Called when agent enters collision
  /// </summary>
  /// <param name="collision">Collision object</param>
  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject != SimulationManager.Instance.GetPlatform())
    {
      agent.OnCollisionEnter();
    }
  }

  /// <summary>
  /// Called each frame agent stays in collision
  /// </summary>
  /// <param name="collision">Collision object</param>
  private void OnCollisionStay(Collision collision)
  {
    if (collision.gameObject != SimulationManager.Instance.GetPlatform())
    {
      agent.OnCollisionStay();
    }
  }
}