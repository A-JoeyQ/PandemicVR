using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Newtonsoft.Json;

public class Fixation
{
    public float duration;
    public float startTime;
    public float endTime;
    public AreaOfInterest aoi;

    public Fixation(float duration, float startTime, float endTime, AreaOfInterest aoi)
    {
        this.duration = duration;
        this.startTime = startTime;
        this.endTime = endTime;
        this.aoi = aoi;
    }

    private string bool2json(bool b)
    {
        return b ? "true" : "false";
    }
    public string GetFixationLog()
    {
        //actionsJson = ;
        
        string fixationLog = $@"
                    ""startTime"" : {startTime},
                    ""endTime"" : {endTime},
                    ""duration"" : {duration},
                    ""cures"" : {{
                        ""red"" : {bool2json(Game.theGame.RedCure)},
                        ""blue"" : {bool2json(Game.theGame.BlueCure)},
                        ""yellow"" : { bool2json(Game.theGame.YellowCure)}
                    }},
                    {(Game.theGame.CurrentPlayer != null ? (
                        $@"""currentPlayer"" : {{
                            ""role"" : ""{Game.theGame.CurrentPlayer.Role}"",
                            ""name"" : ""{Game.theGame.CurrentPlayer.Name}"",
                            ""currentCity"" : ""{Game.theGame.CurrentPlayer.GetCurrentCityScript().city.cityName}"",
                            ""currentCityID"" : {Game.theGame.CurrentPlayer.GetCurrentCity()},
                            ""cardsRed"" : {Game.theGame.CurrentPlayer.RedCardsInHand.Count},
                            ""cardsBlue"" : {Game.theGame.CurrentPlayer.BlueCardsInHand.Count},
                            ""cardsYellow"" : {Game.theGame.CurrentPlayer.YellowCardsInHand.Count},
                            ""actions"" : {
                                JsonConvert.SerializeObject(Game.theGame.CurrentPlayer.playerGui.availableActions())
                            }
                        }},") : null )}
                    
                ";
        
        string aoiLog = aoi.GetJsonLog();

        return $@"{{
                {fixationLog}
                {aoiLog}
                }},";
    }
}
