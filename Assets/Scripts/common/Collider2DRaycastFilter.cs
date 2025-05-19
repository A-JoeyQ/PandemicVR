using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( RectTransform ), typeof( Collider2D ) )]
public class Collider2DRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
  Collider2D myCollider;
  RectTransform myRectTransform;

  void Awake()
  {
    myCollider = GetComponent<Collider2D>();
    myRectTransform = GetComponent<RectTransform>();
  }

  public bool IsRaycastLocationValid( Vector2 screenPos, Camera eventCamera )
  {
    var worldPoint = Vector3.zero;
    var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(
        myRectTransform, screenPos, eventCamera,
        out worldPoint );
    if( isInside )
      isInside = myCollider.OverlapPoint( worldPoint );

    return isInside;
  }
}