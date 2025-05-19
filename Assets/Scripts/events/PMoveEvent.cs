using Newtonsoft.Json.Linq;

internal class PMoveEvent : PlayerEvent, IInitializableEvent
{
    private int newCityID;
    private int oldCityID;
    private int numberOfActionsSpent;
    float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;

    public PMoveEvent(Player player) : base (player) {}
    public PMoveEvent(int cityID, int numberOfActionsSpent): base(Game.theGame.CurrentPlayer)
    {
        this.oldCityID = _player.GetCurrentCity();
        this.newCityID = cityID;
        this.numberOfActionsSpent = numberOfActionsSpent;
    }

    public override void Do(Timeline timeline)
    {
        _player.UpdateCurrentCity(newCityID, true);
        _player.DecreaseActionsRemaining(numberOfActionsSpent);
    }

    public override float Act(bool qUndo = false)
    {
        Game.theGame.Cities[newCityID].Draw();
        Game.theGame.Cities[oldCityID].Draw();
        _playerGui.ClearSelectedAction();
        _playerGui.Draw();
        return 0;
    }

    public override string GetLogInfo()
    {
        return $@" ""newCity"" : {newCityID},
                    ""oldCity"" : {oldCityID},
                    ""numberOfActionsSpent"" : {numberOfActionsSpent}
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
        
        if (jsonData["numberOfActionsSpent"] is JValue jsonNumberOfActionsSpent)
        {
            numberOfActionsSpent = jsonNumberOfActionsSpent.Value<int>();
        }
    }
}