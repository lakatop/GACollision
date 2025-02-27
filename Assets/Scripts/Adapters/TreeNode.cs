/// <summary>
/// Struct representing object in quadTree
/// </summary>
public struct TreeNode
{
  /// <summary>
  /// Flag whether this object is static obstacle. If false, object represents agent
  /// </summary>
  public bool staticObstacle { get; set; }
  /// <summary>
  /// Id of agent. Only valid if object is agent
  /// </summary>
  public int agentIndex { get; set; }
  /// <summary>
  /// Step index of agent. Only valid if object is agent
  /// </summary>
  public int stepIndex { get; set; }
}