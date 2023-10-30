public interface IBasePathPlanner
{
  /// <summary>
  /// Update function called every simulation step
  /// </summary>
  void Update();
  /// <summary>
  /// Designed to be called when destination has been changed
  /// </summary>
  void OnDestinationChange();
}