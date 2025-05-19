using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

public class AoICityCard : AreaOfInterest
{

    private CityCard cityCard;
    private Player player;

    void Start(){
        cityCard = GetComponent<CityCardDisplay>().CityCardData;
    }

    public override string GetAoILog(){
        PlayerGUI playerGUI = GetComponentInParent<PlayerGUI>();
        if(playerGUI != null)
        {
            return "[CityCard]" + cityCard.cityName + "," + cityCard.virusInfo.virusName + ",(" + playerGUI.PlayerModel.Name + "," + playerGUI.PlayerModel.Role + ")";
        }
        if(cityCard != null)
        {
            return "[CityCard]" + cityCard.cityName + cityCard.virusInfo.virusName;
        }
        return "[CityCard]" + "CityCard not found";
        
    }
    public override AoIType GetAoIType(){
        return AoIType.CityCard;
    }

    public override string GetJsonLog(){
        PlayerGUI playerGUI = GetComponentInParent<PlayerGUI>();
        if(playerGUI != null){
            player = playerGUI.PlayerModel;
            return $@" 
                    ""type"" : ""CityCard"",
                    ""city"" : ""{cityCard.cityName}"",
                    ""virus"" : ""{cityCard.virusInfo.virusName}"",
                    ""player"" : ""{player.Position}"",
                    ""playerRole"" : ""{player.Role}""
                ";
        }
        return $@" 
                    ""type"" : ""CityCard"",
                    ""city"" : ""{cityCard.cityName}"",
                    ""virus"" : ""{cityCard.virusInfo.virusName}""
                ";
        
    }
}
