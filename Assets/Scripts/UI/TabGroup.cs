using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroupUI : MonoBehaviour
{
  public List<TabButtonUI> tabButtons;
  private TabButtonUI _selectedTab;
  public List<GameObject> objectsToSwap;

  public void Subscribe(TabButtonUI button)
  {
    if (tabButtons == null)
    {
      tabButtons = new List<TabButtonUI>();
    }

    tabButtons.Add(button);
  }

  public void OnTabEnter(TabButtonUI button)
  {

  }

  public void OnTabExit(TabButtonUI button)
  {

  }

  public void OnTabSelected(TabButtonUI button)
  {
    _selectedTab = button;
    int index = button.transform.GetSiblingIndex();
    for (int i =0; i < objectsToSwap.Count; i++)
    {
      if (i == index)
      {
        objectsToSwap[i].SetActive(true);
      }
      else
      {
        objectsToSwap[i].SetActive(false);
      }
    }
  }
}
