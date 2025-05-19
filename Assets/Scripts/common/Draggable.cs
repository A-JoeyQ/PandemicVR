using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Draggable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  private bool QDragging = false;
  private Vector2 myOffset;
  private RectTransform myParentRT;
  private RectTransform myRT;

  void Awake()
  {
    myParentRT = transform.parent as RectTransform;
    myRT = transform as RectTransform;
  }

  public void OnPointerDown( PointerEventData data )
  {
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
      myRT, data.position, data.pressEventCamera, out myOffset );
    QDragging = true;
  }

  public void OnBeginDrag( PointerEventData data )
  {

  }

  public void OnDrag( PointerEventData data )
  {
    if( !QDragging )
      return;

    Vector2 localPointerPosition;
    if( RectTransformUtility.ScreenPointToLocalPointInRectangle(
        myParentRT, data.position, data.pressEventCamera, out localPointerPosition
    ) )
    {
      myRT.localPosition = localPointerPosition - myOffset;
    }
  }

  public void OnEndDrag( PointerEventData data )
  {
    QDragging = false;
  }
}