using System.Collections.Generic;
using System;

public class CollisionManager
{
  private List<IBaseCollisionAvoider> _collisionAlgorithms { get; set; }

  public CollisionManager()
  {
    _collisionAlgorithms = new List<IBaseCollisionAvoider>();
  }

  public IBaseCollisionAvoider GetOrCreateCollisionAlg<T>(Func<T> ctor) where T: IBaseCollisionAvoider, new()
  {
    foreach (var col in _collisionAlgorithms)
    {
      if (typeof(T) == col.GetType())
      {
        return col;
      }
    }

    _collisionAlgorithms.Add(ctor());
    return _collisionAlgorithms[_collisionAlgorithms.Count - 1];
  }

 }