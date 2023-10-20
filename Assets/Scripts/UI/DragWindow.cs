using UnityEngine;
using UnityEngine.EventSystems;

public class DragWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
  private GameObject _frame = null;
  private Canvas _canvas = null;

  public void Awake()
  {
    _frame = GameObject.FindGameObjectWithTag(GlobStrings.Tags.UI.kFrame);

    Transform nextParent = transform.parent;
    while (nextParent != null)
    {
      _canvas = nextParent.GetComponent<Canvas>();
      if (_canvas != null)
      {
        break;
      }
      nextParent = nextParent.parent;
    }
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    _frame.GetComponent<CanvasGroup>().alpha = 0.5f;
  }

  public void OnDrag(PointerEventData eventData)
  {
    _frame.GetComponent<RectTransform>().anchoredPosition += eventData.delta / _canvas.scaleFactor;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    _frame.GetComponent<CanvasGroup>().alpha = 1f;
  }
}
