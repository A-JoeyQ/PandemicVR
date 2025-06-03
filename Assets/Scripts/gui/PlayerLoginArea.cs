using UnityEngine;
using System;
using UnityEngine.UI;
using static ENUMS;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Fusion;

public class PlayerLoginArea : MonoBehaviour
{
    private const string ChooseRoleTextContent = "Choose a role for this game.";
    public MainMenu MainMenu;
    public TMPro.TextMeshProUGUI ChooseRoleText;
    public TMPro.TextMeshProUGUI playerNameText;
    public GameObject prefabRolecard;
    public GameObject horizontalLayoutRoles;

    public int Position;

    private GameObject[] roleCards;
    private RoleCardDisplay[] roleCardsDisplay;

    private string playerName;

    public string PlayerName
    {
        get { return playerName; }
        set {
            playerName = value;
            playerNameText.text = playerName;
        }
    }


    Player.Roles? _role;

    public Player.Roles? Role
    {
        get { return _role; }
        set
        {
            if(_role != null)
            {
                roleCardsDisplay[(int)_role].background.GetComponent<Outline>().enabled = false;
            }
            if(value != null)
            {
                roleCardsDisplay[(int)value].background.GetComponent<Outline>().enabled = true;
                ChooseRoleText.text = "You have chosen " + (value.GetDescription()) + " as your role for this game.";
            }
            _role = value;

        }
    }

    // Use this for initialization
    void Start()
    {
        //resetPlayerLoginArea();
    }

    public void ResetPlayerLoginArea()
    {
        Role = null;
        PlayerName = MainMenu.PlayerNames[Position];
        ChooseRoleText.text = ChooseRoleTextContent;
        ChangePlayerAreaColor(null, GameGUI.gui.playerUIOpacity);
        roleCards = new GameObject[Enum.GetValues(typeof(Player.Roles)).Length];




        roleCardsDisplay = new RoleCardDisplay[roleCards.Length];
        for (int i = 0; i < roleCards.Length; i++)
        {
            int currentValue = i;
            roleCards[i] = Instantiate(prefabRolecard, horizontalLayoutRoles.transform);
            roleCardsDisplay[i] = roleCards[i].GetComponent<RoleCardDisplay>();
            roleCardsDisplay[i].RoleCardData = GameGUI.gui.roleCards[i];
            // This needs to be modified for VR ray interaction
            roleCards[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                OnRoleClicked((Player.Roles)currentValue);
            });
        }
    }

    public void ChangePlayerAreaColor(Player.Roles? role, float alpha)
    {
        Color color = Color.gray;
        if (role != null)
        {
            foreach (var roleCard in roleCardsDisplay)
            {
                if (roleCard.RoleCardData.roleName == role.GetDescription())
                {
                    color = roleCard.RoleCardData.roleColor;
                }
            }
        }
        color.a = alpha;
        gameObject.GetComponent<UnityEngine.UI.Image>().color = color;
    }


    public void HandleOnRoleClicked(Player.Roles roleToChangeTo)
    {
        if (Role == roleToChangeTo)
        {
            Role = null;
            MainMenu.AddRole(roleToChangeTo);
            ChooseRoleText.text = ChooseRoleTextContent;
            MainMenu.UpdateRoles();
            ChangePlayerAreaColor(null, GameGUI.gui.playerUIOpacity);
        }
        else
        if (MainMenu.FreeRoles.Contains(roleToChangeTo))
        {
            //MainMenu.FreeRoles.Remove(roleToChangeTo);
            MainMenu.RemoveRole(roleToChangeTo);
            if (Role != null)
            {
                MainMenu.AddRole(Role.Value);
                //MainMenu.FreeRoles.Add(Role.Value);
            }
            Role = roleToChangeTo;
            ChangePlayerAreaColor(roleToChangeTo, GameGUI.gui.playerUIOpacity);
            MainMenu.UpdateRoles();
        }
    }
    public void OnRoleClicked(Player.Roles roleToChangeTo)
    {

        
        NetworkPlayer.LocalInstance.RPC_OnRoleClicked(roleToChangeTo, Position);
    }

    public bool IsPlaying()
    {
        return Role != null;
    }

    internal void UpdateRole(HashSet<Player.Roles> freeRoles)
    {
        foreach (Player.Roles role in Enum.GetValues(typeof(Player.Roles)))
        {
            if(role != Role)
            {
                if(freeRoles.Contains(role))
                    roleCards[(int)role].gameObject.SetActive(true);
                else
                    roleCards[(int)role].gameObject.SetActive(false);
            }    
        }
    }
}
