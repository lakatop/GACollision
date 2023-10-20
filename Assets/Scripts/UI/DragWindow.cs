using UnityEngine;
using UnityEngine.EventSystems;

public class DragWindow : MonoBehaviour, IDragHandler
{
  [SerializeField] private RectTransform _dragRectTransform;
  [SerializeField] private Canvas _canvas;

  public void Awake()
  {
    if (!_dragRectTransform)
    {
      _dragRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    if (!_canvas)
    {
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
  }

  public void OnDrag(PointerEventData eventData)
  {
    _dragRectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
  }
}
