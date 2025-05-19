using System;
using Newtonsoft.Json.Linq;

internal class PPilotFlyToCity : PlayerEvent, IInitializableEvent
{
    private int pilotCitySelected;
    private int initialOtherPlayerCity = -1;
    private int initialPlayerCity;
    private Player otherPlayer;

    public PPilotFlyToCity(Player player) : base(player) {}
    
    public PPilotFlyToCity(int pilotCitySelected, Player otherPlayer) : base(Game.theGame.CurrentPlayer)
    {
        this.pilotCitySelected = pilotCitySelected;
        this.otherPlayer = otherPlayer;
    }

    public override void Do(Timeline timeline)
    {
        initialPlayerCity = _player.GetCurrentCity();
        _player.UpdateCurrentCity(pilotCitySelected, true);
        if (otherPlayer != null)
        {
            initialOtherPlayerCity = otherPlayer.GetCurrentCity();
            otherPlayer.UpdateCurrentCity(pilotCitySelected, true);
        }
        _player.DecreaseActionsRemaining(1);
    }

    public override float Act(bool qUndo = false)
    {
        if (initialOtherPlayerCity != -1 && otherPlayer != null)
            Game.theGame.Cities[initialOtherPlayerCity].Draw();
        Game.theGame.Cities[pilotCitySelected].Draw();
        Game.theGame.Cities[initialPlayerCity].Draw();
        _playerGui.Draw();
        return 0;
    }

    public override string GetLogInfo()
    {
        return $@" ""pilotCitySelected"" : {pilotCitySelected},
                    ""otherPlayer"" : {(otherPlayer != null ? $@"""{otherPlayer.Role}""" : "null")}
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["pilotCitySelected"] is JValue jsonCitySelected)
        {
            pilotCitySelected = jsonCitySelected.Value<int>();
        }

        if (jsonData["otherPlayer"] is JValue jsonOtherPlayer)
        {
            string otherPlayerRoleString = jsonOtherPlayer.Value<string>();
            if (!string.IsNullOrEmpty(otherPlayerRoleString))
            {
                Player.Roles otherPlayerRole = Enum.Parse<Player.Roles>(otherPlayerRoleString);
                otherPlayer = PlayerList.GetPlayerByRole(otherPlayerRole);
            }
            else
            {
                otherPlayer = null;
            }
        }
    }
}
