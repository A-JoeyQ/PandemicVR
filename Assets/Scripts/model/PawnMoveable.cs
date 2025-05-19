using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Oculus.Interaction;
using static GameGUI;
using static Game;
using Fusion;

public class PawnMoveable : NetworkBehaviour, IPointerClickHandler
{

    public Player PlayerModel;
    [Networked, OnChangedRender(nameof(OnPlayerChanged))]
    public int playerPosition { get; set; } = -1;

    public CustomSnapInteractor snapInteractor = null;
    [Networked]
    public bool CanMove { get; set; } = false;

    [Networked]
    public bool IsInterfaceElement { get; set; } = false;

    [Networked]
    private int initialCityID { get; set; }

    private City endedInCity = null;

    [Networked, OnChangedRender(nameof(OnRoleChanged))]
    public Player.Roles PawnRole { get; set; }


    public override void Spawned()
    {
        base.Spawned();
        OnPlayerChanged();
        OnRoleChanged();
        SetMoveable(CanMove);
    }
    public void OnPlayerChanged()
    {
        Debug.Log("Player changed");
        PlayerModel = GameGUI.PlayerPadForPosition(playerPosition).PlayerModel;
        PawnRole = PlayerModel.Role;
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        Image image = GetComponent<Image>();
        if (renderer != null)
        {
            renderer.material.color = gui.roleCards[(int)PawnRole].roleColor;
        }
        else if (image != null)
        {
            image.color = gui.roleCards[(int)PawnRole].roleColor;
        }
    }

    public void OnRoleChanged()
    {
        Debug.Log("Role changed");
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        Image image = GetComponent<Image>();
        if (renderer != null)
        {
            renderer.material.color = gui.roleCards[(int)PawnRole].roleColor;
        }
        else if (image != null)
        {
            image.color = gui.roleCards[(int)PawnRole].roleColor;
        }
    }



    public void Despawn(){
        CanMove = false;
        //Sends the despan command to the state authority
        RPC_Despawn();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_Despawn(){
        NetworkPlayer.LocalInstance.DespawnNetworkObject(Object);
    }
    // Called when the pawn is released and snapped to a city
    private void SnappedToCity(SnapInteractable interactable)
    {
        if(Object == null || !Object.HasStateAuthority) return;
        Debug.Log("Snapped to something");
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
                NetworkPlayer.LocalInstance.RPC_OnPawnSnap(0, PlayerModel.Position, endedInCity.city.cityID, 0);
                //RPC_OnPawnSnap(0, endedInCity.city.cityID, 0);
                //Timeline.theTimeline.addEvent(new PCharterEvent(endedInCity)); //0
                Despawn();
            }
            else
            {
                int distance = theGame.DistanceFromCity(PlayerModel.GetCurrentCity(), endedInCity.city.cityID);
                if (pGUI.pInEvent == EventState.CALLTOMOBILIZE)
                {
                    if (distance > 0 && distance <= 2)
                    {
                        //PERHAPS, RPC: RPC_OnPawnSnap(<eventTypeID>, PlayerModel (position), endedInCity.cityID)
                        // check the distance from the city (<=2) when doing a call to mobilize
                        NetworkPlayer.LocalInstance.RPC_OnPawnSnap(1, PlayerModel.Position, endedInCity.city.cityID, distance);
                        //RPC_OnPawnSnap(1, endedInCity.city.cityID, 0);
                        //Timeline.theTimeline.addEvent(new PMobilizeEvent(PlayerModel, endedInCity.city.cityID)); //1
                        Despawn();
                    }
                }
                else if (pGUI.ActionSelected == ActionTypes.Move)
                {
                    if (distance > 0 && distance <= pGUI.PlayerModel.ActionsRemaining)
                    {
                        NetworkPlayer.LocalInstance.RPC_OnPawnSnap(2, PlayerModel.Position, endedInCity.city.cityID, distance);
                        //RPC_OnPawnSnap(2, endedInCity.city.cityID, distance);
                        //Timeline.theTimeline.addEvent(new PMoveEvent(endedInCity.city.cityID, distance));
                        Despawn();
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
                    if (distance >= 0 && distance <= 2) // Distance >= 0  for the check, >0 is handled when checking if the action is to be accepted
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

        playerPosition = player.Position;
        PawnRole = player.Role;
        PlayerModel = player;

        initialCityID = PlayerModel.GetCurrentCity();
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        Image image = GetComponent<Image>();
        if (renderer != null)
        {
            renderer.material.color = gui.roleCards[(int)PawnRole].roleColor;
        }
        else if (image != null)
        {
            image.color = gui.roleCards[(int)PawnRole].roleColor;
        }
    }

    public void SetMoveable(bool moveable)
    {
        if (snapInteractor == null) return;

        CanMove = moveable;
        snapInteractor.gameObject.SetActive(moveable);
        if (moveable)
        {
            snapInteractor.OnObjectSnapped.AddListener(SnappedToCity);

            SnapInteractable initialInteractable = PlayerModel.GetCurrentCityScript().GetComponentInChildren<SnapInteractable>();
            snapInteractor.SetDefaultInteractable(initialInteractable);
            snapInteractor.SetTimeOutInteractable(initialInteractable);
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (IsInterfaceElement)
        //    theGame.CurrentPlayer.playerGui.PawnClicked(this);
    }
}

