using System.Collections.Generic;
using System;

/// <summary>
/// Manages algorithms that require single instance.
/// Usually that instance is responsible for running simulation on every agent
/// and keeps global state (e.g. positions, velocities, ...)
/// </summary>
public class AlgorithmsManager
{
  private List<IBaseCollisionAvoider> _collisionAlgorithms { get; set; }
  private GeneticAlgorithm _geneticAlgorithm { get; set; }

  public AlgorithmsManager()
  {
    _collisionAlgorithms = new List<IBaseCollisionAvoider>();
    _geneticAlgorithm = null;
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

  public GeneticAlgorithm GetOrCreateGeneticAlgorithm()
  {
    if (_geneticAlgorithm == null)
    {
      _geneticAlgorithm = new GeneticAlgorithm();
    }

    return _geneticAlgorithm;
  }

 }