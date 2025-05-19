using DG.Tweening;
using UnityEngine;
using static Game;
using static AnimationTemplates;

public class PDealCard : PlayerEvent, IIndirectEvent
{
    private float ANIMATIONDURATION = 0.5f / GameGUI.gui.AnimationTimingMultiplier;
    int cardToAdd = -1;
    private bool epidemicPopped = false;

    public PDealCard(Player player): base(player){}

    public override void Do(Timeline timeline)
    {
        if (theGame.PlayerCards.Count == 0)
        {
            Timeline.theTimeline.ClearPendingEvents();
            Timeline.theTimeline.AddEvent(new EGameOver(ENUMS.GameOverReasons.NoMorePlayerCards));
            return;
        }
        cardToAdd = theGame.PlayerCards.Pop();
        if (cardToAdd == 28)
        {
            Timeline.theTimeline.AddEvent(new EEpidemicInitiate());
            epidemicPopped = true;
        }
        else
        {
            _player.AddCardToHand(cardToAdd);
        }
        theGame.PlayerCardsDrawn++;
        theGame.actionCompleted = true;
    }

    public override float Act(bool qUndo = false)
    {
        if (cardToAdd == -1)
            return 0f;

        if (epidemicPopped)
            return 0f;

        GameObject card = game.AddPlayerCardToTransform(cardToAdd, gameGUI.AnimationCanvas.transform, false, _playerGui, gameGUI.PlayerDeck.transform);
        gameGUI.DrawBoard();
        Sequence sequence = HighlightCardAndMove(card, _playerGui.roleCard.transform, 3f, ANIMATIONDURATION);
        sequence.onComplete += () =>
        {
            Object.Destroy(card);
            _playerGui.Draw();
        };
        sequence.Play();
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        return $@" ""epidemicPopped"" : {epidemicPopped.ToString().ToLower()},
                    ""cardToAdd"" : {cardToAdd}
                ";
    }
}
