using System.Text.RegularExpressions;
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
  private string _materialPath = "group1";
  /// <summary>
  /// Path to animation controller of agent
  /// </summary>
  private string _animControllerPath = "lowman/lowman/animation/humanoid";

  public BaseAgent()
  {
    _object = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    _object.GetComponent<CapsuleCollider>().radius = 0.5f;
    _object.AddComponent<NavMeshAgent>();
    _object.GetComponent<NavMeshAgent>().baseOffset = 1f;
    _object.GetComponent<MeshRenderer>().material = Resources.Load<Material>(_materialPath);

    Create3DArrowIndicator(_object.transform);
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
  /// <inheritdoc cref="IBaseAgent.SetPosition(Vector2)"/>
  public void SetPosition(Vector2 pos)
  {
    position = pos;
    if (_object != null)
    {
      _object.transform.position = new Vector3(pos.x, 1.5f, pos.y);
      GetComponent<NavMeshAgent>().Warp(new Vector3(pos.x, 1.5f, pos.y));
    }
  }
  /// <inheritdoc cref="BaseAgent.SetForward(Vector2)"/>
  public void SetForward(Vector2 forw)
  {
    _object.transform.forward = new Vector3(forw.x, 0, forw.y).normalized;
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

  public Vector3 GetPos()
  {
    return _object.transform.position;
  }

  GameObject Create3DArrowIndicator(Transform parentTransform)
  {
    // Create a child GameObject for the 3D arrow indicator
    GameObject arrow = new GameObject("3DArrowIndicator");
    arrow.transform.parent = parentTransform;

    // Create a MeshFilter component for the arrow mesh
    MeshFilter meshFilter = arrow.AddComponent<MeshFilter>();
    meshFilter.mesh = Create3DArrowMesh();

    // Create a MeshRenderer component for the arrow material
    MeshRenderer meshRenderer = arrow.AddComponent<MeshRenderer>();
    meshRenderer.material.color = Color.red; // Set the arrow color

    // Position and scale the arrow (modify these values as needed)
    arrow.transform.localPosition = new Vector3(0f, 0f, 1f);
    arrow.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

    return arrow;
  }

  Mesh Create3DArrowMesh()
  {
    // Create a 3D arrow mesh pointing in the positive z-direction
    Mesh arrowMesh = new Mesh();
    Vector3[] vertices = new Vector3[]
    {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(-0.05f, 0.8f, 0.2f),
            new Vector3(0.05f, 0.8f, 0.2f),
    };
    int[] triangles = new int[] { 0, 1, 2, 0, 1, 3 };
    arrowMesh.vertices = vertices;
    arrowMesh.triangles = triangles;
    arrowMesh.RecalculateNormals();
    return arrowMesh;
  }
}
