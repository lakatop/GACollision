/// <summary>
/// Interface for managing resources.
/// </summary>
public interface IResourceManager
{
  /// <summary>
  /// Called every frame before other updates
  /// </summary>
  void OnBeforeUpdate();
  /// <summary>
  /// Called every frame after other updates
  /// </summary>
  void OnAfterUpdate();
}
