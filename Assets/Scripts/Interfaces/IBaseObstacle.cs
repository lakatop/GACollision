public interface IBaseObstacle
{
  IBaseObstacle next { get; set; }
  IBaseObstacle previous { get; set; }
  RVO.Vector2 direction { get; set; }
  RVO.Vector2 point { get; set; }
  int id { get; set; }
  bool convex { get; set; }
}