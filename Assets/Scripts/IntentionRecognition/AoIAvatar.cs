using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
public class AoIAvatar : AreaOfInterest
{
    private NetworkPlayer networkPlayer;
    public override string GetAoILog()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        if (networkPlayer == null)
        {
            return "Avatar not found";
        }
        PlayerGUI playerGUI = GetBoardPlayer(networkPlayer);
        if(playerGUI == null)
        {
            return "PlayerGUI not found for avatar";
        }
        return "[Player]" + playerGUI.PlayerModel.Name + "," + playerGUI.PlayerModel.Role;
    }
    public override AoIType GetAoIType()
    {
        return AoIType.Avatar;
    }

    private PlayerGUI GetBoardPlayer(NetworkPlayer networkPlayer)
    {
        return GameGUI.PlayerPadForPosition(networkPlayer.startingPosition);
    }
    public override string GetJsonLog()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
        if (networkPlayer == null)
        {
            return $@" 
                    ""type"" : ""Avatar""
                ";
        }
        PlayerGUI playerGUI = GetBoardPlayer(networkPlayer);
        if (playerGUI == null)
        {
            return $@" 
                    ""type"" : ""Avatar"",
                    ""position"" : ""{networkPlayer.startingPosition}""
                ";
        }
        return $@" 
                    ""type"" : ""Avatar"",
                    ""position"" : ""{networkPlayer.startingPosition}"",
                    ""name"" : ""{playerGUI.PlayerModel.Name}"",
                    ""role"" : ""{playerGUI.PlayerModel.Role}""
                ";
    }
}
