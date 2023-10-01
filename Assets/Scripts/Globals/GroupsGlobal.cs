using System;
using UnityEngine;

public class GroupsGlobal : MonoBehaviour
{
  [Serializable]
  public class MyCls
  {
    public GameObject spawn;
    public GameObject destination;
    public int size;
  }
  public MyCls[] groups;
}
