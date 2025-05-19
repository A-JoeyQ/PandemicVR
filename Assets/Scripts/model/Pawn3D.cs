using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Oculus.Interaction; // Add this for Meta Interaction SDK
public class Pawn3D : MonoBehaviour
{

    private Canvas citiesCanvas;
    private Canvas canvas;
    public Camera vrCamera;
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        try{
            citiesCanvas = GameGUI.gui.CityCanvas.GetComponent<Canvas>();
        }
        catch{
            Debug.Log("CityCanvas not found");
        }
        GraphicRaycaster raycaster = citiesCanvas.GetComponent<GraphicRaycaster>();

        List<RaycastResult> results = new List<RaycastResult>();
        Vector2 screenPosition = vrCamera.WorldToScreenPoint(transform.localPosition);
        //Debug.Log("Screen from position: " + vrCamera.WorldToScreenPoint(transform.position) + " screen from localposition: " + vrCamera.WorldToScreenPoint(transform.localPosition));
        RectTransform canvasRectTransform = citiesCanvas.GetComponent<RectTransform>();
        //Ray cast from the object position
        Vector2 localPointerPosition;
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        raycaster.Raycast(eventData, results);
        Debug.Log("Raycast results: " + results.Count);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.name == "Image")
            {
                this.gameObject.transform.position = result.gameObject.transform.position;
            }
        }
        
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out localPointerPosition)){
            Debug.Log("Raycasting with localPointerPosition: " + localPointerPosition + " and screenPosition: " + screenPosition + " and regular position: " + transform.position + " and local position: " + transform.localPosition);
        }
    }
}
