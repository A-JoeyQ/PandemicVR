using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static ENUMS;
using static Game;

public class PMobileHospitalEvent : PlayerEvent, IInitializableEvent
{
    private City city;
    private VirusName virusName;

    public PMobileHospitalEvent(Player player) : base(player)
    {
        
    }
    public PMobileHospitalEvent(Player player, City city, ENUMS.VirusName virusName) : base(player)
    {
        this.city = city;
        this.virusName = virusName;

    }

    public override float Act(bool qUndo = false)
    {
        city.Draw();
        gameGUI.DrawBoard();
        foreach (Player player in PlayerList.GetAllPlayers())
        {
            player.playerGui.Draw();
        }
        return 0;
    }

    public override void Do(Timeline timeline)
    {
        /*Debug.Log("Player of Mobile Hospital = " + game.CurrentPlayer.Name);
        Debug.Log("In the city :" + game.CurrentPlayer.GetCurrentCity() + " cityID=" + city.city.cityID + " theGame.InEventCard = " + theGame.InEventCard);*/

        if (city == null)
        {
            city = theGame.CurrentPlayer.GetCurrentCityScript();
        }
        if (theGame.MobileHospitalInExecution && city.city.cityID == game.CurrentPlayer.GetCurrentCity())
        {
            //Debug.Log("In the city :" + game.CurrentPlayer.GetCurrentCity() + " city.getInstanceID=" + city.GetInstanceID());
            city.IncrementNumberOfCubes((VirusName)virusName, -1);
            game.IncrementNumberOfCubesOnBoard((VirusName)virusName, 1);

            /*theGame.MobileHospitalPlayer.playerGui.ChangeToInEvent(EventState.NOTINEVENT);
            theGame.ChangeToInEvent(EventState.NOTINEVENT);*/
            theGame.RemovePlayersWait();
            //theGame.CubeClicked(city, virusName);
            theGame.MobileHospitalInExecution = false;
            
            if (theGame.MobileHospitalPlayer.playerGui.callToMobilizePending && !theGame.MobileHospitalPlayer.playerGui.callToMobilizeExecuted )
            {
                //theGame.MobileHospitalPlayer.playerGui.ChangeToInEvent(EventState.CALLTOMOBILIZE);
                theGame.ChangeToInEvent(EventState.CALLTOMOBILIZE, theGame.MobileHospitalPlayer);
            }
            
            if (theGame.CurrentPlayer.ActionsRemaining == 0)
            {
                theGame.SetCurrentGameState(GameState.DRAWPLAYERCARDS);
            }

        }
    }

    public override string GetLogInfo()
    {
        return $@" ""virusName"" : ""{virusName}""
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["virusName"] is JValue jsonVirusName)
        {
            if (Enum.TryParse(jsonVirusName.Value<string>(), out VirusName parsedVirusName))
            {
                virusName = parsedVirusName;
            }
            else
            {
                Debug.LogError($"Invalid VirusName: {jsonVirusName}");
            }
        }
    }
}