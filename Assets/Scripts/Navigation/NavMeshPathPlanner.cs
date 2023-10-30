using UnityEngine.AI;

/// <summary>
/// Concrete class for path planning
/// Uses UnityEngine.AI.NavMeshAgent for all calculations
/// </summary>
public class NavMeshPathPlanner: IBasePathPlanner
{
  private IBaseAgent _agent = null;
  private NavMeshAgent _navMeshAgent = null;

  public NavMeshPathPlanner(IBaseAgent agent)
  {
    _agent = agent;
    if (_agent is MyNavMeshAgent)
    {
      _navMeshAgent = ((MyNavMeshAgent)_agent).GetComponent<NavMeshAgent>();
    }
  }

  /// <inheritdoc cref="IBasePathPlanner.OnDestinationChange"/>
  public void OnDestinationChange()
  {
    RecalculatePath();
  }

  /// <inheritdoc cref="IBasePathPlanner.Update"/>
  public void Update()
  {
    return;
  }

  /// <summary>
  /// Recalculates path according to agents destination
  /// </summary>
  private void RecalculatePath()
  {
    if (!_navMeshAgent)
      return;

    _navMeshAgent.SetDestination(_agent.destination);
  }
}

