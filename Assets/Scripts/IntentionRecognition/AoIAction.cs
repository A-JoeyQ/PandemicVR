using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
public class AoIAction : AreaOfInterest
{
    [SerializeField] public ActionType actionType;
    private Player player;
    private string action = "None"; //Instead enum I assign to it.
    
    public override string GetAoILog(){
        action = gameObject.name;
        PlayerGUI playerGUI = gameObject.GetComponentInParent<PlayerGUI>();
        if(playerGUI == null)
        {
            return "[Action]" + actionType + ",(PlayerGUI not found)";
        }
        player = playerGUI.PlayerModel;
        return "[Action]" + actionType + ",("+player.Name + "," + player.Role + ")";
    }
    public override AoIType GetAoIType(){
        return AoIType.Action;
    }

    public override string GetJsonLog(){
        action = gameObject.name;
        
        PlayerGUI playerGUI = gameObject.GetComponentInParent<PlayerGUI>();
        if (playerGUI == null)
        {
            return $@" 
                    ""type"" : ""Action"",
                    ""action"" : ""{actionType}"",
                    ""player"" : ""Not found"",
                    ""playerRole"" : ""Not found""
                ";
        }
        player = playerGUI.PlayerModel;
        return $@" 
                    ""type"" : ""Action"",
                    ""action"" : ""{actionType}"",
                    ""player"" : {player.Position},
                    ""playerRole"" : ""{player.Role}""
                ";
    }
}
