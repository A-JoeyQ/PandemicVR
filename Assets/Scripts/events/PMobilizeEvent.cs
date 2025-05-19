using System;
using Newtonsoft.Json.Linq;

internal class PMobilizeEvent : PlayerEvent, IInitializableEvent
{
    private int newCityID;
    private int oldCityID;
    
    public PMobilizeEvent(Player player) : base(player)
    {
        
    }
    public PMobilizeEvent(Player playerModel, int cityID): base(playerModel)
    {
        oldCityID = _player.GetCurrentCity();
        newCityID = cityID;
    }

    public override float Act(bool qUndo = false)
    {
        Game.theGame.Cities[newCityID].Draw();
        Game.theGame.Cities[oldCityID].Draw();
        return 0;
    }

    public override void Do(Timeline timeline)
    {
        _player.playerGui.DestroyMovingPawn();
        if(newCityID != oldCityID) //TODO: check this to fix the MovingPawn bug
            _player.UpdateCurrentCity(newCityID, true);
        _playerGui.callToMobilizeExecuted = true;
        _player.playerGui.ChangeToInEvent(Game.EventState.NOTINEVENT, true);
    }

    public override string GetLogInfo()
    {
        return $@" ""eventInitiator"" : ""{_player.Role}"",
                    ""newCity"" : {newCityID},
                    ""oldCity"" : {oldCityID}
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["newCity"] is JValue jsonNewCity)
        {
            newCityID = jsonNewCity.Value<int>();
        }
        
        if (jsonData["oldCity"] is JValue jsonOldCity)
        {
            oldCityID = jsonOldCity.Value<int>();
        }
        
    }

}