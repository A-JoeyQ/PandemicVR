using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

public class AoIEventCard : AreaOfInterest
{

    private EventCard eventCard;
    private Player player;

    public override string GetAoILog(){
        eventCard = GetComponent<EventCardDisplay>().EventCardData;
        PlayerGUI playerGUI = GetComponentInParent<PlayerGUI>();
        if (playerGUI != null)
        {
            return "[EventCard]" + eventCard.eventName + ",(" + playerGUI.PlayerModel.Name + "," + playerGUI.PlayerModel.Role + ")";
        }
        return "[EventCard]" + eventCard.eventName;
    }
    public override AoIType GetAoIType(){
        return AoIType.EventCard;
    }

    public override string GetJsonLog(){
        PlayerGUI playerGUI = GetComponentInParent<PlayerGUI>();
        if(playerGUI != null){
            eventCard = GetComponent<EventCardDisplay>().EventCardData;
            player = playerGUI.PlayerModel;
            return $@" 
                    ""type"" : ""EventCard"",
                    ""event"" : ""{eventCard.eventName}"",
                    ""eventID"" : ""{eventCard.eventID}"",
                    ""player"" : ""{player.Position}"",
                    ""playerRole"" : ""{player.Role}""
                ";
        }
        return $@" 
                    ""type"" : ""EventCard"",
                    ""event"" : ""{eventCard.eventName}"",
                    ""eventID"" : ""{eventCard.eventID}""
                ";
        
    }
}
