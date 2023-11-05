using System;

public class StaticObstacle : IBaseObstacle
{
  public StaticObstacle()
  {
  }

  public IBaseObstacle next { get; set; }
  public IBaseObstacle previous { get; set; }
  public RVO.Vector2 direction { get; set; }
  public RVO.Vector2 point { get; set; }
  public int id { get; set; }
  public bool convex { get; set; }
}

