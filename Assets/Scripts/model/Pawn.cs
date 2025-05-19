using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Oculus.Interaction;
using static GameGUI;
using static Game;
using Fusion;

public class Pawn : MonoBehaviour, IPointerClickHandler
{

    public Player PlayerModel;
    public int playerPosition { get; set; }

    public CustomSnapInteractor snapInteractor = null;
    public bool CanMove { get; set; } = false;

    public bool IsInterfaceElement { get; set; } = false;

    private int initialCityID { get; set; }

    private City endedInCity = null;

    public Player.Roles PawnRole { get; set; }
    
    // Called when the pawn is released and snapped to a city
    private void SnappedToCity(SnapInteractable interactable)
    {
        City endedInCity = interactable.GetComponentInParent<City>();
        PlayerGUI pGUI;
        if (theGame.MobileHospitalPlayer != null && theGame.MobileHospitalPlayer.playerGui.callToMobilizePending)
        {
            pGUI = PlayerPadForPlayer(theGame.MobileHospitalPlayer);
        }
        else
        {
            pGUI = PlayerPadForPlayer(PlayerModel);
        }

        if (endedInCity != null && endedInCity.city.cityID != PlayerModel.GetCurrentCity())
        {
            Debug.Log("Snapped to city: " + endedInCity.city.cityName);
            if (pGUI.ActionSelected == ActionTypes.Charter)
            {
                Timeline.theTimeline.AddEvent(new PCharterEvent(endedInCity));
                Destroy(gameObject);
            }
            else
            {
                int distance = theGame.DistanceFromCity(PlayerModel.GetCurrentCity(), endedInCity.city.cityID);
                if (pGUI.pInEvent == EventState.CALLTOMOBILIZE)
                {
                    if(distance > 0 && distance <= 2)
                    {
                        // check the distance from the city (<=2) when doing a call to mobilize
                        Timeline.theTimeline.AddEvent(new PMobilizeEvent(PlayerModel, endedInCity.city.cityID));
                        Destroy(gameObject);
                    }
                }
                else if (pGUI.ActionSelected == ActionTypes.Move)
                {
                    if (distance > 0 && distance <= pGUI.PlayerModel.ActionsRemaining)
                    {
                        Timeline.theTimeline.AddEvent(new PMoveEvent(endedInCity.city.cityID, distance));
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    public bool CanMoveToCity(City possibleCity)
    {
        PlayerGUI pGUI;
        if (theGame.MobileHospitalPlayer != null && theGame.MobileHospitalPlayer.playerGui.callToMobilizePending)
        {
            pGUI = PlayerPadForPlayer(theGame.MobileHospitalPlayer);
        }
        else
        {
            pGUI = PlayerPadForPlayer(PlayerModel);
        }
        
        if (possibleCity != null) //For the check moving to same city is okay!
        {
            if (pGUI.ActionSelected == ActionTypes.Charter)
            {
                return true;
            }
            else
            {
                int distance = theGame.DistanceFromCity(PlayerModel.GetCurrentCity(), possibleCity.city.cityID);
                if (pGUI.pInEvent == EventState.CALLTOMOBILIZE)
                {
                    if(distance >= 0 && distance <= 2) // Distance >= 0  for the check, >0 is handled when checking if the action is to be accepted
                    {
                        // check the distance from the city (<=2) when doing a call to mobilize
                        return true;
                    }
                    else return false;
                }
                else if (pGUI.ActionSelected == ActionTypes.Move)
                {
                    if (distance >= 0 && distance <= pGUI.PlayerModel.ActionsRemaining)
                    {
                        return true;
                    }
                    else return false;
                }
                return false;
            }
        }
        else return false;
    }

    internal void SetRoleAndPlayer(Player player)
    {
        
        PawnRole = player.Role;
        PlayerModel = player;

        initialCityID = PlayerModel.GetCurrentCity();
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        Image image = GetComponent<Image>();
        if(renderer != null)
        {
            renderer.material.color = gui.roleCards[(int)PawnRole].roleColor;
        }else if(image != null){
            image.color = gui.roleCards[(int)PawnRole].roleColor;
        }
    }

    public void SetMoveable(bool moveable)
    {
        if(snapInteractor == null) return;

        CanMove = moveable;
        snapInteractor.gameObject.SetActive(moveable);
        if(moveable){
            snapInteractor.OnObjectSnapped.AddListener(SnappedToCity);

            SnapInteractable initialInteractable = PlayerModel.GetCurrentCityScript().GetComponentInChildren<SnapInteractable>();
            snapInteractor.SetDefaultInteractable(initialInteractable);
            snapInteractor.SetTimeOutInteractable(initialInteractable);
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInterfaceElement)
        {
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            if(PlayerModel != null)
                NetworkPlayer.LocalInstance.RPC_PawnClicked(PawnRole, PlayerModel.Position);
        }
            
            //theGame.CurrentPlayer.playerGui.PawnClicked(this);
    }
}
