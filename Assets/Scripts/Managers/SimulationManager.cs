using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Singleton
/// Agent manager class
/// Keeps track of all agents in the simulation and is responsible for updating them
/// </summary>
public class SimulationManager : MonoBehaviour
{
  /// <summary>
  /// Static instance of this class
  /// </summary>
  public static SimulationManager Instance { get; private set; }
  /// <summary>
  /// List of all agents present in the simulation
  /// </summary>
  private List<IBaseAgent> _agents { get; set; }
  /// <summary>
  /// List of all obstacles present in the simulation
  /// </summary>
  public List<Obstacle> _obstacles { get; private set; }
  /// <summary>
  /// List of all collision avoidance algorithms that registered themselves to SimulationManager
  /// </summary>
  private List<IBaseCollisionAvoider> _collisionListeners { get; set; }
  /// <summary>
  /// List of all classes that require some special resource allocation/deallocation during simulation
  /// e.g. GeneticAlgorithm for NativeArray(s)
  /// </summary>
  private List<IResourceManager> _resourceListeners { get; set; }
  /// <summary>
  /// Manager for collision avoidance algorithms
  /// This should be the only instance in entire simulation
  /// </summary>
  public AlgorithmsManager _collisionManager { get; private set; }
  /// <summary>
  /// Walking platform bound
  /// </summary>
  private NativeQuadTree.AABB2D _platfornm { get; set; }
  /// <summary>
  /// Represents QuadElements (points) for static obstacles
  /// </summary>
  private List<NativeQuadTree.QuadElement<TreeNode>> _quadtreeStaticElements { get; set; }
  /// <summary>
  /// Represents QuadElements (points) for agents positions
  /// </summary>
  private List<NativeQuadTree.QuadElement<TreeNode>> _quadAgentsPositions { get; set; }
  /// <summary>
  /// Quadtree for current simulation
  /// </summary>
  private NativeQuadTree.NativeQuadTree<TreeNode> _quadTree { get; set; }
  private bool _quadTreeCreated { get; set; }
  /// <summary>
  /// Data to fill _quadTree with
  /// </summary>
  private NativeArray<NativeQuadTree.QuadElement<TreeNode>> _quadTreeData;
  /// <summary>
  /// Delta t - determines how often agent calculates new position
  /// </summary>
  public float _agentUpdateInterval { get; private set; }
  /// <summary>
  /// 
  /// </summary>
  private float _updateTimer { get; set; }

  void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
    }
    else
    {
      Instance = this;
    }

    _agents = new List<IBaseAgent>();
    _obstacles = new List<Obstacle>();
    _collisionListeners = new List<IBaseCollisionAvoider>();
    _collisionManager = new AlgorithmsManager();
    _resourceListeners = new List<IResourceManager>();
    _quadtreeStaticElements = new List<NativeQuadTree.QuadElement<TreeNode>>();
    _quadAgentsPositions = new List<NativeQuadTree.QuadElement<TreeNode>>();
    _agentUpdateInterval = 0.5f;
    _quadTreeCreated = false;
  }

  void Start()
  {
    RegisterObstacles();
    RegisterWalkingPlatform();
    TransformObstaclesToQuadElements();
  }

  /// <summary>
  /// Called every simulation step
  /// Check for user input and update agents
  /// </summary>
  void Update()
  {
    _updateTimer += Time.deltaTime;
    // TODO: will probably require refactor in the future:
    //    more robust user input, setting destination to just some client(s) etc.

    // On left mouse click - spawn agent
    if (Input.GetMouseButtonDown(0))
    {
      SpawnAgent();
    }
    // On right mouse click - set new destination for all agents
    else if (Input.GetMouseButtonDown(1))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo))
      {
        foreach (var agent in _agents)
        {
          agent.SetDestination(new Vector2(agent.position.x, agent.position.y + 30));
        }
      }
    }
    else
    {
      if (_updateTimer > _agentUpdateInterval)
      {
        foreach(var agent in _agents)
        {
          agent.OnAfterUpdate(Vector2.zero);
        }

        _updateTimer = 0f;

        // Dispose previous quadtree and its data
        if (_quadTreeCreated)
        {
          _quadTree.Dispose();
          _quadTreeData.Dispose();
        }

        // Create a new quadtree and data
        _quadTree = new NativeQuadTree.NativeQuadTree<TreeNode>(_platfornm, Allocator.Persistent);
        CreateAgentsQuadPosition(10);
        var length = _quadtreeStaticElements.Count + _quadAgentsPositions.Count;
        _quadTreeData = new NativeArray<NativeQuadTree.QuadElement<TreeNode>>(length, Allocator.Persistent);
        int index = 0;
        foreach (var staticElement in _quadtreeStaticElements)
        {
          _quadTreeData[index] = staticElement;
          index++;
        }
        foreach (var agentPos in _quadAgentsPositions)
        {
          _quadTreeData[index] = agentPos;
          index++;
        }
        _quadTree.ClearAndBulkInsert(_quadTreeData);
        _quadTreeCreated = true;
      }

      // Update simulation
      foreach (var agent in _agents)
      {
        agent.OnBeforeUpdate();
      }
      //foreach (var collisionAvoider in _collisionListeners)
      //{
      //  collisionAvoider.Update();
      //}
      foreach (var agent in _agents)
      {
        agent.OnAfterUpdate(Vector2.zero);
      }
    }

  }

  private void OnDestroy()
  {
    _quadTree.Dispose();
    _quadTreeData.Dispose();
  }

  /// <summary>
  /// Add collision avoidance algorithm instance to list of listeners.
  /// This listener will receive updating calls
  /// </summary>
  /// <param name="collisionAvoider">Instance to be added</param>
  public void RegisterCollisionListener(IBaseCollisionAvoider collisionAvoider)
  {
    _collisionListeners.Add(collisionAvoider);
  }

  /// <summary>
  /// Add resource handling class to listener list.
  /// This listener will receive updating calls
  /// </summary>
  /// <param name="resourceManager">Intance to be added</param>
  public void RegisterResourceListener(IResourceManager resourceManager)
  {
    _resourceListeners.Add(resourceManager);
  }

  /// <summary>
  /// Getter for agents in simulation
  /// </summary>
  /// <returns>_agents list</returns>
  public List<IBaseAgent> GetAgents()
  {
    return _agents;
  }

  /// <summary>
  /// Create new agent according to current mouse position
  /// </summary>
  private void SpawnAgent()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hitInfo))
    {
      _agents.Add(new BasicGAAgentParallel());
      var agent = _agents[_agents.Count - 1];
      agent.id = _agents.Count;
      //((BaseAgent)agent).SpawnPosition(new Vector2(hitInfo.point.x, hitInfo.point.z));
      ((BaseAgent)agent).SpawnPosition(new Vector2(0, 1));
      //((ORCAAgent)agent).prevPos = new Vector2(hitInfo.point.x, hitInfo.point.z);
      agent.SetDestination(agent.position);
      if (agent is BaseAgent)
      {
        ((BaseAgent)agent).SetName();
      }

      foreach (var collisionAvoider in _collisionListeners)
      {
        collisionAvoider.OnAgentAdded(agent);
      }
    }
  }

  private void RegisterObstacles()
  {
    // Find all NavMeshModifier components in the scene
    NavMeshModifier[] navMeshModifiers = GameObject.FindObjectsOfType<NavMeshModifier>();

    // Iterate over each NavMeshModifier
    foreach (NavMeshModifier modifier in navMeshModifiers)
    {
      if (!modifier.overrideArea)
      {
        // Skip modifiers that don't override the area
        continue;
      }

      // Get the GameObject associated with the NavMeshModifier
      GameObject obj = modifier.gameObject;

      // Ensure the GameObject has a BoxCollider component
      BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
      if (boxCollider == null)
      {
        Debug.LogError("BoxCollider component not found.");
        return;
      }

      // Get the cube's collider bounds
      Bounds bounds = boxCollider.bounds;

      // Calculate cube corners based on collider bounds
      List<Vector2> corners = CalculateCubeCorners(bounds);
      _obstacles.Add(new Obstacle(corners));

      // Do something with the cube corners (e.g., print or process them)
      foreach (Vector2 corner in corners)
      {
        Debug.Log("Cube Corner: " + corner);
      }
    }
  }

  private List<Vector2> CalculateCubeCorners(Bounds bounds)
  {
    Vector2 center = new Vector2(bounds.center.x, bounds.center.z);
    Vector2 extents = new Vector2(bounds.extents.x, bounds.extents.z);

    // Calculate corners based on bounds
    List<Vector2> corners = new List<Vector2>(4)
    {
    new Vector2(center.x - extents.x, center.y - extents.y),
    new Vector2(center.x + extents.x, center.y - extents.y),
    new Vector2(center.x + extents.x, center.y + extents.y),
    new Vector2(center.x - extents.x, center.y + extents.y),
    };

    return corners;
  }

  private void RegisterWalkingPlatform()
  {
    // Find all NavMeshModifier components in the scene
    NavMeshSurface[] navMeshSurfaces = GameObject.FindObjectsOfType<NavMeshSurface>();

    // Iterate over each NavMeshModifier
    foreach (NavMeshSurface surface in navMeshSurfaces)
    {
      // Get the GameObject associated with the NavMeshModifier
      GameObject obj = surface.gameObject;

      // Ensure the GameObject has a BoxCollider component
      BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
      if (boxCollider == null)
      {
        Debug.LogError("BoxCollider component not found.");
        return;
      }

      // Get the cube's collider bounds
      Bounds bounds = boxCollider.bounds;
      _platfornm = new NativeQuadTree.AABB2D(
        new Unity.Mathematics.float2(bounds.center.x, bounds.center.z),
        new Unity.Mathematics.float2(bounds.extents.x, bounds.extents.z)
      );
    }
  }

  private void TransformObstaclesToQuadElements()
  {
    var agentRadius = 0.4f; // make it slightly smaller that actual radius so agent wont be able to slip between obstacle points
    foreach (var obstacle in _obstacles)
    {
      var start = obstacle.vertices[0];
      var verticesCount = obstacle.vertices.Count;

      for (int i = 1; i <= verticesCount; i++)
      {
        var v = new Vector2(obstacle.vertices[i % verticesCount].x - start.x, obstacle.vertices[i % verticesCount].y - start.y);

        // Add node at the end
        _quadtreeStaticElements.Add(new NativeQuadTree.QuadElement<TreeNode>()
        {
          pos = new Unity.Mathematics.float2(obstacle.vertices[i % verticesCount].x, obstacle.vertices[i % verticesCount].y),
          element = new TreeNode()
          {
            staticObstacle = true,
            agentIndex = -1,
            stepIndex = 0

          },
        });

        var vNormalized = v.normalized;
        var vSize = v.magnitude;
        int pointsCount = (int)(vSize / (3 * agentRadius));
        var point = new Vector2(start.x + ((3 * agentRadius) * vNormalized.x), start.y + ((3 * agentRadius) * vNormalized.y));

        // create nodes between star and end
        for(int j = 0; j < pointsCount; j++)
        {
          //create new tree node on point
          _quadtreeStaticElements.Add(new NativeQuadTree.QuadElement<TreeNode>()
          {
            pos = new Unity.Mathematics.float2(point.x, point.y),
            element = new TreeNode()
            {
              staticObstacle = true,
              agentIndex = -1,
              stepIndex = 0

            },
          });

          point = new Vector2(point.x + ((3 * agentRadius) * vNormalized.x), point.y + ((3 * agentRadius) * vNormalized.y));
        }

        start = obstacle.vertices[i % verticesCount];
      }
    }
  }

  private bool IsWithingBounds(NativeQuadTree.AABB2D bounds, Vector2 point)
  {
    return bounds.Contains(new Unity.Mathematics.float2(point.x, point.y));
  }

  /// <summary>
  /// This method creates agents position in quadtree and also its pre-computed position during simulation
  /// Pre-computing is done according to agents current velocity
  /// This method DOESNT check for collision, which means that pre-compution will can be very faulty (e.g. agent can move through other agents and obstacles)
  /// </summary>
  /// <param name="steps">Number of steps to be pre-computed</param>
  private void CreateAgentsQuadPosition(int steps)
  {
    _quadAgentsPositions.Clear();
    foreach (var agent in _agents)
    {
      var pos = agent.position;
      var forward = agent.GetForward().normalized;
      var velocity = forward * agent.speed * _agentUpdateInterval;

      for (int i = 0; i < steps; i++)
      {
        _quadAgentsPositions.Add(new NativeQuadTree.QuadElement<TreeNode>()
        {
          pos = pos,
          element = new TreeNode()
          {
            staticObstacle = false,
            agentIndex = agent.id,
            stepIndex = i
          }
        });

        pos += velocity;
        if (!IsWithingBounds(_platfornm, pos))
        {
          break;
        }
      }
    }
  }

  public NativeQuadTree.NativeQuadTree<TreeNode> GetQuadTree()
  {
    return _quadTree;
  }
}
