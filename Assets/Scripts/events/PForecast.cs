using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static Game;

internal class PForecast : PlayerEvent, IInitializableEvent
{
    private List<int> topInfectionCards;
    public PForecast(Player playerModel) : base(playerModel) { }

    public override float Act(bool qUndo = false)
    {
        _playerGui.ClearSelectedAction();
        _playerGui.Draw();
        gameGUI.Draw();
        return 0;
    }

    public override void Do(Timeline timeline)
    {
        if (topInfectionCards != null && topInfectionCards.Count > 0) // When topInfectionCards is initialized from a log file.
        {
            _playerGui.ForeCastEventCardsIDs = topInfectionCards.ToList();
        }
        else
        {
            topInfectionCards = _playerGui.ForeCastEventCardsIDs.ToList();
        }
        _playerGui.ForeCastEventCardsIDs.Reverse();
        foreach (var item in _playerGui.ForeCastEventCardsIDs)
        {
            theGame.InfectionCards.Add(item);
        }

        _playerGui.ForeCastEventCards[0].transform.parent.gameObject.SetActive(false);
        _playerGui.ForeCastEventCardsIDs.Clear();
        _playerGui.ForeCastEventCardSelected = -1;
        _playerGui.ChangeToInEvent(EventState.NOTINEVENT, false);
    }

    public override string GetLogInfo()
    {
        string cardIds = string.Join(", ", topInfectionCards);
        return $@" ""eventInitiator"" : ""{_player.Role}"",
                    ""topInfectionCards"" : [{cardIds}]
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["topInfectionCards"] is JArray jsonTopInfectionCards)
        {
            topInfectionCards = new List<int>();
            foreach (var token in jsonTopInfectionCards)
            {
                if (int.TryParse(token.ToString(), out int cardId))
                {
                    topInfectionCards.Add(cardId);
                }
                else
                {
                    Debug.LogWarning($"Failed to parse card ID: {token}");
                }
            }
        }
    }
}