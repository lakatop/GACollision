using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;


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
  public List<Obstacle> obstacles { get; private set; }
  /// <summary>
  /// List of scenarios
  /// </summary>
  public List<IScenario> scenarios { get; set; }
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
  /// <summary>
  /// Boolean flag wheter _quadTree was created
  /// </summary>
  private bool _quadTreeCreated { get; set; }
  /// <summary>
  /// Data to fill _quadTree with
  /// </summary>
  private NativeArray<NativeQuadTree.QuadElement<TreeNode>> _quadTreeData;
  /// <summary>
  /// Delta t - determines how often agent calculates new position
  /// </summary>
  public float agentUpdateInterval { get; private set; }
  /// <summary>
  /// Temporary Time.deltaTime accumulator
  /// </summary>
  private float _updateTimer { get; set; }

  /// <summary>
  /// Called as a first method for this component - perform initialisation
  /// </summary>
  void Awake()
  {
    System.Console.WriteLine("SimulationManager Awake call");
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    _agents = new List<IBaseAgent>();
    obstacles = new List<Obstacle>();
    _quadtreeStaticElements = new List<NativeQuadTree.QuadElement<TreeNode>>();
    _quadAgentsPositions = new List<NativeQuadTree.QuadElement<TreeNode>>();
    agentUpdateInterval = 0.5f;
    _quadTreeCreated = false;
    _scenarioStarted = false;
    _scenarioIndex = 0;
    _skipNextFrame = true;
  }

  /// <summary>
  /// Called on component start after Awake method
  /// </summary>
  void Start()
  {
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
    if (_skipNextFrame)
    {
      _skipNextFrame = false;
      return;
    }

    if (!_scenarioStarted)
    {
      scenarios[_scenarioIndex].SetupScenario(_agents);
      scenarios[_scenarioIndex].runCounter--;
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
        // When playing via editor, this is how we quit the simulation (but keep unity running)
        UnityEditor.EditorApplication.isPlaying = false;
      }
    }
    else
    {
      RunSimulation();
    }
  }

  /// <summary>
  /// Update agents and data related to their update
  /// </summary>
  private void RunSimulation()
  {
    _scenarioStarted = true;

    if (_updateTimer > agentUpdateInterval)
    {
      foreach (var agent in _agents)
      {
        agent.OnAfterUpdate();
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
    }

    foreach (var agent in _agents)
    {
      agent.OnAfterUpdate();
    }
  }

  /// <summary>
  /// Called when this component is detroyed
  /// </summary>
  private void OnDestroy()
  {
    if (_quadTreeCreated)
    {
      _quadTree.Dispose();
      _quadTreeData.Dispose();
    }
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
  /// Register obstacles present in current simulation scenario
  /// </summary>
  private void RegisterObstacles()
  {
    obstacles.Clear();
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
      obstacles.Add(new Obstacle(corners));

      // Do something with the cube corners (e.g., print or process them)
      foreach (Vector2 corner in corners)
      {
        Debug.Log("Cube Corner: " + corner);
      }
    }
  }

  /// <summary>
  /// Calculate corners of rectangle based on its bounds
  /// </summary>
  /// <param name="bounds">Bounds of rectangle</param>
  /// <returns>List of rectangles corners</returns>
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

  /// <summary>
  /// Register walkable platform in current simulation
  /// </summary>
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
    var agentRadius = 0.5f;
    _quadtreeStaticElements.Clear();
    foreach (var obstacle in obstacles)
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

        // For debug drawing uncomment the following line
        //PathDrawer.DrawCircle(point, agentRadius);

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

          // For debug drawing uncomment the following line
          //PathDrawer.DrawCircle(point, agentRadius);
          point.x += 2 * agentRadius;
        }

        point.x = start.x + agentRadius;
        point.y += 2 * agentRadius;
      }
    }
  }

  /// <summary>
  /// Check whether point is within given bounds
  /// </summary>
  /// <param name="bounds">Bounds in which we want to check</param>
  /// <param name="point">Point for checking</param>
  /// <returns>True if point is within bounds, false otherwise</returns>
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
      var velocity = forward * agent.speed * agentUpdateInterval;

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

  /// <summary>
  /// Populate scenarios list
  /// </summary>
  private void CreateScenarios()
  {
    System.Console.WriteLine("Creating scenarios");
    scenarios = new List<IScenario>
    {
      new StraightLineScenario(20),
      new SmallObstacleScenario(20),
      new CornerScenario(20),
      new OppositeScenario(20),
      new OppositeMultipleScenario(20),
      new OppositeCircleScenario(20),
      new NarrowCoridorsOppositeNoNavmeshScenario(20),
      new NarrowCoridorOppositeScenario(20)
    };
  }

  /// <summary>
  /// Check if there is next scenario
  /// </summary>
  /// <returns>True if we have next scenario in list, false otherwise</returns>
  private bool IsThereNextScenario()
  {
    return _scenarioIndex < (scenarios.Count - 1);
  }

  /// <summary>
  /// Check if we should repeat current scenario
  /// </summary>
  /// <returns>True if we should repeat current scenario, false otherwise</returns>
  private bool ShouldRepeatScenario()
  {
    return scenarios[_scenarioIndex].runCounter > 0;
  }

  /// <summary>
  /// Sets next scenario
  /// </summary>
  private void  SetNextScenario()
  {
    var nextSceneIndex = _scenarioIndex + 1;

    if (nextSceneIndex >= scenarios.Count)
      return;
    SceneManager.LoadScene(nextSceneIndex);
    scenarios[nextSceneIndex].runCounter--;
    _scenarioStarted = false;
    _skipNextFrame = true;
    _scenarioIndex++;
  }

  /// <summary>
  /// Sets resources needed for current scenario
  /// </summary>
  private void SetScenarioResources()
  {
    RegisterObstacles();
    RegisterWalkingPlatform();
    TransformObstaclesToQuadElements();
  }

  /// <summary>
  /// Create new _quadTree and populate it with data of current scenario
  /// </summary>
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

  /// <summary>
  /// Clear resources of current scenario
  /// </summary>
  private void ClearScenarioResources()
  {
    scenarios[_scenarioIndex].ClearScenario(_agents);

    foreach (var agent in _agents)
    {
      Destroy(((BaseAgent)agent).GetGameObject());
    }
    _agents.Clear();
    obstacles.Clear();
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
