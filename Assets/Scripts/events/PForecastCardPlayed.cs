using DG.Tweening;
using System;
using static Game;
using static AnimationTemplates;
using System.Collections.Generic;

public class PForecastCardPlayed : PlayerEvent
{
    public new float ANIMATIONDURATION = 1f;

    public List<int> cardsToSort = new List<int>();

    public PForecastCardPlayed(Player player) : base(player){}

    public override void Do(Timeline timeline)
    {
        int numberOfCardsInDeck = theGame.InfectionCards.Count;
        if (numberOfCardsInDeck > 0)
        {
            _player.playerGui.ChangeToInEvent(EventState.FORECAST, false);
            _player.RemoveCardInHand(FORECASTINDEX, true);

            for (int i = 0; i < Math.Min(6, numberOfCardsInDeck); i++)
            {
                cardsToSort.Add(theGame.InfectionCards.Pop());
                //_player.playerGui.ForeCastEventCardsIDs.Add(theGame.InfectionCards.Pop());
            }
            
            for(int i = 0; i < cardsToSort.Count; i++)
            {
                _player.playerGui.ForeCastEventCardsIDs.Add(cardsToSort[i]);
            }
            //_player.playerGui.ForeCastEventCardSelected = _player.playerGui.ForeCastEventCardsIDs[0];

        }
    }

    public override float Act(bool qUndo = false)
    {
        _playerGui.EnableContextButtons(false, false, false, false, false, false);
        Sequence sequence = HighlightCardAndMove(_playerGui.GetCardInHand(FORECASTINDEX), gameGUI.PlayerDeckDiscard.transform, 3f, ANIMATIONDURATION);
        sequence.onComplete += () =>
        {
            _player.playerGui.ForeCastEventCardSelected = _player.playerGui.ForeCastEventCardsIDs[0];
            _player.playerGui.ActionsContainer.SetActive(false);
            _player.playerGui.Draw();
            gameGUI.Draw();
        };

        return base.Act(qUndo);
    }
    
    public override string GetLogInfo()
    {
        string cardIds = string.Join(", ", cardsToSort);
        return $@" ""eventInitiator"" : ""{_player.Role}"",
                    ""cardsToSort"" : [{cardIds}]
                ";
    }
}