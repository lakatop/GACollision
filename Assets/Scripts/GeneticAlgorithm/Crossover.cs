using Unity.Collections;
using Unity.Burst;


[BurstCompile]
public struct UniformBezierCrossOperatorParallel : IParallelPopulationModifier<BezierIndividualStruct>
{
  [ReadOnly] public Unity.Mathematics.Random rand;
  public NativeArray<BezierIndividualStruct> parents;
  [ReadOnly] public float crossProb;

  public void ModifyPopulation(ref NativeArray<BezierIndividualStruct> currentPopulation, int iteration)
  {
    for (int i = 0; i < currentPopulation.Length - 1; i += 2)
    {
      var crossProb = rand.NextFloat();
      // Do cross only with small probability
      if (crossProb > this.crossProb)
        return;

      var parent1 = currentPopulation[i];
      var nextParentIndex = rand.NextInt(currentPopulation.Length);
      while (nextParentIndex == i)
      {
        nextParentIndex = rand.NextInt(currentPopulation.Length);
      }

      parents[0] = currentPopulation[i];
      parents[1] = currentPopulation[nextParentIndex];

      UnityEngine.Vector2 P1 = UnityEngine.Vector2.zero;
      UnityEngine.Vector2 P2 = UnityEngine.Vector2.zero;



      int prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P1.x = parents[prob].bezierCurve.points[1].x;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P1.y = parents[prob].bezierCurve.points[1].y;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P2.x = parents[prob].bezierCurve.points[2].x;
      prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
      P2.y = parents[prob].bezierCurve.points[2].y;

      for (int j = 0; j < parent1.accelerations.Length; j++)
      {
        prob = (int)System.Math.Round(rand.NextFloat(), System.MidpointRounding.AwayFromZero);
        parent1.accelerations[j] = parents[prob].accelerations[j];
      }

      parent1.bezierCurve.points[1] = P1;
      parent1.bezierCurve.points[2] = P2;
      currentPopulation[i] = parent1;
    }
  }

  public string GetComponentName()
  {
    return GetType().Name;
  }

  public float GetCrossProbability()
  {
    return crossProb;
  }

  public void Dispose()
  {
    parents.Dispose();
  }
}
