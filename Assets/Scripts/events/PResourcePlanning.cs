using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static Game;

internal class PResourcePlanning : PlayerEvent, IInitializableEvent
{
    
    private List<int> topPlayerCards;

    public PResourcePlanning(Player playerModel) : base(playerModel){}

    public override float Act(bool qUndo = false)
    {
        _playerGui.ClearSelectedAction();
        _playerGui.Draw();
        gameGUI.Draw();
        return 0;
    }

    public override void Do(Timeline timeline)
    {
        topPlayerCards = _playerGui.ResourcePlanningEventCardsIDs.ToList();
        _playerGui.ResourcePlanningEventCardsIDs.Reverse();
        foreach (var item in _playerGui.ResourcePlanningEventCardsIDs)
        {
            theGame.PlayerCards.Add(item);
        }
        _playerGui.ResourcePlanningEventCardsCities[0].transform.parent.parent.gameObject.SetActive(false);
        _playerGui.ResourcePlanningEventCardsIDs.Clear();
        _playerGui.ResourcePlanningEventCardSelected = -1;
        _playerGui.ChangeToInEvent(EventState.NOTINEVENT, false);
    }
    
    public override string GetLogInfo()
    {
        string cardIds = string.Join(", ", topPlayerCards);
        return $@" ""eventInitiator"" : ""{_player.Role}"",
                    ""topPlayerCards"" : [{cardIds}]
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["topPlayerCards"] is JArray jsonTopPlayerCards)
        {
            topPlayerCards = new List<int>();
            foreach (var token in jsonTopPlayerCards)
            {
                if (int.TryParse(token.ToString(), out int cardId))
                {
                    topPlayerCards.Add(cardId);
                }
                else
                {
                    Debug.LogWarning($"Failed to parse card ID: {token}");
                }
            }
        }
    }
}