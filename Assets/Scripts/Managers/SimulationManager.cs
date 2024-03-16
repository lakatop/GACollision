using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;
using System.Threading;
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
  /// List of scenarios
  /// </summary>
  public List<IScenario> _scenarios { get; set; }
  /// <summary>
  /// Flag determining whether the scenario has already started
  /// </summary>
  private bool _scenarioStarted { get; set; }
  /// <summary>
  /// Index keeping track of which scenario is currently running
  /// </summary>
  private int _scenarioIndex { get; set; }
  /// <summary>
  /// Flag whether we should skip next frame Update. Used when scene is currently in load
  /// </summary>
  private bool _skipNextFrame { get; set; }
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
  /// Gameobject that represents walking platform
  /// </summary>
  private GameObject _platformObject { get; set; }
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
  /// <summary>
  /// Agents destinations in different scenarios that they should swithc to after trigger
  /// </summary>
  private List<Vector2> _agentsScenarioDestinations { get; set; }

  void Awake()
  {
    System.Console.WriteLine("SimulationManager Awake call");
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    //else
    //{
    //  Instance = this;
    //}
    Instance = this;
    DontDestroyOnLoad(gameObject);

    _agents = new List<IBaseAgent>();
    _obstacles = new List<Obstacle>();
    _collisionListeners = new List<IBaseCollisionAvoider>();
    _collisionManager = new AlgorithmsManager();
    _resourceListeners = new List<IResourceManager>();
    _quadtreeStaticElements = new List<NativeQuadTree.QuadElement<TreeNode>>();
    _quadAgentsPositions = new List<NativeQuadTree.QuadElement<TreeNode>>();
    _agentUpdateInterval = 0.5f;
    _quadTreeCreated = false;
    _agentsScenarioDestinations = new List<Vector2>();
    _scenarioStarted = false;
    _scenarioIndex = 0;
    _skipNextFrame = true;
  }

  void Start()
  {
    System.Console.WriteLine("SimulationManager StartCall");
    CreateScenarios();
    SetScenarioResources();
  }

  /// <summary>
  /// Called every simulation step
  /// Handles switching to other scenes if scenario in current one has finished
  /// Updates simulation
  /// </summary>
  void Update()
  {
    System.Console.WriteLine("SimulationManager Update call");
    if (_skipNextFrame)
    {
      _skipNextFrame = false;
      return;
    }

    if (!_scenarioStarted)
    {
      _scenarios[_scenarioIndex].SetupScenario(_agents);
      _scenarios[_scenarioIndex].runCounter--;
      SetScenarioResources();
      CreateQuadtreeAndData();
      _scenarioStarted = true;
      // Wait one more frame so that objects get spawned
      return;
    }

    _updateTimer += Time.deltaTime;

    if (_scenarioStarted && AllAgentsFinished())
    {
      ClearScenarioResources();

      if (ShouldRepeatScenario())
      {
        _scenarioStarted = false;
        _skipNextFrame = true;
        _updateTimer = 0f;
      }
      else if (IsThereNextScenario())
      {
        SetNextScenario();
        _updateTimer = 0f;
      }
      else
      {
        // Ideally end application, but it seems iOS has some troubles with that
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Unload();
      }
    }
    else
    {
      RunSimulation();
    }
  }

  private void RunSimulation()
  {
    System.Console.WriteLine("RUNNING RunSimulation");
    _scenarioStarted = true;

    if (_updateTimer > _agentUpdateInterval)
    {
      foreach (var agent in _agents)
      {
        Debug.Log("In destination: " + agent.inDestination);
        agent.OnAfterUpdate(Vector2.zero);
      }

      _updateTimer = 0f;

      // Dispose previous quadtree and its data
      if (_quadTreeCreated)
      {
        _quadTree.Dispose();
        _quadTreeData.Dispose();
      }

      CreateQuadtreeAndData();
    }

    // Update simulation
    foreach (var agent in _agents)
    {
      agent.OnBeforeUpdate();
      Debug.Log("In destination: " + agent.inDestination);
    }
    //foreach (var collisionAvoider in _collisionListeners)
    //{
    //  collisionAvoider.Update();
    //}
    foreach (var agent in _agents)
    {
      agent.OnAfterUpdate(Vector2.zero);
      Debug.Log("In destination: " + agent.inDestination);
    }
  }

  private void OnDestroy()
  {
    if (_quadTreeCreated)
    {
      _quadTree.Dispose();
      _quadTreeData.Dispose();
    }
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
      CreateScenarios();
    }
  }

  private void RegisterObstacles()
  {
    _obstacles.Clear();
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
        //Debug.LogError("BoxCollider component not found.");
        continue;
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

      _platformObject = obj;

      // Get the cube's collider bounds
      Bounds bounds = boxCollider.bounds;
      _platfornm = new NativeQuadTree.AABB2D(
        new Unity.Mathematics.float2(bounds.center.x, bounds.center.z),
        new Unity.Mathematics.float2(bounds.extents.x, bounds.extents.z)
      );
    }
  }

  /// <summary>
  /// Transforms obstacles to quadtree elements that are later tested for collision.
  /// Currently suports only rectangular shapes
  /// </summary>
  private void TransformObstaclesToQuadElements()
  {
    var agentRadius = 0.5f; // make it slightly smaller that actual radius so agent wont be able to slip between obstacle points
    _quadtreeStaticElements.Clear();
    foreach (var obstacle in _obstacles)
    {
      var start = obstacle.vertices[0];
      var end = obstacle.vertices[1];
      var thirdCorner = obstacle.vertices[3];
      var distance = Mathf.Abs(start.y - thirdCorner.y);
      var point = new Vector2(start.x + agentRadius, start.y + agentRadius);

      for(int i = 0; i < distance; i++)
      {
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
        PathDrawer.DrawCircle(point, agentRadius);

        point.x += 2 * agentRadius;
        while(point.x <= (end.x - agentRadius))
        {
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
          PathDrawer.DrawCircle(point, agentRadius);
          point.x += 2 * agentRadius;
        }

        point.x = start.x + agentRadius;
        point.y += 2 * agentRadius;
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
  /// This method DOESNT check for collision, which means that pre-compution can be very faulty (e.g. agent can move through other agents and obstacles)
  /// </summary>
  /// <param name="steps">Number of steps to be pre-computed</param>
  private void CreateAgentsQuadPosition(int steps)
  {
    var agentRadius = 0.5f;
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

        // Create intersteps of agent between updates
        var interSteps = Mathf.Ceil((velocity.magnitude - (2 * agentRadius))/ (agentRadius * 2));
        var interPos = pos;
        for (int j = 0; j < (int)interSteps; j++)
        {
          interPos += (velocity.normalized * 2 * agentRadius);
          if (!IsWithingBounds(_platfornm, interPos))
          {
            break;
          }
          _quadAgentsPositions.Add(new NativeQuadTree.QuadElement<TreeNode>()
          {
            pos = interPos,
            element = new TreeNode()
            {
              staticObstacle = false,
              agentIndex = agent.id,
              stepIndex = i
            }
          });
        }

        pos += velocity;
        if (!IsWithingBounds(_platfornm, pos))
        {
          break;
        }
      }
    }
  }

  /// <summary>
  /// Returns whether all agents are in their final destination
  /// </summary>
  /// <returns>Whether all agents are in their final destination</returns>
  private bool AllAgentsFinished()
  {
    bool finished = true;
    foreach (var agent in _agents)
    {
      finished &= agent.inDestination;
    }

    return finished;
  }

  private void CreateScenarios()
  {
    System.Console.WriteLine("Creating scenarios");
    _scenarios = new List<IScenario>
    {
      new StraightLineScenario(1),
      new SmallObstacleScenario(1),
      new CornerScenario(1),
      new OppositeScenario(1),
      new OppositeMultipleScenario(1),
      new OppositeCircleScenario(1),
      new NarrowCoridorTurnAroundScenario(1),
      new NarrowCoridorOppositeScenario(1),
      new NarrowCoridorsOppositeNoNavmeshScenario(1)
    };
  }

  private bool IsThereNextScenario()
  {
    return _scenarioIndex < (_scenarios.Count - 1);
  }

  private bool ShouldRepeatScenario()
  {
    return _scenarios[_scenarioIndex].runCounter > 0;
  }

  private void  SetNextScenario()
  {
    var nextSceneIndex = _scenarioIndex + 1;

    if (nextSceneIndex >= _scenarios.Count)
      return;
    SceneManager.LoadScene(nextSceneIndex);
    _scenarios[nextSceneIndex].runCounter--;
    _scenarioStarted = false;
    _skipNextFrame = true;
    _scenarioIndex++;
  }

  private void SetScenarioResources()
  {
    RegisterObstacles();
    RegisterWalkingPlatform();
    TransformObstaclesToQuadElements();
  }

  private void CreateQuadtreeAndData()
  {
    _quadTree = new NativeQuadTree.NativeQuadTree<TreeNode>(_platfornm, Allocator.Persistent);
    CreateAgentsQuadPosition(7);
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

  private void ClearScenarioResources()
  {
    _scenarios[_scenarioIndex].ClearScenario(_agents);

    foreach (var agent in _agents)
    {
      Destroy(((BaseAgent)agent)._object);
    }
    _agents.Clear();
    _obstacles.Clear();
    _quadtreeStaticElements.Clear();
    if (_quadTreeCreated)
    {
      _quadTree.Dispose();
      _quadTreeData.Dispose();
      _quadTreeCreated = false;
    }
  }

  /// <summary>
  /// Getter for _quadTree
  /// </summary>
  /// <returns>SimulationManager's _quadTree</returns>
  public NativeQuadTree.NativeQuadTree<TreeNode> GetQuadTree()
  {
    return _quadTree;
  }

  /// <summary>
  /// Getter for platform object
  /// </summary>
  /// <returnsSimulaitonManager's _platformObject></returns>
  public GameObject GetPlatform()
  {
    return _platformObject;
  }
}
