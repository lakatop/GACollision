using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing obstacle in space
/// </summary>
public class Obstacle
{
  public List<Vector2> vertices { get; set; }

  public Obstacle(List<Vector2> vertices)
  {
    this.vertices = vertices;
  }
}
