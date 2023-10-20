using Tags;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollapseUI : MonoBehaviour, IPointerClickHandler
{
  private bool _isCollapsed = false;
  private GameObject _frame = null;
  private GameObject _body = null;
  private GameObject _footer = null;
  private GameObject _header = null;
  private GameObject _headerTabs = null;

  // This needs to be referenced in editor because title is disabled on start
  // and there is no non-hacky way to get its refence via scripts
  public GameObject _headerTitle;

  public void Awake()
  {
    _frame = GameObject.FindGameObjectWithTag(UI.kFrame);
    _body = GameObject.FindGameObjectWithTag(UI.kBody);
    _footer = GameObject.FindGameObjectWithTag(UI.kFooter);
    _header = GameObject.FindGameObjectWithTag(UI.kHeader);
    _headerTabs = GameObject.FindGameObjectWithTag(UI.kHeaderTabs);
  }

  private void ToggleCollapsibleElement()
  {
    _isCollapsed = !_isCollapsed;

    if (_isCollapsed)
    {
      _headerTabs.SetActive(false);
      _headerTitle.SetActive(true);
      _header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
      _body.SetActive(false);
      _footer.SetActive(false);
      _frame.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
      transform.Rotate(0, 0, 90); // Rotate arrow toggle image
    }
    else
    {
      _headerTabs.SetActive(true);
      _headerTitle.SetActive(false);
      _header.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
      _body.SetActive(true);
      _footer.SetActive(true);
      _frame.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
      transform.Rotate(0, 0, -90); // Rotate arrow toggle image
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    ToggleCollapsibleElement();
  }
}
