using DG.Tweening;
using static Game;
using static AnimationTemplates;

public class PCallToMobilizeCardPlayed : PlayerEvent
{
    private float ANIMATIONDURATION = 1f;
    
    public PCallToMobilizeCardPlayed(Player playerThatTriggered) : base(playerThatTriggered)
    {
        
    }

    public override void Do(Timeline timeline)
    {
        theGame.ChangeToInEvent(EventState.CALLTOMOBILIZE);
        _player.RemoveCardInHand(CALLTOMOBILIZEINDEX, true);
    }

    public override float Act(bool qUndo = false)
    {
        //GameObject cardToAddObject = _playerGui.AddPlayerCardToTransform(CALLTOMOBILIZEINDEX, gameGUI.AnimationCanvas.transform, false);
        Sequence sequence = HighlightCardAndMove(_playerGui.GetCardInHand(CALLTOMOBILIZEINDEX), gameGUI.PlayerDeckDiscard.transform, 3f, ANIMATIONDURATION);
        /*sequence.onComplete += () =>
        {
            gameGUI.draw();
        };*/

        return base.Act(qUndo);
    }
    
    public override string GetLogInfo()
    {
        return $@" ""eventInitiator"" : ""{_player.Role}""
                ";
    }
}