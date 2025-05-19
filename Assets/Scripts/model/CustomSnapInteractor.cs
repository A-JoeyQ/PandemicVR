using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class CustomSnapInteractor : SnapInteractor
{
    public UnityEvent<SnapInteractable> OnObjectSnapped;
    public UnityEvent<SnapInteractable> OnObjectUnsnapped;

    private PawnMoveable pawn;
    private SnapInteractable defaultInteractable = null;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        pawn = GetComponentInParent<PawnMoveable>();
        base.OnEnable();
        if (_started)
        {
            if (defaultInteractable != null)
            {
                base.SetComputeCandidateOverride(() => defaultInteractable, true);
                base.SetComputeShouldSelectOverride(()=>true, true);
            }
        }
    }
    
    protected override void InteractableSelected(SnapInteractable interactable)
    {
        //Not running this causes the object to not snap but the snap is still initiated and it continues to try to snap to this city
        //e.g The function is continuously called --> This solution does not cancel the snap but it does not snap to the city
        Debug.Log("OnSnapCall : " + interactable == null ? "Is null" : " Not Null");
        base.InteractableSelected(interactable);
        if (interactable != null)
        {
            
            OnObjectSnapped?.Invoke(interactable);
        }
    }
    protected override void InteractableUnselected(SnapInteractable interactable)
    {
        base.InteractableUnselected(interactable);

        if (interactable != null)
        {
            OnObjectUnsnapped?.Invoke(interactable);
        }
    }
    protected override SnapInteractable ComputeCandidate(){
        //Good stuff the distance check should happen here.
        SnapInteractable interactable = base.ComputeCandidate();
        if(interactable != null)
        {
            City city = interactable.GetComponentInParent<City>();
            if (city != null)
            {
                //If the pawn cannot move to the city, return null as to not accept the snap location
                if (!pawn.CanMoveToCity(city))
                {
                    return null;
                }
            }
        }
        return interactable;
    }

    /// <summary>
    /// Sets a timeout interactable and timeout time.
    /// </summary>
    public void SetTimeOutInteractable(SnapInteractable interactable, float time = 0.0f)
    {
        City city = interactable.GetComponentInParent<City>();
        if(city != null)
        {
            Debug.Log("Setting timeout interactable to city: " + city.city.cityName);
        }
        base.InjectOptionalTimeOutInteractable(interactable);
        base.InjectOptionaTimeOut(time);
    }

    public void SetDefaultInteractable(SnapInteractable interactable)
    {
        defaultInteractable = interactable;
    }

    //Should set the default on startup of the pawn as that is the default snap location for the pawn
}
